namespace SQL_Studio.Services.Interfaces
{
    /// <summary>One completed round of the AI dialog: the user's question and the SQL it produced.</summary>
    public sealed record SqlChatTurn(string Question, string GeneratedSql);

    public interface ITextToSqlService
    {
        Task<string> GenerateSqlAsync(IConnectionFactoryService connectionFactory, string databaseName,
            IReadOnlyList<SqlChatTurn> history, string question, CancellationToken cancellationToken);
    }
}
