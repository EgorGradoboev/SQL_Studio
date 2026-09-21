using SQL_Studio.Services.Interfaces;
using System.Windows;

namespace SQL_Studio.Services
{
    public class ApiKeySetupService : IApiKeySetupService
    {
        private readonly IApiKeyStore _apiKeyStore;

        public ApiKeySetupService(IApiKeyStore apiKeyStore)
        {
            _apiKeyStore = apiKeyStore;
        }

        public bool IsConfigured => _apiKeyStore.GetApiKey() != null;

        public bool PromptAndSave()
        {
            var window = new ApiKeyWindow();
            if (Application.Current.MainWindow is { IsVisible: true } owner)
            {
                window.Owner = owner;
            }
            if (window.ShowDialog() != true || window.ApiKey == null)
            {
                return false;
            }
            _apiKeyStore.SaveApiKey(window.ApiKey);
            return true;
        }
    }
}
