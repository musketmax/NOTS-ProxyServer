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
        public ObservableCollection<ListBoxItem> Log { get; set; } = new ObservableCollection<ListBoxItem>();
        private IPAddress IP;
        private TcpListener listener;
        private Cache cache = new Cache();
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        public bool running { get; set; } = false;
        public int port { get; set; } = 8080;
        public int cacheTimeOut { get; set; } = 300;
        public int bufferSize { get; set; } = 1024;
        public bool serveFromCache { get; set; } = false;
        public bool authRequired { get; set; } = false;
        public bool hideUserAgent { get; set; } = false;
        public bool filterContent { get; set; } = false;
        public bool logRequest { get; set; } = true;
        public bool logResponse { get; set; } = true;

        public Server()
        {
            // Set IP to localhost
            IP = IPAddress.Parse("127.0.0.1");
        }

        // Start the Proxy
        public void Start()
        {
            try
            {
                listener = new TcpListener(IP, port);
                listener.Start();
                running = true;
                Task.Run(() => ListenForIncomingClients());
                AddToLog("===============\r\nPROXY IS LISTENING\r\n===============");
            }
            catch
            {
                AddToLog("Error starting proxy!");
            }
        }

        // Stop the Proxy
        public void Stop()
        {
            while (true)
            {
                // Only terminate the Proxy when all requests have been handled
                if (listener.Pending()) continue;

                listener.Stop();
                running = false;
                AddToLog("===============\r\nPROXY IS TERMINATED\r\n===============");
                break;
            }
        }

        // Listen for incoming connections
        private void ListenForIncomingClients()
        {
            AddToLog("Listening for requests...");

            while (running)
            {
                try
                {
                    // Set every connection on dedicated thread/task
                    TcpClient client = listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        // Handle the connection request and response
        private async Task HandleClient(TcpClient client)
        {
            using (client)
            using (NetworkStream clientStream = client.GetStream())
            {
                // Retrieve the incoming request from client
                HttpRequest request = await ReceiveHttpRequest(clientStream);
                HttpResponse response = null;

                if (request != null)
                {
                    // Hide the user agent if checked
                    if (hideUserAgent) request.HideUserAgent();

                    if (logRequest) AddToLog($"\r\n========== REQUEST RECEIVED ==========\r\n{request.ToString}\r\n");

                    // Check if auth is required, or if the request has been authenticated
                    if (!authRequired || isAuthenticated(request))
                    {
                        // Check if we need to filter some content, and the content can be filtered
                        if (filterContent && request.HasContentToFilter())
                        {
                            response = HttpResponse.GetSafePlaceholderImageResponse();
                            await clientStream.WriteAsync(response.ToBytes, 0, response.ToBytes.Length);
                        }

                        // Check if we can cache the request and if we already have something ready in cache
                        if (serveFromCache && request.IsCacheable() && cache.IsStored(request.FirstLine))
                        {
                            CacheItem cacheItem = cache.GetCacheItem(request.FirstLine);

                            // If cacheItem is still valid, return cacheItem as response.
                            if (cacheItem.IsValid(cacheTimeOut))
                            {
                                response = cacheItem.Response;
                                await clientStream.WriteAsync(cacheItem.ResponseInBytes, 0, cacheItem.ResponseInBytes.Length);
                                AddToLog($"\r\n========== SERVING '{request.FirstLine}' FROM CACHE, TTL: {Math.Round(cacheItem.RemainingTime(cacheTimeOut))} SECONDS ==========\r\n");
                            }
                            // If cacheItem is no longer valid, remove cacheItem from cache.
                            else
                            {
                                cache.RemoveCacheItem(request.FirstLine);
                            }
                        }

                        // Stream response from server if response is not yet sent
                        if (response == null)
                        {
                            response = await StreamHttpResponseFromServerToClient(request, clientStream);

                            // If request is cacheable, store the response in cache
                            if (request.IsCacheable())
                            {
                                cache.Store(request, response);
                                AddToLog($"\r\n========== '{request.FirstLine}' IS CACHED FOR FUTURE USE ==========\r\n");
                            }
                        }
                    }
                    else
                    {
                        // Send 407 to client if we need authentication and are not authenticated yet
                        response = HttpResponse.Return407NotAuthenticatedToProxy();
                        await clientStream.WriteAsync(response.ToBytes, 0, response.ToBytes.Length);
                    }

                    if (response != null && logResponse) AddToLog($"\r\n========== RESPONSE SENT ==========\r\n{response.ToString}\r\n");
                }
            }
        }

        // Handle incoming connection and parse to httprequest model
        private async Task<HttpRequest> ReceiveHttpRequest(NetworkStream clientStream)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] requestBuffer = new byte[bufferSize];
                    int bytesReceived;

                    // For as long as we can stream data from the client, fill our memorystream
                    if (clientStream.CanRead)
                    {
                        if (clientStream.DataAvailable)
                        {
                            do
                            {
                                bytesReceived = await clientStream.ReadAsync(requestBuffer, 0, requestBuffer.Length);
                                await memoryStream.WriteAsync(requestBuffer, 0, bytesReceived);
                            } while (clientStream.DataAvailable);
                        }
                    }

                    // Fill byte array with streamed bytes from client, and parse the request so we can work with it
                    byte[] requestBytes = memoryStream.ToArray();
                    HttpRequest httpRequest = HttpRequest.ParseToHTTPRequest(requestBytes);

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
        private async Task<HttpResponse> StreamHttpResponseFromServerToClient(HttpRequest request, NetworkStream stream)
        {
            using (TcpClient client = new TcpClient(request.GetHost(), 80))
            using (NetworkStream clientStream = client.GetStream())
            {
                await clientStream.WriteAsync(request.ToBytes, 0, request.ToBytes.Length);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    try
                    {
                        byte[] responseBuffer = new byte[bufferSize];

                        while (true)
                        {
                            int bytesReceived = await clientStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                            if (bytesReceived == 0) break;

                            await memoryStream.WriteAsync(responseBuffer, 0, bytesReceived);
                            await stream.WriteAsync(responseBuffer, 0, bytesReceived);
                        }

                        byte[] responseBytes = memoryStream.ToArray();
                        HttpResponse httpResponse = HttpResponse.ParseToHTTPResponse(responseBytes);

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

        // Check if authenticated
        private bool isAuthenticated(HttpRequest request)
        {
            if (request.HasHeader("Proxy-Authorization"))
            {
                HttpHeader adminHeader = request.GetHeader("Proxy-Authorization");
                if (adminHeader.Value == "admin") return true;
            }

            return false;
        }

        private void AddToLog(string message) => dispatcher.Invoke(() => Log.Add(new ListBoxItem { Content = message }));
        public void ClearLog() => Log.Clear();
    }
}
