using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Controls;
using Pingdom.Helpers;
using Microsoft.Phone.Shell;

namespace Pingdom.Views
{
    public partial class Settings : PhoneApplicationPage
    {
        private bool ignoreDateFormatSelection = true;
        private bool ignoreCheckBoxAction = true;
        private BackgroundAgentHelper agentHelper;

        public Settings()
        {
            InitializeComponent();

            Loaded += new System.Windows.RoutedEventHandler(Settings_Loaded);

            agentHelper = new BackgroundAgentHelper();

            //Hide BG Agent stuff 
            if (!agentHelper.DeviceCanUseAgents())
            {
                NotificationStatusCheckbox.Visibility = System.Windows.Visibility.Collapsed;
                NotificationDesc.Visibility = System.Windows.Visibility.Collapsed;
            }

            LoadDateFormats();
        }

        void Settings_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (agentHelper.AgentStatus())
            {
                NotificationStatusCheckbox.IsChecked = true;
            }
            else
            {
                NotificationStatusCheckbox.IsChecked = false;
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("UseTransparentTiles"))
            {
                TransparentTileCheckbox.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["UseTransparentTiles"];
            }

            ignoreCheckBoxAction = false;
        }

        private void LoadDateFormats()
        {
            string[] dateFormats = { DateTime.Now.ToString("ddd d MMM ", System.Globalization.CultureInfo.CurrentUICulture) + DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() };

            DateFormatPicker.ItemsSource = dateFormats;

            if (Account.Instance.UseLocalDateFormat())
            {
                DateFormatPicker.SelectedIndex = 1;
            }

            //Added after all items have been added to prevent setup events being handled.
            DateFormatPicker.SelectionChanged += DateFormat_SelectionChanged;
        }

        private void DateFormat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //Selection change fires each time an item is added and to set an initial selection.
            //This ignores this setup.
            if (ignoreDateFormatSelection)
            {
                ignoreDateFormatSelection = false;
                return;
            }

            if (DateFormatPicker.SelectedIndex == 1)
            {
                IsolatedStorageSettings.ApplicationSettings["date_format"] = "local";
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Remove("date_format");
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private void NotificationStatusCheckbox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ignoreCheckBoxAction)
            {
                return;
            }

            IsolatedStorageSettings.ApplicationSettings["BackgroundAgentStatus"] = true;
            IsolatedStorageSettings.ApplicationSettings.Save();

            agentHelper.EnableAgent();
        }

        private void NotificationStatusCheckbox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ignoreCheckBoxAction)
            {
                return;
            }

            IsolatedStorageSettings.ApplicationSettings["BackgroundAgentStatus"] = false;
            IsolatedStorageSettings.ApplicationSettings.Save();

            agentHelper.DisableAgent();

        }

        private void TestAgent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (agentHelper != null)
            {
                agentHelper.TestAgent();
            }
        }

        private void TransparentTileCheckbox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ignoreCheckBoxAction)
            {
                return;
            }

            IsolatedStorageSettings.ApplicationSettings["UseTransparentTiles"] = true;
            IsolatedStorageSettings.ApplicationSettings.Save();

            // Switch to a transparent icon
            ShellTile MainTile = ShellTile.ActiveTiles.First();

            StandardTileData tileData = new StandardTileData();
            tileData.BackgroundImage = new Uri("/icon173xTransparent.png", UriKind.Relative);
            tileData.Title = "Pingdom";

            MainTile.Update(tileData);

        }

        private void TransparentTileCheckbox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ignoreCheckBoxAction)
            {
                return;
            }

            IsolatedStorageSettings.ApplicationSettings["UseTransparentTiles"] = false;
            IsolatedStorageSettings.ApplicationSettings.Save();

            // Switch to standard icon
            ShellTile MainTile = ShellTile.ActiveTiles.First();

            StandardTileData tileData = new StandardTileData();
            tileData.BackgroundImage = new Uri("/Background.png", UriKind.Relative);
            tileData.Title = "Pingdom";

            MainTile.Update(tileData);

        }
    }
}