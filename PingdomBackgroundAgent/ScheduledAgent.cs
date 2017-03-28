using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using System.Net;
using Madebywill.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace PingdomBackgroundAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }


        private List<Check> storedChecks;
        private List<Check> currentChecks;
        private static string checksJsonFileName = "checks.json";
        private static string errorStringFileName = "errors.txt";
        private static string appKey = "awogqsoa5wjvsrspq40h8nd2yfilyclr";
        private List<string> checksWithErrors;
        private DateTime expirationTime;
        private bool? useTransparentTiles;

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            expirationTime = task.ExpirationTime;

            string authString = string.Empty;
            bool accountVerified = false;

            if (IsolatedStorageSettings.ApplicationSettings.Contains("auth_string"))
            {
                byte[] unprotectedBytes = UnicodeEncoding.UTF8.GetBytes(IsolatedStorageSettings.ApplicationSettings["auth_string"].ToString());
                byte[] protectedBytes = System.Security.Cryptography.ProtectedData.Protect(unprotectedBytes, null);

                IsolatedStorageSettings.ApplicationSettings["protected_auth_string"] = protectedBytes;
                IsolatedStorageSettings.ApplicationSettings.Remove("auth_string");
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("protected_auth_string") && IsolatedStorageSettings.ApplicationSettings.Contains("account_verified"))
            {
                byte[] protectedBytes = (byte[])IsolatedStorageSettings.ApplicationSettings["protected_auth_string"];
                byte[] decodedBytes = ProtectedData.Unprotect(protectedBytes, null);

                authString = UnicodeEncoding.UTF8.GetString(decodedBytes, 0, decodedBytes.Length);
                accountVerified = (bool)IsolatedStorageSettings.ApplicationSettings["account_verified"];
            }

            if (accountVerified == false || string.IsNullOrEmpty(authString))
            {
                NotifyComplete();    
            }

            //Load list of checks that already have errors
            LoadChecksWithErrors();

            //Load/parse old checks
            storedChecks = ProcessChecks(LoadStoredChecks());

            Random r = new Random();
            int rand = r.Next(50000);

            //Download/parse new checks
            WebClient wc = new WebClient();
            wc.Headers["App-Key"] = appKey;
            wc.Headers["Authorization"] = "Basic " + authString;
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);

            wc.DownloadStringAsync(new Uri("https://api.pingdom.com/api/2.0/checks" + "?rand=" + rand));
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                int errorCount = 0;

                //parse new checks
                currentChecks = ProcessChecks(e.Result);

                //save new checks
                StoreChecks(e.Result);

                if (currentChecks.Count < 1)
                {
                    NotifyComplete();
                }

                foreach (Check c in currentChecks)
                {
                    Check sc = storedChecks.SingleOrDefault(x => x.Id == c.Id);

                    if (c.Status == "down")
                    {
                        errorCount++;
                        AddToListOfErrors(c.Id);
                    }
                    else
                    {
                        if (sc != null)
                        {
                            if (c.LastErrorTime > sc.LastErrorTime)
                            {
                                errorCount++;
                                AddToListOfErrors(c.Id);
                            }
                        }
                    }

                    //Update the tile...
                    ShellTile checkTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("pinnedcheck=" + c.Id));

                    if (checkTile != null)
                    {
                        StandardTileData tileData = new StandardTileData();
                        tileData.BackgroundImage = new Uri(GetTileImage(c.Status), UriKind.Relative);
                        tileData.BackContent = c.LastResponseTime.ToString() + " ms";

                        checkTile.Update(tileData);
                    }

                }

                //If we've got any new errors...
                if (errorCount > 0)
                {
                    string tileBack = string.Empty;

                    if (checksWithErrors.Count() == 1)
                    {
                        tileBack = "1 new error";
                    }
                    else
                    {
                        tileBack = checksWithErrors.Count().ToString() + " new errors";
                    }

                    //update the tile
                    ShellTile tile = ShellTile.ActiveTiles.First();

                    if (tile != null)
                    {
                        StandardTileData tileData = new StandardTileData
                        {
                            Count = checksWithErrors.Count(),
                            BackContent = tileBack
                        };

                        tile.Update(tileData);

                        IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"] = true;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                    }

                    SaveChecksWithErrors();
                }
            }

            if (expirationTime.Subtract(DateTime.Now).TotalHours < 48)
            {

                // we'll assume it's true if we don't know
                bool backDataSet = true;

                if (IsolatedStorageSettings.ApplicationSettings.Contains("TileBackDataSet"))
                {
                    backDataSet = (bool)IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"];
                }

                if (!backDataSet)
                {
                    ShellTile tile = ShellTile.ActiveTiles.First();

                    if (tile != null)
                    {
                        StandardTileData tileData = new StandardTileData
                        {
                            BackContent = "Open me to reset live tile updates"
                        };

                        tile.Update(tileData);
                    }

                    IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"] = true;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }

            NotifyComplete();
        }

        private void AddToListOfErrors(int checkId)
        {
            if (!checksWithErrors.Contains(checkId.ToString()))
            {
                checksWithErrors.Add(checkId.ToString());
            }
        }

        private void LoadChecksWithErrors()
        {
            string data = string.Empty;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(errorStringFileName))
                {
                    using (IsolatedStorageFileStream stream = store.OpenFile(errorStringFileName, System.IO.FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            data = sr.ReadToEnd();
                        }
                    }
                }
            }

            //split on comma and fill list

            checksWithErrors = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
        }

        private void SaveChecksWithErrors()
        {
            string errString = string.Empty;

            foreach (string cid in checksWithErrors)
            {
                errString += cid + ",";
            }

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(errorStringFileName))
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(errString);
                    }
                }
            }
        }

        private string LoadStoredChecks()
        {
            string data = string.Empty;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(checksJsonFileName))
                {
                    using (IsolatedStorageFileStream stream = store.OpenFile(checksJsonFileName, System.IO.FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            data = sr.ReadToEnd();
                        }
                    }
                }
            }

            return data;
        }

        private void StoreChecks(string data)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(checksJsonFileName))
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(data);
                    }
                }
            }
        }

        /// <summary>
        /// Process Check JSON
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<Check> ProcessChecks(string result)
        {
            try
            {
                JObject j = JObject.Parse(result);

                var checks = from c in j["checks"].Children()
                             select new Check
                             {
                                 Id = (int?)c["id"] ?? 0,
                                 CreatedTimeStamp = (int?)c["created"] ?? 0,
                                 LastErrorTime = UnixTimestamp.StampToLocalDateTime((int?)c["lasterrortime"] ?? 0),
                                 LastResponseTime = (int?)c["lastresponsetime"] ?? 0,
                                 Status = (string)c["status"] ?? ""
                             };

                return checks.ToList<Check>();
            }
            catch (Exception)
            {
                return new List<Check>();
            }
        }

        private string GetTileImage(string status)
        {
            if (useTransparentTiles == null)
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("UseTransparentTiles"))
                {
                    useTransparentTiles = (bool)IsolatedStorageSettings.ApplicationSettings["UseTransparentTiles"];
                }
            }

            string extension = ".png";

            if (useTransparentTiles == true)
            {
                extension = ".trans.png";
            }

            string bg = string.Empty;

            switch (status)
            {
                case "up":
                    bg = "/Images/tile.up";
                    break;
                case "down":
                    bg = "/Images/tile.down";
                    break;
                case "unconfirmed_down":
                    bg = "/Images/tile.unconfirmed";
                    break;
                case "unknown":
                    bg = "/Images/tile.unknown";
                    break;
                case "paused":
                    bg = "/Images/tile.paused";
                    break;
                default:
                    bg = "/Images/tile.nostatus";
                    break;
            }

            return bg + extension;
        }

    }
}