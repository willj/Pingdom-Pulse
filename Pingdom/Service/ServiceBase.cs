using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Net.NetworkInformation;
using System.ComponentModel;

namespace Pingdom.Service
{
    public class ServiceBase
    {
        protected string baseUrl = "https://api.pingdom.com/api/2.0/";

        public void HttpGet(string url, Action<string, bool> resultCallback)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            string authString = string.Format("Basic {0}", Helpers.Account.Instance.GetAuthString());

            request.Headers[HttpRequestHeader.Authorization] = authString;
            request.Headers["App-Key"] = Helpers.Account.Instance.GetAppKey();

            request.BeginGetResponse((result) =>
            {
                ResponseHandler(resultCallback, result);
            }, request);
        }

        protected void ResponseHandler(Action<string, bool> resultCallback, IAsyncResult asyncResult)
        {
            try
            {
                var request = (HttpWebRequest)asyncResult.AsyncState;
                var response = request.EndGetResponse(asyncResult);

                using (var rs = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(rs))
                    {
                        string result = sr.ReadToEnd();
						
                        resultCallback(result, true);
                    }
                }
            }
            catch (WebException ex)
            {
                resultCallback("", false);

                HttpError(ex);
            }
        }

        protected void HttpPost()
        {
            throw new NotImplementedException();
        }

        protected void HttpPut(string url, string requestData, Action<string, bool> resultCallback)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, e) =>
            {
                WebClient wc = new WebClient();

                string authString = string.Format("Basic {0}", Helpers.Account.Instance.GetAuthString());

                wc.Headers[HttpRequestHeader.Authorization] = authString;
                wc.Headers["App-Key"] = Helpers.Account.Instance.GetAppKey();

                wc.UploadStringCompleted += wc_UploadStringCompleted;

                wc.UploadStringAsync(new Uri(url), "PUT", requestData, resultCallback);
            };

            bw.RunWorkerAsync();
        }

        void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            Action<string, bool> resultCallback = e.UserState as Action<string, bool>;

            try
            {
                resultCallback(e.Result, true);
            }
            catch (WebException ex)
            {
                resultCallback("", false);

                HttpError(ex);
            }
        }

        protected void HttpDelete()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles HTTP errors
        /// </summary>
        /// <param name="ex"></param>
        protected void HttpError(WebException ex)
        {
            HttpWebResponse r = ex.Response as HttpWebResponse;

            if (r.StatusCode == HttpStatusCode.Unauthorized)
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Your email address or password are incorrect, please correct them and try again.", "Unauthorized", MessageBoxButton.OK);
                });
                return;
            }
            
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("A network connection could not be found, check your wi-fi & mobile network status.", "Network unavailable", MessageBoxButton.OK);
                });
                return;
            }

            SmartDispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("We're having trouble reaching pingdom right now, try again later.", "", MessageBoxButton.OK);
            });

            return;
        }

    }
}
