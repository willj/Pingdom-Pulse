using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Linq;
using Microsoft.Phone.Info;

namespace Pingdom.Helpers
{
    public class BackgroundAgentHelper
    {
        string agentName = "PingdomNotifications";
        PeriodicTask pingdomTask;


        public BackgroundAgentHelper()
        {
            pingdomTask = ScheduledActionService.Find(agentName) as PeriodicTask;
        }

        public bool EnableAgent()
        {
            // If the task already exists and the IsEnabled property is false
            // background agents have been disabled by the user
            if (pingdomTask != null && !pingdomTask.IsEnabled)
            {
                MessageBoxResult r = MessageBox.Show("Notifications have been switched off, check background tasks in your phone Settings. Would you like notifications to try and start again next time you start Pingdom Pulse?", "", MessageBoxButton.OKCancel);

                if (r == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            // if (testTask != null && testTask.IsEnabled)
            // Don't test for enabled, because if it's been disabled but allowed to restart it returns as disabled so we can never enable again.
            if (pingdomTask != null)
            {
                DisableAgent();
            }

            pingdomTask = new PeriodicTask(agentName);
            pingdomTask.Description = "Updates Live Tiles when errors occur.";
            pingdomTask.ExpirationTime = DateTime.Now.AddDays(14);

            try
            {
                ScheduledActionService.Add(pingdomTask);
            }
            catch (Exception)
            {
                MessageBox.Show("Notifications couldn't be turned on. Check background tasks in your phone Settings to see if this one is disabled or if you're running too many.");
                return false;
            }

            //Probably worked... :)
            return true;
        }

        public void DisableAgent()
        {
            try
            {
                ScheduledActionService.Remove(agentName);
            }
            catch (Exception)
            {
                //Nothing to do here...
            }
        }

        public bool AgentStatus()
        {
            if (pingdomTask == null || !pingdomTask.IsEnabled)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void ResetTiles()
        {
            ShellTile tile = ShellTile.ActiveTiles.First();

            if (tile != null)
            {
                StandardTileData tileData = new StandardTileData
                {
                    Count = 0,
                    BackContent = ""
                };

                tile.Update(tileData);
            }
        }

        public void CheckAgentStatus()
        {
            // Is this a 256MB Device that doesn't allow BG Agents?
            if (!DeviceCanUseAgents())
            {
                return;
            }

            // Should the agent be running?
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("BackgroundAgentStatus"))
            {
                MessageBoxResult r = MessageBox.Show("Would you like to recieve live tile updates?", "", MessageBoxButton.OKCancel);

                if (r == MessageBoxResult.OK)
                {
                    if (EnableAgent())
                    {
                        IsolatedStorageSettings.ApplicationSettings["BackgroundAgentStatus"] = true;
                    }
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["BackgroundAgentStatus"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            else
            {
                ResetTiles();

                IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"] = false;
                IsolatedStorageSettings.ApplicationSettings.Save();

                if ((bool)IsolatedStorageSettings.ApplicationSettings["BackgroundAgentStatus"])
                {
                    //Refresh the agent
                    EnableAgent();
                }
            }
        }

        public bool DeviceCanUseAgents()
        {
            try
            {
                long ninetyMb = 94371840;
                long result = (long)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");

                if (result < ninetyMb)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // The device has not received the 7.1.1 OS update, which means the device is a 512-MB device.
                return true;
            }
        }

        public void TestAgent()
        {
            ScheduledActionService.LaunchForTest(agentName, TimeSpan.FromSeconds(5));
        }

    }
}
