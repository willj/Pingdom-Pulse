using System;
using System.Net;
using System.Windows;
using Pingdom.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows.Threading;

namespace Pingdom.Service
{
    public class Account : ServiceBase
    {
        public void GetAccountSettings(Action<bool, string> callback)
        {
            HttpGet(baseUrl + "settings", (result, status) =>
            {
                string country = string.Empty;

                if (status)
                {
                    try
                    {
                        JObject j = JObject.Parse(result);

                        country = (string)j.SelectToken("settings.country.name") ?? "";
                    }
                    catch (Exception)
                    {
                        status = false;
                    }

                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, country);
                });
            });
        }
    }
}
