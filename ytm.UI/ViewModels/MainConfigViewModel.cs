using ytm.Services;

namespace ytm.UI.ViewModels
{
    public class MainConfigViewModel : ViewModelBase
    {
        private readonly IConfigProvider _configProvider;

        public MainConfigViewModel(/*IConfigProvider configProvider*/)
        {
            //_configProvider = configProvider;
        }

        public string Mp3Path => _configProvider.GetConfig().Mp3Root;
    }
}