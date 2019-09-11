using ProxyServer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProxyServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ProxyServerViewModel _proxyServerViewModel;

        public MainWindow()
        {
            _proxyServerViewModel = new ProxyServerViewModel();
            DataContext = _proxyServerViewModel;

            InitializeComponent();
        }

        // Validate input for numbers
        public void CheckIfNumber(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            Regex isNumberRegex = new Regex("[^0-9]+");
            textCompositionEventArgs.Handled = isNumberRegex.IsMatch(textCompositionEventArgs.Text);
        }
    }
}
