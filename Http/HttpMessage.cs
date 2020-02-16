using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyServer_NOTS.Http
{
    public class HttpMessage
    {
        public string firstLine;
        public byte[] body;
        public byte[] messageInBytes;
        public List<HttpHeader> headers;

        public HttpMessage(string firstLine, List<HttpHeader> headers, byte[] body, byte[] messageInBytes)
        {
            this.firstLine = firstLine;
            this.headers = headers;
            this.body = body;
            this.messageInBytes = messageInBytes;
        }

        public static List<string> toLines(string message)
        {
            string[] result = message.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            return new List<string>(result);
        }

        public static List<HttpHeader> getHeaders(List<string> messageLines)
        {
            // Remove firstLine from messageLines (request line)
            messageLines.RemoveAt(0);

            List<HttpHeader> result = new List<HttpHeader>();

            foreach (string line in messageLines)
            {
                if (line.Equals("")) break;

                int seperator = line.IndexOf(":");
                if (seperator < 0) continue;

                try
                {
                    string key = line.Substring(0, seperator).Trim();
                    string value = line.Substring(seperator + 1).Trim();

                    result.Add(new HttpHeader(key, value));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }

            return result;
        }

        #region Headers
        public HttpHeader getHeader(string key) => headers.Where(header => header.key.ToLower() == key.ToLower()).FirstOrDefault();

        public bool hasHeader(string key) => headers.Where(header => header.key.ToLower() == key.ToLower()).Count() == 1;

        public void addHeader(string key, string value) => headers.Add(new HttpHeader(key, value));

        public void removeHeader(string key) => headers.Remove(getHeader(key));

        public void updateHeader(string key, string newValue)
        {
            HttpHeader httpHeader = headers.Where(header => header.key.ToLower() == key.ToLower()).FirstOrDefault();
            httpHeader.value = newValue;
        }
        #endregion
        protected static byte[] getBody(string messsage)
        {
            string[] result = messsage.Split(
                new[] { "\r\n\r\n" },
                StringSplitOptions.None
            );

            return Encoding.UTF8.GetBytes(result[result.Length - 1]);
        }

        public byte[] toBytes
        {
            get
            {
                byte[] firstLineBytes = Encoding.UTF8.GetBytes(firstLine);
                string headersString = string.Join("\r\n", headers.Select(header => header.ToString));
                byte[] headersBytes = Encoding.UTF8.GetBytes(headersString);
                byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

                List<byte> message = new List<byte>();
                message.AddRange(firstLineBytes);
                message.AddRange(newLine);
                message.AddRange(headersBytes);
                message.AddRange(newLine);
                message.AddRange(newLine);

                if (body.Length > 0) {
                    message.AddRange(body);
                }

                return message.ToArray();
            }
        }

        public new string ToString
        {
            get => $"{firstLine}\r\n{GetHeadersAsString()}";
        }

        private string GetHeadersAsString()
        {
            return string.Join("\r\n", headers.Select(header => header.ToString));
        }
    }
}
