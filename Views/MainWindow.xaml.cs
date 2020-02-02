using ProxyServer.ViewModels;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ProxyServer
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

        public void UpdateScrollBar(object sender, DependencyPropertyChangedEventArgs e)
        {
            var src = listBox.Items.SourceCollection as INotifyCollectionChanged;
            src.CollectionChanged += (obj, args) => { listBox.Items.MoveCurrentToLast(); listBox.ScrollIntoView(listBox.Items.CurrentItem); };
        }
    }
}
