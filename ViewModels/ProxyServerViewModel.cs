using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ProxyServer.Proxy;

namespace ProxyServer.ViewModels
{
    public class ProxyServerViewModel : ViewModelBase
    {
        private Server _server;

        private readonly CommandDelegate _startStopProxyCommand;
        private readonly CommandDelegate _clearLog;
        public ICommand StartStopProxyCommand => _startStopProxyCommand;
        public ICommand ClearLogCommand => _clearLog;

        public ProxyServerViewModel()
        {
            _server = new Server();
            _startStopProxyCommand = new CommandDelegate(OnStartStopProxy, CanStartStopProxy);
            _clearLog = new CommandDelegate(OnClearLog, CanClearLog);
        }


        public bool IsRunning
        {
            get => _server.IsRunning;
            set => SetProperty(_server.IsRunning, value, () => _server.IsRunning = value);
        }

        public int ProxyPort
        {
            get => _server.Port;
            set => SetProperty(_server.Port, value, () => _server.Port = value);
        }

        public int ProxyCacheTimeOutInSeconds
        {
            get => _server.CacheTimeOutInSeconds;
            set => SetProperty(_server.CacheTimeOutInSeconds, value, () => _server.CacheTimeOutInSeconds = value);
        }

        public int ProxyBufferSize
        {
            get => _server.BufferSize;
            set => SetProperty(_server.BufferSize, value, () => _server.BufferSize = value);
        }

        public bool ProxyAuthenticationRequired
        {
            get => _server.AuthenticationRequired;
            set => SetProperty(_server.AuthenticationRequired, value, () => _server.AuthenticationRequired = value);
        }

        public bool ProxyHideUserAgentEnabled
        {
            get => _server.HideUserAgentEnabled;
            set => SetProperty(_server.HideUserAgentEnabled, value, () => _server.HideUserAgentEnabled = value);
        }

        public bool ProxyFilterContentEnabled
        {
            get => _server.FilterContentEnabled;
            set => SetProperty(_server.FilterContentEnabled, value, () => _server.FilterContentEnabled = value);
        }

        public bool ProxyLogRequest
        {
            get => _server.LogRequest;
            set => SetProperty(_server.LogRequest, value, () => _server.LogRequest = value);
        }

        public bool ProxyLogResponse
        {
            get => _server.LogResponse;
            set => SetProperty(_server.LogResponse, value, () => _server.LogResponse = value);
        }

        public ObservableCollection<ListBoxItem> Log
        {
            get => _server.Log;
        }

        private async void OnStartStopProxy(object commandParameter)
        {
            if (!_server.IsRunning)
            {
                await Task.Run(() => _server.Start());
            }
            else
            {
                await Task.Run(() => _server.Stop());
            }
        }
        private bool CanStartStopProxy(object commandParameter) => true;
        public void OnClearLog(object commandParameter) => _server.ClearLog();
        private bool CanClearLog(object commandParameter) => true;
    }
}
