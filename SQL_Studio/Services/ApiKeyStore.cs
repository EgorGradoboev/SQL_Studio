using SQL_Studio.Services.Interfaces;

namespace SQL_Studio.Services
{
    public class ApiKeyStore : IApiKeyStore
    {
        private const string EnvironmentVariableName = "ANTHROPIC_API_KEY";

        public string? GetApiKey()
        {
            string? apiKey = Environment.GetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable(EnvironmentVariableName, EnvironmentVariableTarget.User);
            }
            return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey;
        }

        public void SaveApiKey(string apiKey)
        {
            apiKey = apiKey.Trim();
            // User scope persists it for new processes; Process scope makes it visible to this running app.
            Environment.SetEnvironmentVariable(EnvironmentVariableName, apiKey, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(EnvironmentVariableName, apiKey, EnvironmentVariableTarget.Process);
        }
    }
}
