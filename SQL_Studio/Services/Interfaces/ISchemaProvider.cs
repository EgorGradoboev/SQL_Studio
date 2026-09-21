namespace SQL_Studio.Services.Interfaces
{
    public interface ISchemaProvider
    {
        Task<string> GetSchemaTextAsync(IConnectionFactoryService connectionFactory, string databaseName,
            CancellationToken cancellationToken);
    }
}
