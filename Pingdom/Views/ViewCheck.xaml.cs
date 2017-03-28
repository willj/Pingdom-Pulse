using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Pingdom.ViewModels;
using Pingdom.Models;
using Pingdom.Helpers;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Pingdom.Views
{
    public partial class ViewCheck : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        private CheckViewModel VM;
        private TileHelper tileHelper;

        public ViewCheck()
        {
            InitializeComponent();

            _isNewPageInstance = true;

            VM = new CheckViewModel();
            DataContext = VM;

            SystemTray.SetProgressIndicator(this, VM.GetProgressIndicator());
            VM.PropertyChanged += VM_PropertyChanged;

            if (App.SelectedCheck != null)
            {
                VM.Check = App.SelectedCheck;

                SetupAppBarButtons();

                Loaded += new RoutedEventHandler(ViewCheck_Loaded);
            }
        }

        void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Check")
            {
                SetupAppBarButtons();
                UpdateTile(VM.Check.Id);
            }
        }

        void ViewCheck_Loaded(object sender, RoutedEventArgs e)
        {
            VM.LoadSummary();
        }

        void SetupAppBarButtons()
        {
            if (VM.Check == null)
            {
                return;
            }

            ApplicationBarIconButton pauseResumeButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

            if (VM.Check.Status == "paused")
            {
                pauseResumeButton.Text = "resume";
                pauseResumeButton.IconUri = new Uri("/Images/appbar.transport.play.rest.png", UriKind.Relative);
            }
            else
            {
                pauseResumeButton.Text = "pause";
                pauseResumeButton.IconUri = new Uri("/Images/appbar.transport.pause.rest.png", UriKind.Relative);
            }

            if (VM.Check.Type != "http" && VM.Check.Type != "httpcustom" && VM.Check.Type != "ping")
            {
                ApplicationBarIconButton browserButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                browserButton.IsEnabled = false;
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                if (VM.CheckSummary != null)
                {
                    State["CheckSummary"] = VM.CheckSummary;
                }

                if (VM.Outages != null)
                {
                    State["Outages"] = VM.Outages;
                }

                State["SelectedPivotIndex"] = SummaryPivot.SelectedIndex;
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            int queryCheckId = 0;

            if (_isNewPageInstance)
            {
                if (NavigationContext.QueryString.ContainsKey("pinnedcheck"))
                {        
                    if (int.TryParse(NavigationContext.QueryString["pinnedcheck"], out queryCheckId))
                    {
                        VM.LoadCheck(queryCheckId);
                    }
                }

                if (State.ContainsKey("SelectedPivotIndex"))
                {
                    SummaryPivot.SelectedIndex = (int)State["SelectedPivotIndex"];
                }

                if (State.ContainsKey("CheckSummary"))
                {
                    VM.RestoreSummary((CheckSummary)State["CheckSummary"]);
                }

                if (State.ContainsKey("Outages"))
                {
                    VM.Outages = State["Outages"] as ObservableCollection<Outage>;
                }
            }

            _isNewPageInstance = false;

            if (queryCheckId == 0)
            {
                queryCheckId = VM.Check.Id;
            }

            UpdateTile(queryCheckId);

            base.OnNavigatedTo(e);
        }

        private void SummaryPivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            Pivot p = sender as Pivot;

            if (p.SelectedIndex == 2)
            {
                VM.LoadOutages();
            }
        }

        private void PauseResumeCheck(object sender, EventArgs e)
        {
            VM.PauseResumeCheck();
        }

        private void OpenUrl(object sender, EventArgs e)
        {
            string host = VM.Check.Hostname;

            if (!host.Contains("http"))
            {
                host = "http://" + host;
            }

            try
            {
                WebBrowserTask wb = new WebBrowserTask();
                wb.Uri = new Uri(host);
                wb.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Something went wrong, try that again");
            }
        }

        private void PinCheck(object sender, EventArgs e)
        {
            if (VM.Check != null)
            {
                if (tileHelper == null)
                {
                    tileHelper = new TileHelper();
                }

                tileHelper.CreateTile(VM.Check.Id, VM.Check.Name, VM.Check.Status, VM.Check.LastResponseTime);
            }
        }

        private void UpdateTile(int checkId)
        {
            if (tileHelper == null)
            {
                tileHelper = new TileHelper();
            }

            ApplicationBarIconButton pinBtn = ApplicationBar.Buttons[2] as ApplicationBarIconButton;

            if (tileHelper.LoadTile(checkId))
            {
                pinBtn.IsEnabled = false;

                if (VM.Check != null)
                {
                    tileHelper.UpdateTile(checkId, VM.Check.Name, VM.Check.Status, VM.Check.LastResponseTime);
                }
            }
            else
            {
                pinBtn.IsEnabled = true;
            }
        }
    }
}