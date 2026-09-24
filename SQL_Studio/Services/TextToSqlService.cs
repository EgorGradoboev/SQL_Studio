using Anthropic;
using Anthropic.Models.Messages;
using SQL_Studio.Services.Interfaces;

namespace SQL_Studio.Services
{
    public class TextToSqlService : ITextToSqlService
    {
        private const string Model = "claude-opus-5";

        private const string SystemPrompt = """
            You are a PostgreSQL expert. The user asks questions about their database in plain language,
            possibly as a follow-up to earlier questions in this conversation (e.g. "now only for last month").
            For each question, write exactly one read-only SQL query (SELECT or WITH ... SELECT) that answers it,
            using only tables and columns from the schema below.
            Reply with the SQL only: no markdown fences, no explanations.
            If the question cannot be answered from this schema, reply with a single SQL comment
            starting with "-- " that explains why.

            Schema:
            """;

        /// <summary>How many previous turns are replayed to the model. Keeps the request from growing without bound.</summary>
        private const int MaxHistoryTurns = 6;

        private readonly ISchemaProvider _schemaProvider;
        private readonly IApiKeyStore _apiKeyStore;
        private AnthropicClient? _client;
        private string? _clientApiKey;

        public TextToSqlService(ISchemaProvider schemaProvider, IApiKeyStore apiKeyStore)
        {
            _schemaProvider = schemaProvider;
            _apiKeyStore = apiKeyStore;
        }

        public async Task<string> GenerateSqlAsync(IConnectionFactoryService connectionFactory, string databaseName,
            IReadOnlyList<SqlChatTurn> history, string question, CancellationToken cancellationToken)
        {
            string apiKey = _apiKeyStore.GetApiKey()
                ?? throw new InvalidOperationException("Anthropic API key is not set.");
            string schema = await _schemaProvider.GetSchemaTextAsync(connectionFactory, databaseName, cancellationToken);
            if (_client == null || _clientApiKey != apiKey)
            {
                _client = new AnthropicClient { ApiKey = apiKey };
                _clientApiKey = apiKey;
            }

            var messages = new List<MessageParam>();
            foreach (var turn in history.TakeLast(MaxHistoryTurns))
            {
                messages.Add(new() { Role = Role.User, Content = turn.Question });
                messages.Add(new() { Role = Role.Assistant, Content = turn.GeneratedSql });
            }
            messages.Add(new() { Role = Role.User, Content = question });

            var response = await _client.Messages.Create(new MessageCreateParams
            {
                Model = Model,
                MaxTokens = 8000,
                System = $"{SystemPrompt}\n{schema}",
                OutputConfig = new OutputConfig { Effort = Effort.Medium },
                Messages = messages,
            }, cancellationToken);

            if (response.StopReason == "refusal")
            {
                throw new InvalidOperationException("The model declined to answer this question.");
            }

            string sql = string.Concat(response.Content
                .Select(block => block.Value)
                .OfType<TextBlock>()
                .Select(block => block.Text));
            return StripCodeFence(sql);
        }

        private static string StripCodeFence(string text)
        {
            text = text.Trim();
            if (!text.StartsWith("```"))
            {
                return text;
            }
            int firstNewLine = text.IndexOf('\n');
            int lastFence = text.LastIndexOf("```");
            if (firstNewLine < 0 || lastFence <= firstNewLine)
            {
                return text;
            }
            return text[(firstNewLine + 1)..lastFence].Trim();
        }
    }
}
