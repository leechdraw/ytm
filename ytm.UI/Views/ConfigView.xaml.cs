using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ytm.UI
{
    public class ConfigView : UserControl
    {
        public ConfigView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}