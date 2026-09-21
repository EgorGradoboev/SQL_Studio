namespace SQL_Studio.Services.Interfaces
{
    public interface ITextToSqlService
    {
        Task<string> GenerateSqlAsync(IConnectionFactoryService connectionFactory, string databaseName,
            string question, CancellationToken cancellationToken);
    }
}
