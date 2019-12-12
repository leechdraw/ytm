using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using ytm.Services;

namespace ytm.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private MainConfigViewModel _configViewModel;

        public MainWindowViewModel()
        {
            InitChildren();
        }

        public MainConfigViewModel MainConfig => _configViewModel;
        
        public IConfigProvider ConfigProvider { get; set; }

        private void InitChildren()
        {
            _configViewModel = new MainConfigViewModel(() => ConfigProvider);
        }
    }
}