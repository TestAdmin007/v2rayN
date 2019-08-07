using System;
using System.Net;
using v2rayN.Mode;
using v2rayN.Handler;
using System.Collections.Specialized;

namespace v2rayN.HttpProxyHandler
{
    class WebClientEx : WebClient
    {
        public int Timeout { get; set; }

        /// <summary>
        /// 配置
        /// </summary>
        protected static Config config;

        public WebClientEx(int timeout = 3000)
        {
            Timeout = timeout;
            ConfigHandler.LoadConfig(ref config);
        }


        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;

            if (config.user != null && config.user.token != null && !Utils.IsNullOrEmpty(config.user.token)) {
                request.Headers.Add("Authorization", "Bearer " + config.user.token);
            }
            return request;
        }

        internal byte[] UploadValuesAsync(string url, string v, NameValueCollection reqParams)
        {
            throw new NotImplementedException();
        }
    }
}
