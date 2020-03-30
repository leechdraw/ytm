using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using ytm.Services;
using ytm.UI.Models;

namespace ytm.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private MainConfigViewModel _configViewModel;
        private readonly List<LogItemModel> _logs = new List<LogItemModel>();

        public MainWindowViewModel()
        {
            InitChildren();
            _logs.Add(new LogItemModel
            {
                Text = "Starting",
                Type = LogItemType.Info
            });
        }

        public MainConfigViewModel MainConfig => _configViewModel;

        public List<LogItemModel> Logs => _logs;
        
        public IConfigProvider ConfigProvider { get; set; }

        private void InitChildren()
        {
            _configViewModel = new MainConfigViewModel(() => ConfigProvider);
        }
    }
}