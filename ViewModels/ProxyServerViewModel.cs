using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ProxyServer.Proxy;

namespace ProxyServer.ViewModels
{
    public class ProxyServerViewModel : ViewModelBase
    {
        private Server server;

        private readonly CommandDelegate startStopProxyCommand;
        private readonly CommandDelegate clearLog;
        public ICommand StartStopProxyCommand => startStopProxyCommand;
        public ICommand ClearLogCommand => clearLog;

        public ProxyServerViewModel()
        {
            server = new Server();
            startStopProxyCommand = new CommandDelegate(OnStartStopProxy, CanStartStopProxy);
            clearLog = new CommandDelegate(OnClearLog, CanClearLog);
        }

        private bool CanStartStopProxy(object commandParameter) => true;
        public void OnClearLog(object commandParameter) => server.ClearLog();
        private bool CanClearLog(object commandParameter) => true;

        public bool IsRunning
        {
            get => server.running;
            set => SetProperty(server.running, value, () => server.running = value);
        }

        public int ProxyPort
        {
            get => server.port;
            set => SetProperty(server.port, value, () => server.port = value);
        }

        public int ProxyCacheTimeOutInSeconds
        {
            get => server.cacheTimeOut;
            set => SetProperty(server.cacheTimeOut, value, () => server.cacheTimeOut = value);
        }

        public int ProxyBufferSize
        {
            get => server.bufferSize;
            set => SetProperty(server.bufferSize, value, () => server.bufferSize = value);
        }

        public bool ProxyServeFromCache
        {
            get => server.serveFromCache;
            set => SetProperty(server.serveFromCache, value, () => server.serveFromCache = value);
        }

        public bool ProxyAuthenticationRequired
        {
            get => server.authRequired;
            set => SetProperty(server.authRequired, value, () => server.authRequired = value);
        }

        public bool ProxyHideUserAgentEnabled
        {
            get => server.hideUserAgent;
            set => SetProperty(server.hideUserAgent, value, () => server.hideUserAgent = value);
        }

        public bool ProxyFilterContentEnabled
        {
            get => server.filterContent;
            set => SetProperty(server.filterContent, value, () => server.filterContent = value);
        }

        public bool ProxyLogRequest
        {
            get => server.logRequest;
            set => SetProperty(server.logRequest, value, () => server.logRequest = value);
        }

        public bool ProxyLogResponse
        {
            get => server.logResponse;
            set => SetProperty(server.logResponse, value, () => server.logResponse = value);
        }

        public ObservableCollection<ListBoxItem> Log
        {
            get => server.Log;
        }

        private async void OnStartStopProxy(object commandParameter)
        {
            if (!server.running)
            {
                await Task.Run(() => server.Start());
            }
            else
            {
                await Task.Run(() => server.Stop());
            }
        }
    }
}
