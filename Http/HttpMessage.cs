using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Http
{
    public class HttpMessage
    {
        public string FirstLine;
        public List<HttpHeader> Headers;
        public byte[] Body;
        public byte[] MessageInBytes;

        public HttpMessage(string firstLine, List<HttpHeader> headers, byte[] body, byte[] messageInBytes)
        {
            FirstLine = firstLine;
            Headers = headers;
            Body = body;
            MessageInBytes = messageInBytes;
        }

        public static List<string> ToLines(string message)
        {
            string[] result = message.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            return new List<string>(result);
        }

        protected static List<HttpHeader> GetHeaders(List<string> messageLines)
        {
            // Remove firstLine from messageLines
            messageLines.RemoveAt(0);

            List<HttpHeader> headers = new List<HttpHeader>();
            foreach (string line in messageLines)
            {
                if (line.Equals("")) break;

                int seperator = line.IndexOf(":");
                if (seperator < 0) continue;

                try
                {
                    string key = line.Substring(0, seperator).Trim();
                    string value = line.Substring(seperator + 1).Trim();

                    headers.Add(new HttpHeader(key, value));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }

            return headers;
        }
        public bool HasHeader(string key)
        {
            if (Headers.Where(header => header.Key.ToLower() == key.ToLower()).Count() == 1) return true;

            return false;
        }
        public HttpHeader GetHeader(string key) => Headers.Where(header => header.Key.ToLower() == key.ToLower()).FirstOrDefault();
        public void UpdateHeader(string key, string newValue)
        {
            HttpHeader httpHeader = Headers.Where(header => header.Key.ToLower() == key.ToLower()).FirstOrDefault();
            httpHeader.Value = newValue;
        }
        public void AddHeader(string key, string value) => Headers.Add(new HttpHeader(key, value));
        public void RemoveHeader(string key) => Headers.Remove(GetHeader(key));

        protected static byte[] GetBody(string messageString)
        {
            string[] bodyStringArray = messageString.Split(
                new[] { "\r\n\r\n" },
                StringSplitOptions.None
            );

            byte[] bodyBytes = Encoding.UTF8.GetBytes(bodyStringArray[bodyStringArray.Length - 1]);

            return bodyBytes;
        }

        public byte[] ToBytes
        {
            get
            {
                byte[] firstLineBytes = Encoding.UTF8.GetBytes(FirstLine);
                string headersString = string.Join("\r\n", Headers.Select(header => header.ToString));
                byte[] headersBytes = Encoding.UTF8.GetBytes(headersString);
                byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

                List<byte> message = new List<byte>();
                message.AddRange(firstLineBytes);
                message.AddRange(newLine);
                message.AddRange(headersBytes);
                message.AddRange(newLine);
                message.AddRange(newLine);
                message.AddRange(Body);

                return message.ToArray();
            }
        }
        public new string ToString
        {
            get => $"{FirstLine}\r\n{GetHeadersAsString()}";
        }
        private string GetHeadersAsString() => string.Join("\r\n", Headers.Select(header => header.ToString));
    }
}
