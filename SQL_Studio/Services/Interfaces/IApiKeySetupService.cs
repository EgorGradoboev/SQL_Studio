namespace SQL_Studio.Services.Interfaces
{
    public interface IApiKeySetupService
    {
        bool IsConfigured { get; }
        bool PromptAndSave();
    }
}
