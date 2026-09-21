namespace SQL_Studio.Services.Interfaces
{
    public interface IApiKeyStore
    {
        string? GetApiKey();
        void SaveApiKey(string apiKey);
    }
}
