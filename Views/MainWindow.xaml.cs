using ProxyServer_NOTS.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ProxyServer_NOTS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ProxyServerViewModel proxyServerViewModel;

        public MainWindow()
        {
            proxyServerViewModel = new ProxyServerViewModel();
            DataContext = proxyServerViewModel;

            InitializeComponent();
        }

        public void isNumber(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            Regex regex = new Regex("[^0-9]+");
            textCompositionEventArgs.Handled = regex.IsMatch(textCompositionEventArgs.Text);
        }
    }
}
