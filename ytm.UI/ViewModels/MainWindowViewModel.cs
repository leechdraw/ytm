using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using ytm.Services;

namespace ytm.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IConfigProvider _configProvider;
        private MainConfigViewModel _configViewModel;

        public MainWindowViewModel(/*[NotNull] IConfigProvider configProvider*/)
        {
            /*_configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));*/
            InitChildren();
        }

        public string Greeting => "Hello World!";

        public MainConfigViewModel MainConfig => _configViewModel;

        private void InitChildren()
        {
            _configViewModel = new MainConfigViewModel(/*_configProvider*/);
        }
    }
}