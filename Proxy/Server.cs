using ProxyServer.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProxyServer.Proxy
{
    public class Server
    {
        private IPAddress _ipAddress;
        private TcpListener _tcpListener;
        private Cache _cacheHandler;
        private Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        public bool IsRunning { get; set; }
        public int Port { get; set; }
        public int CacheTimeOutInSeconds { get; set; }
        public int BufferSize { get; set; }
        public bool AuthenticationRequired { get; set; }
        public bool HideUserAgentEnabled { get; set; }
        public bool FilterContentEnabled { get; set; }
        public bool LogRequest { get; set; }
        public bool LogResponse { get; set; }
        public ObservableCollection<ListBoxItem> Log { get; set; }

        public Server()
        {
            IsRunning = false;
            _ipAddress = IPAddress.Parse("127.0.0.1");
            _cacheHandler = new Cache();
            Port = 8080;
            CacheTimeOutInSeconds = 300;
            BufferSize = 1024;
            AuthenticationRequired = false;
            HideUserAgentEnabled = false;
            FilterContentEnabled = false;
            LogRequest = true;
            LogResponse = true;
            Log = new ObservableCollection<ListBoxItem>();
        }

        public void Start()
        {
            try
            {
                _tcpListener = new TcpListener(_ipAddress, Port);
                _tcpListener.Start();
                IsRunning = true;
                Task.Run(() => ListenForIncomingClients());
                AddToLog("STARTED PROXY!\r\n============");
            }
            catch
            {
                AddToLog("Could not start the Proxy!");
            }
        }
        public void Stop()
        {
            while (true)
            {
                if (_tcpListener.Pending()) continue;

                _tcpListener.Stop();
                IsRunning = false;
                AddToLog("============\r\nSTOPPED PROXY!");
                break;
            }
        }

        private void ListenForIncomingClients()
        {
            AddToLog("Waiting for new requests");

            while (IsRunning)
            {
                try
                {
                    TcpClient incomingClient = _tcpListener.AcceptTcpClient();
                    Task.Run(() => HandleClient(incomingClient));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        private async Task HandleClient(TcpClient incomingClient)
        {
            using (incomingClient)
            using (NetworkStream incomingClientStream = incomingClient.GetStream())
            {
                HttpRequest httpRequest = await ReceiveHttpRequest(incomingClientStream);
                HttpResponse httpResponse = null;

                if (httpRequest != null)
                {
                    if (HideUserAgentEnabled) httpRequest.HideUserAgent();
                    if (LogRequest) AddToLog($"\r\n==========Request Received==========\r\n{httpRequest.ToString}\r\n");

                    // Check if authentication is not required or if the request is authenticated.
                    if (!AuthenticationRequired || IsAuthenticated(httpRequest))
                    {
                        // Check if proxy needs to filter content.
                        if (FilterContentEnabled && httpRequest.HasContentToFilter())
                        {
                            httpResponse = HttpResponse.GetPlaceholderResponse();
                            await incomingClientStream.WriteAsync(httpResponse.ToBytes, 0, httpResponse.ToBytes.Length);
                        }

                        // Check if request is cacheable and is already stored in the cache.
                        if (httpRequest.IsCacheable() && _cacheHandler.IsStoredInCache(httpRequest.FirstLine))
                        {
                            CacheItem cacheItem = _cacheHandler.GetCacheItem(httpRequest.FirstLine);

                            // If cacheItem is still valid, return cacheItem as response.
                            if (cacheItem.IsValid(CacheTimeOutInSeconds))
                            {
                                httpResponse = cacheItem.Response;
                                await incomingClientStream.WriteAsync(cacheItem.ResponseInBytes, 0, cacheItem.ResponseInBytes.Length);
                            }
                            // If cacheItem is no longer valid, remove cacheItem from cache.
                            else
                            {
                                _cacheHandler.RemoveCacheItem(httpRequest.FirstLine);
                            }
                        }

                        // If request is still 'null', the proxy needs to stream the response from the server.
                        if (httpResponse == null)
                        {
                            httpResponse = await StreamHttpResponseFromServerToClient(httpRequest, incomingClientStream);

                            // If request is cacheable, store the response in cache.
                            if (httpRequest.IsCacheable()) _cacheHandler.TryStoreInCache(httpRequest, httpResponse);
                        }
                    }
                    // Request is not authenticated, send 407 response.
                    else
                    {
                        httpResponse = HttpResponse.Get407Response();
                        await incomingClientStream.WriteAsync(httpResponse.ToBytes, 0, httpResponse.ToBytes.Length);
                    }

                    if (httpResponse != null && LogResponse) AddToLog($"\r\n==========Response Sent==========\r\n{httpResponse.ToString}\r\n");
                }
            }
        }

        private async Task<HttpRequest> ReceiveHttpRequest(NetworkStream incomingClientStream)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] requestBuffer = new byte[BufferSize];
                    int bytesReceived;

                    if (incomingClientStream.CanRead)
                    {
                        if (incomingClientStream.DataAvailable)
                        {
                            do
                            {
                                bytesReceived = await incomingClientStream.ReadAsync(requestBuffer, 0, requestBuffer.Length);
                                await memoryStream.WriteAsync(requestBuffer, 0, bytesReceived);
                            } while (incomingClientStream.DataAvailable);
                        }
                    }

                    byte[] requestBytes = memoryStream.ToArray();
                    HttpRequest httpRequest = HttpRequest.TryParse(requestBytes);

                    return httpRequest;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }
        private async Task<HttpResponse> StreamHttpResponseFromServerToClient(HttpRequest httpRequest, NetworkStream incomingClientStream)
        {
            using (TcpClient clientToApproach = new TcpClient(httpRequest.GetHost(), 80))
            using (NetworkStream clientToApproachStream = clientToApproach.GetStream())
            {
                await clientToApproachStream.WriteAsync(httpRequest.ToBytes, 0, httpRequest.ToBytes.Length);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    try
                    {
                        byte[] responseBuffer = new byte[BufferSize];

                        while (true)
                        {
                            int bytesReceived = await clientToApproachStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                            if (bytesReceived == 0) break;
                            await memoryStream.WriteAsync(responseBuffer, 0, bytesReceived);
                            await incomingClientStream.WriteAsync(responseBuffer, 0, bytesReceived);
                        }

                        byte[] responseBytes = memoryStream.ToArray();
                        HttpResponse httpResponse = HttpResponse.TryParse(responseBytes);

                        return httpResponse;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.ToString());
                        return null;
                    }
                }
            }
        }

        private bool IsAuthenticated(HttpRequest httpRequest)
        {
            if (httpRequest.HasHeader("Admin"))
            {
                HttpHeader adminHeader = httpRequest.GetHeader("Admin");
                if (adminHeader.Key == adminHeader.Value) return true;
            }

            return false;
        }
        private void AddToLog(string message) => _dispatcher.Invoke(() => Log.Add(new ListBoxItem { Content = message }));
        public void ClearLog() => Log.Clear();
    }
}
