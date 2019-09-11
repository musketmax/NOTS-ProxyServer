using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Http
{
    public class HttpHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public HttpHeader(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public new string ToString
        {
            get => $"{Key}: {Value}";
        }
    }
}
