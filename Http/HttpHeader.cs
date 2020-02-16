using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyServer_NOTS.Http
{
    public class HttpHeader
    {
        public string key { get; set; }
        public string value { get; set; }

        public HttpHeader(string key, string value)
        {
            this.key = key;
            this.value = value;

            Console.WriteLine(this.key);
        }

        public new string ToString
        {
            get => $"{key}: {value}";
        }
    }
}
