﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using v2rayN.Mode;
using v2rayN.Properties;
using v2rayN.HttpProxyHandler;

namespace v2rayN.Handler
{
    /// <summary>
    ///Update V2ray Core 
    /// </summary>
    class V2rayUpdateHandle
    {
        public event EventHandler<ResultEventArgs> AbsoluteCompleted;

        public event EventHandler<ResultEventArgs> UpdateCompleted;

        public event EventHandler<ResultEventArgs> LoginCompleted;

        public event ErrorEventHandler Error;


 
        public string DownloadFileName
        {
            get { return "v2ray-windows.zip"; }
        }

        public class ResultEventArgs : EventArgs
        {
            public bool Success;
            public string Msg;

            public ResultEventArgs(bool success, string msg)
            {
                this.Success = success;
                this.Msg = msg;
            }
        }



        private string latestUrl = "https://github.com/v2ray/v2ray-core/releases/latest";
        private const string coreURL = "https://github.com/v2ray/v2ray-core/releases/download/{0}/v2ray-windows-{1}.zip";
        private int progressPercentage = -1;
        private bool blFirst = true;
        private long totalBytesToReceive = 0;
        private DateTime totalDatetime = new DateTime();

        public void AbsoluteV2rayCore(Config config)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            ServicePointManager.DefaultConnectionLimit = 256;
            WebRequest request = WebRequest.Create(latestUrl);
            request.BeginGetResponse(new AsyncCallback(OnResponse), request);
        }

        private void OnResponse(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                string redirectUrl = response.ResponseUri.AbsoluteUri;
                string version = redirectUrl.Substring(redirectUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

                string osBit = string.Empty;
                if (Environment.Is64BitProcess)
                {
                    osBit = "64";
                }
                else
                {
                    osBit = "32";
                }
                string url = string.Format(coreURL, version, osBit);
                if (AbsoluteCompleted != null)
                {
                    AbsoluteCompleted(this, new ResultEventArgs(true, url));
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);

                if (Error != null)
                    Error(this, new ErrorEventArgs(ex));
            }
        }


        public void DownloadFileAsync(Config config, string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
                ServicePointManager.DefaultConnectionLimit = 256;
                if (UpdateCompleted != null)
                {
                    UpdateCompleted(this, new ResultEventArgs(false, url));
                }

                progressPercentage = -1;

                WebClientEx ws = new WebClientEx();
                ws.DownloadFileCompleted += ws_DownloadFileCompleted;
                ws.DownloadProgressChanged += ws_DownloadProgressChanged;
                ws.DownloadFileAsync(new Uri(url), Utils.GetPath(DownloadFileName));
                blFirst = true;
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);

                if (Error != null)
                    Error(this, new ErrorEventArgs(ex));
            }
        }

        void ws_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (blFirst)
            {
                totalBytesToReceive = e.TotalBytesToReceive - e.BytesReceived;
                totalDatetime = DateTime.Now;
                blFirst = false;
            }
            if (UpdateCompleted != null)
            {
                if (progressPercentage != e.ProgressPercentage && e.ProgressPercentage % 10 == 0)
                {
                    progressPercentage = e.ProgressPercentage;
                    string msg = string.Format("......{0}%", e.ProgressPercentage);
                    UpdateCompleted(this, new ResultEventArgs(false, msg));
                }
            }
        }

        void ws_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null
                    || Utils.IsNullOrEmpty(e.Error.ToString()))
                {
                    if (UpdateCompleted != null)
                    {
                        TimeSpan ts = (DateTime.Now - totalDatetime);
                        string speed = string.Format("{0} M/s", (totalBytesToReceive / ts.TotalMilliseconds / 1000).ToString("#0.##"));
                        UpdateCompleted(this, new ResultEventArgs(true, speed));
                    }
                }
                else
                {
                    throw e.Error;
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);

                if (Error != null)
                    Error(this, new ErrorEventArgs(ex));
            }
        }

        /// <summary>
        /// DownloadString
        /// </summary> 
        /// <param name="url"></param>
        public void WebDownloadString(string url)
        {
            string source = string.Empty;
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
                ServicePointManager.DefaultConnectionLimit = 256;

                WebClientEx ws = new WebClientEx();
                ws.DownloadStringCompleted += Ws_DownloadStringCompleted;
                ws.DownloadStringAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);
            }
        }

        private void Ws_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null
                    || Utils.IsNullOrEmpty(e.Error.ToString()))
                {
                    string source = e.Result;
                    if (UpdateCompleted != null)
                    {
                        UpdateCompleted(this, new ResultEventArgs(true, source));
                    }
                }
                else
                {
                    throw e.Error;
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);

                if (Error != null)
                    Error(this, new ErrorEventArgs(ex));
            }
        }

        public void WebLogin(string url, string username, string pwd)
        {
            string source = string.Empty;

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
                ServicePointManager.DefaultConnectionLimit = 256;

                WebClientEx ws = new WebClientEx();

                var reqParams = new System.Collections.Specialized.NameValueCollection();
                reqParams.Add("account", username);
                reqParams.Add("password", pwd);

                ws.UploadValuesCompleted += Ws_login_completed;
                ws.UploadValuesAsync(new Uri(url), "POST", reqParams);

            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);
            }
        }

        private void Ws_login_completed(object sender, UploadValuesCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null
                    || Utils.IsNullOrEmpty(e.Error.ToString()))
                {
                    string source = Encoding.UTF8.GetString(e.Result);

                    if (LoginCompleted != null)
                    {
                        LoginCompleted(this, new ResultEventArgs(true, source));
                    }
                }
                else
                {
                    throw e.Error;
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);

                if (Error != null)
                    Error(this, new ErrorEventArgs(ex));
            }
        }

    }
}
