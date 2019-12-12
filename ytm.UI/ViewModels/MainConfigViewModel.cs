using System;
using JetBrains.Annotations;
using ytm.Services;

namespace ytm.UI.ViewModels
{
    public class MainConfigViewModel : ViewModelBase
    {
        private readonly Func<IConfigProvider> _getProvider;

        public MainConfigViewModel([NotNull] Func<IConfigProvider> getProvider)
        {
            _getProvider = getProvider ?? throw new ArgumentNullException(nameof(getProvider));
        }

        public string Mp3Folder => _getProvider()?.GetConfig(true)?.FfMpeg;
    }
}