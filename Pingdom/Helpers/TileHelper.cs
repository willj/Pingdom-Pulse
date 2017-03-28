using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Pingdom.Models;
using System.IO.IsolatedStorage;

namespace Pingdom.Helpers
{
    public class TileHelper
    {
        private ShellTile checkTile;
        private bool? backgroundAgentStatus;
        private bool? useTransparentTiles;

        public TileHelper()
        {
            BackgroundAgentHelper agentHelper = new BackgroundAgentHelper();
            backgroundAgentStatus = agentHelper.AgentStatus();

            if (IsolatedStorageSettings.ApplicationSettings.Contains("UseTransparentTiles"))
            {
                useTransparentTiles = (bool)IsolatedStorageSettings.ApplicationSettings["UseTransparentTiles"];
            }
        }

        public bool LoadTile(int checkId)
        {
            checkTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("pinnedcheck=" + checkId));

            if (checkTile != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreateTile(int checkId, string checkName, string status, int responseTime)
        {
            if (!LoadTile(checkId))
            {
                StandardTileData tileData = new StandardTileData();
                tileData.Title = checkName;
                tileData.BackgroundImage = new Uri(GetTileImage(status), UriKind.Relative);

                if (backgroundAgentStatus == true)
                {
                    tileData.BackContent = responseTime.ToString() + " ms";
                }

                ShellTile.Create(new Uri("/Views/ViewCheck.xaml?pinnedcheck=" + checkId, UriKind.Relative), tileData);
            }
        }

        public void UpdateTile(int checkId, string checkName, string status, int responseTime)
        {
            //Always load the tile, it might have been deleted
            LoadTile(checkId);

            if (checkTile != null)
            {
                StandardTileData tileData = new StandardTileData();
                tileData.Title = checkName;
                tileData.BackgroundImage = new Uri(GetTileImage(status), UriKind.Relative);
                tileData.Count = 0;

                if (backgroundAgentStatus == true)
                {
                    tileData.BackContent = responseTime.ToString() + " ms";
                }

                checkTile.Update(tileData);
            }
        }

        private string GetTileImage(string status)
        {
            string extension = ".png";

            if (useTransparentTiles == true)
            {
                extension = ".trans.png";
            }

            if (backgroundAgentStatus == false)
            {
                return "/Images/tile.nostatus" + extension;
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

        public void UpdateAllTiles(List<Check> checks)
        {
            if(backgroundAgentStatus == false){
                return;
            }
            
            foreach (ShellTile t in ShellTile.ActiveTiles)
            {
                Check c = checks.FirstOrDefault(x => "/Views/ViewCheck.xaml?pinnedcheck=" + x.Id == t.NavigationUri.ToString());
                
                if(c != null){
                    StandardTileData tileData = new StandardTileData();
                    tileData.Title = c.Name;
                    tileData.BackgroundImage = new Uri(GetTileImage(c.Status), UriKind.Relative);
                    tileData.BackContent = c.LastResponseTime + " ms";

                    t.Update(tileData);
                }
            }
        }
    }
}
