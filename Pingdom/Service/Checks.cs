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
using System.Windows.Threading;
using System.Collections.Generic;
using Pingdom.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Pingdom.Helpers;
using Madebywill.Helpers;

namespace Pingdom.Service
{
    public class Checks : ServiceBase
    {
        public void GetCheckList(Action<bool, List<Check>> callback)
        {
            
            HttpGet(baseUrl + "checks", (result, status) =>
            {
                List<Check> list = new List<Check>();

                if (status)
                {
                    //process results
                    try
                    {
                        JObject j = JObject.Parse(result);

                        var checks = from c in j["checks"].Children()
                                     select new Check{
                                        Hostname = (string)c["hostname"] ?? "",
                                        Id = (int?)c["id"] ?? 0,
                                        CreatedTimeStamp = (int?)c["created"] ?? 0,
                                        LastErrorTime = UnixTimestamp.StampToLocalDateTime((int?)c["lasterrortime"] ?? 0),
                                        LastResponseTime = (int?)c["lastresponsetime"] ?? 0,
                                        LastTestTime = UnixTimestamp.StampToLocalDateTime((int?)c["lasttesttime"] ?? 0),
                                        Name = (string)c["name"] ?? "",
                                        Resolution = (int?)c["resolution"] ?? 0,
                                        Status = (string)c["status"] ?? "",
                                        Type = (string)c["type"] ?? ""
                                     };

                        list = checks.ToList<Check>();
                    }
                    catch (Exception)
                    {
                        status = false;
                    }
                }

                //Send a result whatever

                //We're still on the background thread...
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, list);
                });

            });
        }

        public void GetCheck(int checkId, Action<bool, Check> callback)
        {
            HttpGet(baseUrl + "checks/" + checkId, (result, status) =>
            {
                Check check = new Check();

                if (status)
                {
                    try
                    {
                        JObject j = JObject.Parse(result);

                        check.Id = checkId;
                        check.CreatedTimeStamp = (int?)j.SelectToken("check.created") ?? 0;
                        check.Hostname = (string)j.SelectToken("check.hostname") ?? "";
                        check.Name = (string)j.SelectToken("check.name") ?? "";
                        check.Status = (string)j.SelectToken("check.status") ?? "";
                        check.Resolution = (int?)j.SelectToken("check.resolution") ?? 0;
                        check.LastErrorTime = UnixTimestamp.StampToLocalDateTime((int?)j.SelectToken("check.lasterrortime") ?? 0);
                        check.LastResponseTime = (int?)j.SelectToken("check.lastresponsetime") ?? 0;
                        check.LastTestTime = UnixTimestamp.StampToLocalDateTime((int?)j.SelectToken("check.lasttesttime") ?? 0);

                        if (j.SelectToken("check.type.http.url") != null)
                        {
                            check.Type = "http";
                        }
                        else if (j.SelectToken("check.type.httpcustom.url") != null)
                        {
                            check.Type = "httpcustom";
                        }
                        else if (j.SelectToken("check.type.tcp.port") != null)
                        {
                            check.Type = "tcp";
                        }
                        else if (j.SelectToken("check.type.ping") != null)
                        {
                            check.Type = "ping";
                        }
                        else if (j.SelectToken("check.type.dns.nameserver") != null)
                        {
                            check.Type = "dns";
                        }
                        else if (j.SelectToken("check.type.udp.port") != null)
                        {
                            check.Type = "udp";
                        }
                        else if (j.SelectToken("check.type.smtp.port") != null)
                        {
                            check.Type = "smtp";
                        }
                        else if (j.SelectToken("check.type.pop3.port") != null)
                        {
                            check.Type = "pop3";
                        }
                        else if (j.SelectToken("check.type.imap.port") != null)
                        {
                            check.Type = "imap";
                        }
                        else
                        {
                            check.Type = "unknown";
                        }
                    }
                    catch (Exception)
                    {
                        status = false;
                    }
                }

                //We're still on the background thread...
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, check);
                });

            });
        }

        public void GetCheckSummary(int checkId, int checkCreatedTimeStamp, Action<bool, CheckSummary> callback)
        {
            
            
            //From time: 30 days ago
            int fromTime = (UnixTimestamp.GetTimeStamp() - (60 * 60 * 24 * 30));

            //unless check was created less than 30 days ago, then it should be createdtime
            if (fromTime < checkCreatedTimeStamp)
            {
                fromTime = checkCreatedTimeStamp;
            }

            HttpGet(baseUrl + "summary.average/" + checkId + "?includeuptime=true&from=" + fromTime, (result, status) =>
            {
                CheckSummary cs = new CheckSummary();

                if (status)
                {
                    try
                    {
                        JObject j = JObject.Parse(result);

                        cs.StartTime = UnixTimestamp.StampToLocalDateTime((int?)j.SelectToken("summary.responsetime.from") ?? 0);
                        cs.EndTime = UnixTimestamp.StampToLocalDateTime((int?)j.SelectToken("summary.responsetime.to") ?? 0);
                        cs.AverageResponse = (int?)j.SelectToken("summary.responsetime.avgresponse") ?? 0;
                        cs.TotalUp = (int?)j.SelectToken("summary.status.totalup") ?? 0;
                        cs.TotalDown = (int?)j.SelectToken("summary.status.totaldown") ?? 0;
                        cs.TotalUnknown = (int?)j.SelectToken("summary.status.totalunknown") ?? 0;
                    }
                    catch (Exception)
                    {
                        status = false;
                    }
                }

                //We're still on the background thread...
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, cs);
                });

            });
        }

        public void GetOutages(int checkId, int checkCreatedTimeStamp, Action<bool, List<Outage>> callback)
        {
            //From time: 30 days ago
            int fromTime = (UnixTimestamp.GetTimeStamp() - (60 * 60 * 24 * 30));

            //unless check was created less than 30 days ago, then it should be createdtime
            if (fromTime < checkCreatedTimeStamp)
            {
                fromTime = checkCreatedTimeStamp;
            }

            HttpGet(baseUrl + "summary.outage/" + checkId + "?from=" + fromTime + "&order=desc", (result, status) =>
            {
                List<Outage> list = new List<Outage>();

                if (status)
                {
                    try
                    {
                        JObject j = JObject.Parse(result);

                        var outages = from s in j["summary"]["states"].Children()
                                      where ((string)s["status"] == "down" || (string)s["status"] == "unknown") && (int?)s["timefrom"] > 0
                                     select new Outage
                                     {
                                         Status = (string)s["status"] ?? "",
                                         TimeFrom = UnixTimestamp.StampToLocalDateTime((int?)s["timefrom"] ?? 0),
                                         TimeTo = UnixTimestamp.StampToLocalDateTime((int?)s["timeto"] ?? 0)
                                     };

                        list = outages.ToList<Outage>();
                    }
                    catch (Exception)
                    {
                        status = false;
                    }
                }

                //We're still on the background thread...
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, list);
                });

            });
        }

        public void QuickCheck(string host, Action<bool, SingleCheck> callback)
        {
            HttpGet(baseUrl + "single?host=" + host + "&type=http", (result, status) =>
            {
                SingleCheck sc = new SingleCheck();

                if (status)
                {
                    try
                    {
                        JObject j = JObject.Parse(result);

                        sc.Status = (string)j.SelectToken("result.status") ?? "";
                        sc.ProbeLocation = (string)j.SelectToken("result.probedesc") ?? "";
                        sc.ResponseTime = (int?)j.SelectToken("result.responsetime") ?? 0;
                    }
                    catch (Exception)
                    {
                        status = false;
                    }

                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, sc);
                });

            });
        }

        public void PauseCheck(int checkId, Action<bool> callback)
        {
            HttpPut(baseUrl + "checks/" + checkId, "paused=true", (result, status) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status);
                });
            });
        }

        public void ResumeCheck(int checkId, Action<bool> callback)
        {
            HttpPut(baseUrl + "checks/" + checkId, "paused=false", (result, status) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status);
                });
            });
        }

    }
}
