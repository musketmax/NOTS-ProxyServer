using ProxyServer_NOTS.Proxy;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProxyServer_NOTS.ViewModels
{
    public class ProxyServerViewModel : ViewModelBase
    {
        private ProxyServer proxyServer;

        private readonly CommandDelegate startStopProxy;
        private readonly CommandDelegate clearLog;
        private readonly CommandDelegate clearCache;
        public ICommand startStopProxyCommand => startStopProxy;
        public ICommand clearLogCommand => clearLog;
        public ICommand clearCacheCommand => clearCache;

        public ProxyServerViewModel()
        {
            proxyServer = new ProxyServer();
            startStopProxy = new CommandDelegate(onStartStopProxy, canStartStopProxy);
            clearLog = new CommandDelegate(onClearLog, canClearLog);
            clearCache = new CommandDelegate(onClearCache, canClearCache);
        }

        private bool canStartStopProxy(object commandParameter) => true;
        private bool canClearLog(object commandParameter) => true;
        private bool canClearCache(object commandParameter) => true;

        public void onClearLog(object commandParameter) => proxyServer.clearLog();
        public void onClearCache(object commandParameter) => proxyServer.clearCache();

        private async void onStartStopProxy(object commandParameter)
        {
            if (!proxyServer.running)
            {
                await Task.Run(() => proxyServer.start());
            }
            else
            {
                await Task.Run(() => proxyServer.stop());
            }
        }

        public bool isRunning
        {
            get => proxyServer.running;
            set => SetProperty(proxyServer.running, value, () => proxyServer.running = value);
        }

        public string startStopButtonText
        {
            get => proxyServer.startStopButtonText;
        }

        public int proxyPort
        {
            get => proxyServer.port;
            set => SetProperty(proxyServer.port, value, () => proxyServer.port = value);
        }

        public int proxyCacheTimeOutInSeconds
        {
            get => proxyServer.cacheTimeOut;
            set => SetProperty(proxyServer.cacheTimeOut, value, () => proxyServer.cacheTimeOut = value);
        }

        public int proxyBufferSize
        {
            get => proxyServer.bufferSize;
            set => SetProperty(proxyServer.bufferSize, value, () => proxyServer.bufferSize = value);
        }

        public bool proxyServeFromCache
        {
            get => proxyServer.serveFromCache;
            set => SetProperty(proxyServer.serveFromCache, value, () => proxyServer.serveFromCache = value);
        }

        public bool proxyAuthenticationRequired
        {
            get => proxyServer.authRequired;
            set => SetProperty(proxyServer.authRequired, value, () => proxyServer.authRequired = value);
        }

        public bool proxyHideUserAgentEnabled
        {
            get => proxyServer.hideUserAgent;
            set => SetProperty(proxyServer.hideUserAgent, value, () => proxyServer.hideUserAgent = value);
        }

        public bool proxyFilterContentEnabled
        {
            get => proxyServer.filterContent;
            set => SetProperty(proxyServer.filterContent, value, () => proxyServer.filterContent = value);
        }

        public bool proxyLogRequest
        {
            get => proxyServer.logRequest;
            set => SetProperty(proxyServer.logRequest, value, () => proxyServer.logRequest = value);
        }

        public bool proxyLogResponse
        {
            get => proxyServer.logResponse;
            set => SetProperty(proxyServer.logResponse, value, () => proxyServer.logResponse = value);
        }

        public ObservableCollection<ListBoxItem> log
        {
            get => proxyServer.log;
        }

        public ObservableCollection<ListBoxItem> cacheItems
        {
            get => proxyServer.cacheItems;
        }
    }
}
