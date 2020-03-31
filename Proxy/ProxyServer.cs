using ProxyServer_NOTS.Http;
using ProxyServer_NOTS.Proxy.Caches;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProxyServer_NOTS.Proxy
{
    public class ProxyServer
    {
        public IPAddress IP;
        public TcpListener listener;

        public bool running { get; set; } = false;
        public bool stopping { get; set; } = false;
        public int port { get; set; } = 8080;
        public int cacheTimeOut { get; set; } = 600;
        public int bufferSize { get; set; } = 4096;
        public bool serveFromCache { get; set; } = false;
        public bool authRequired { get; set; } = false;
        public bool hideUserAgent { get; set; } = false;
        public bool filterContent { get; set; } = false;
        public bool logRequest { get; set; } = true;
        public bool logResponse { get; set; } = true;
        public string startStopButtonText
        {
            get => "Start/Stop Proxy";
        }

        public ObservableCollection<ListBoxItem> log { get; set; }
        public ObservableCollection<ListBoxItem> cacheItems { get; set; }

        private Cache cache;
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        public ProxyServer()
        {
            cache = new Cache();
            IP = IPAddress.Parse("127.0.0.1");
            log = new ObservableCollection<ListBoxItem>();
            cacheItems = new ObservableCollection<ListBoxItem>();
        }

        public async Task start()
        {
            try
            {
                listener = new TcpListener(IP, port);
                listener.Start();
                running = true;
                addToLog("===============\r\nPROXY IS LISTENING\r\n===============");

                try
                {
                    await Task.Run(() => listenForConnections());
                }
                catch { }
            }
            catch (Exception exception)
            {
                addToLog($"Error starting Proxy: {exception.Message}");
            }
        }

        public void stop()
        {
            addToLog("===============\r\nSTOPPING PROXY... WAITING FOR OPEN CONNECTIONS TO FINISH\r\n===============");
            stopping = true;

            while (true)
            {
                // Only terminate the Proxy when all requests have been handled
                if (listener.Pending()) continue;

                listener.Stop();
                running = false;
                addToLog("===============\r\nPROXY IS TERMINATED\r\n===============");
                break;
            }
        }

        private async Task listenForConnections()
        {
            addToLog("Listening for requests...");

            while (running && !stopping)
            {
                try
                {
                    // Set every connection on dedicated thread/task
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    await Task.Run(() => handleIncomingConnection(client));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        private async Task handleIncomingConnection(TcpClient client)
        {
            using (NetworkStream connectionStream = client.GetStream())
            {
                // Retrieve the incoming request from client
                HttpRequest request = await parseIncomingConnection(connectionStream);

                if (request != null && !stopping)
                {
                    try
                    {
                        HttpResponse response = null;

                        // Hide the user agent if checked
                        if (hideUserAgent) request.removeUserAgent();

                        if (logRequest) addToLog($"\r\n========== REQUEST RECEIVED ==========\r\n{request.ToString}\r\n");

                        // Check if auth is required, or if the request has been authenticated
                        if (!authRequired || (authRequired && isAuthenticated(request)))
                        {
                            // Check if we need to filter some content, and the content can be filtered
                            if (filterContent && request.hasContentToFilter())
                            {
                                response = HttpResponse.getSafePlaceholderImageResponse();
                                await connectionStream.WriteAsync(response.toBytes, 0, response.toBytes.Length);

                                if (response != null && logResponse) addToLog($"\r\n========== RESPONSE SENT ==========\r\n{response.ToString}\r\n");

                                return;
                            }

                            // Check if we can cache the request and if we already have something ready in cache
                            if (serveFromCache && request.canCache() && cache.isStored(request.firstLine))
                            {
                                CacheItem cacheItem = cache.getItem(request.firstLine);

                                // If cacheItem is still valid, return cacheItem as response.
                                if (cacheItem.isValid(cacheTimeOut))
                                {
                                    response = cacheItem.response;
                                    addToLog($"\r\n========== SERVING '{request.firstLine}' FROM CACHE, TTL: {Math.Round(cacheItem.getRemainingTime(cacheTimeOut))} SECONDS ==========\r\n");
                                    await connectionStream.WriteAsync(cacheItem.responseInBytes, 0, cacheItem.responseInBytes.Length);

                                    if (response != null && logResponse) addToLog($"\r\n========== RESPONSE SENT ==========\r\n{response.ToString}\r\n");

                                    return;
                                }
                                else
                                {
                                    // If cacheItem is no longer valid, remove cacheItem from cache.
                                    cache.removeItem(request.firstLine);
                                    addToLog($"\r\n========== '{request.firstLine}' IS NO LONGER VALID. REVERTING TO NORMAL REQUEST MODE ==========\r\n");
                                }
                            }

                            // Stream response from server if response is not yet sent
                            if (response == null)
                            {
                                response = await sendNormalHttpResponseToClient(request, connectionStream);

                                // If request is cacheable, store the response in cache
                                if (request.canCache() && response != null && !filterContent)
                                {
                                    if (cache.isStored(request.firstLine))
                                    {
                                        addToLog($"\r\n========== '{request.firstLine}' IS ALREADY IN CACHE ==========\r\n");
                                        return;
                                    }

                                    cache.store(request, response);
                                    addToLog($"\r\n========== '{request.firstLine}' IS CACHED FOR FUTURE USE ==========\r\n");
                                    dispatcher.Invoke(() => cacheItems.Add(new ListBoxItem { Content = request.firstLine }));
                                }
                            }
                        }
                        else
                        {
                            // Send 407 to client if we need authentication and are not authenticated yet
                            response = HttpResponse.Return407NotAuthenticatedToProxy();
                            await connectionStream.WriteAsync(response.toBytes, 0, response.toBytes.Length);
                        }

                        if (response != null && logResponse) addToLog($"\r\n========== RESPONSE SENT ==========\r\n{response.ToString}\r\n");
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error sending response: {exception.Message}");
                        addToLog($"Request terminated: {exception.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Something went wrong trying to connect to remote host.");
                }
            }
        }

        private async Task<HttpRequest> parseIncomingConnection(NetworkStream connection)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[bufferSize];
                    int bytesReceived;

                    // For as long as we can stream data from the client, fill our memorystream
                    if (connection.DataAvailable)
                    {
                        do
                        {
                            bytesReceived = await connection.ReadAsync(buffer, 0, buffer.Length);
                            await memoryStream.WriteAsync(buffer, 0, bytesReceived);
                        } while (connection.DataAvailable && !stopping);
                    }

                    // Fill byte array with streamed bytes from client, and parse the request so we can work with it
                    byte[] requestBytes = memoryStream.ToArray();
                    HttpRequest httpRequest = HttpRequest.parseToHttpRequest(requestBytes);

                    return httpRequest;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }

        // Stream response from server to client
        private async Task<HttpResponse> sendNormalHttpResponseToClient(HttpRequest request, NetworkStream connectionStream)
        {
            try
            {
                TcpClient host = new TcpClient();
                host.ReceiveBufferSize = bufferSize;
                await host.ConnectAsync(request.getHost(), 80);

                using (NetworkStream hostStream = host.GetStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    try
                    {
                        byte[] buffer = new byte[bufferSize];
                        await hostStream.WriteAsync(request.toBytes, 0, request.toBytes.Length);

                        while (true && !stopping)
                        {
                            int bytesRead = await hostStream.ReadAsync(buffer, 0, buffer.Length);
                            System.Diagnostics.Debug.WriteLine(bytesRead);

                            if (bytesRead == 0) break;

                            await connectionStream.WriteAsync(buffer, 0, bytesRead);
                            await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        }

                        byte[] responseBytes = memoryStream.GetBuffer();
                        HttpResponse httpResponse = HttpResponse.parseToHTTPResponse(responseBytes);

                        return httpResponse;
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                return null;
            }
        }

        // Check if authenticated
        private bool isAuthenticated(HttpRequest request)
        {
            if (request.hasHeader("Proxy-Authorization"))
            {
                HttpHeader adminHeader = request.getHeader("Proxy-Authorization");
                if (adminHeader.value == "admin") return true;
            }

            return false;
        }

        public void clearCache()
        {
            cache.clear();
            addToLog("\r\n ========== CACHE CLEARED ==========\r\n");
            cacheItems.Clear();
        }

        #region log
        public void clearLog() => log.Clear();

        private void addToLog(string message) => dispatcher.Invoke(() => log.Add(new ListBoxItem { Content = message }));
        #endregion
    }
}
