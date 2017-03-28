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
using Pingdom.Service;
using Pingdom.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;

namespace Pingdom.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Check> MyChecks { get; private set; }
        public bool InitComplete { get; set; }
        private Checks checkService;
        public string MbwLogo { get; private set; }
        public string PulseImage { get; private set; }
        private static string errorStringFileName = "errors.txt";
        private List<string> checksWithErrors;
        private Helpers.BackgroundAgentHelper backgroundAgentHelper;

        public MainViewModel()
        {
            MyChecks = new ObservableCollection<Check>();
            SetNoContentMessageVisibility(1);
            InitComplete = false;

            MbwLogo = (App.CurrentTheme == Theme.Light) ? "/Images/mbw.light.jpg" : "/Images/mbw.dark.jpg";
            PulseImage = (App.CurrentTheme == Theme.Light) ? "/Images/pulse.head.light.jpg" : "/Images/pulse.head.dark.jpg";
        }

        public void LoadChecks()
        {
            if (checkService == null)
            {
                checkService = new Checks();
            }

            SetLoadingStatus(true);

            LoadChecksWithErrors();

            checkService.GetCheckList((status, mychecks) =>
            {
                SetLoadingStatus(false);

                SetNoContentMessageVisibility(mychecks.Count);

                MyChecks.Clear();

                foreach (Check c in mychecks)
                {
                    if (checksWithErrors.Contains(c.Id.ToString()))
                    {
                        c.HasRecentError = true;
                    }

                    MyChecks.Add(c);
                }

                InitComplete = true;

                BackgroundWorker bw = new BackgroundWorker();

                bw.DoWork += (s, e) =>
                {
                    Helpers.TileHelper t = new Helpers.TileHelper();
                    t.UpdateAllTiles(mychecks);
                };

                bw.RunWorkerAsync();

            });

            if (backgroundAgentHelper == null)
            {
                backgroundAgentHelper = new Helpers.BackgroundAgentHelper();
                backgroundAgentHelper.CheckAgentStatus();
            }

        }

        public void RestoreState(ObservableCollection<Check> c, bool initStatus){
            MyChecks = c;

            NotifyPropertyChanged("MyChecks");

            SetNoContentMessageVisibility(c.Count);

            InitComplete = initStatus;
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

                    store.DeleteFile(errorStringFileName);
                }
            }

            checksWithErrors = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
        }
    }
}
