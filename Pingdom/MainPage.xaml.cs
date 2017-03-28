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
using System.Collections.ObjectModel;
using Microsoft.Phone.Tasks;
using Pingdom.Helpers;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.Text;

namespace Pingdom
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        public MainViewModel VM;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            _isNewPageInstance = true;
            VersionString.Text = "Version " + App.VersionNumber;

            VM = new MainViewModel();
            DataContext = VM;

            SystemTray.SetProgressIndicator(this, VM.GetProgressIndicator());

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Account.Instance.AccountVerified())
            {
                if (!VM.InitComplete)
                {
                    VM.LoadChecks();
                }
            }
        }

        private void MyChecksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyChecksListBox.SelectedItem != null)
            {
                App.SelectedCheck = MyChecksListBox.SelectedItem as Check;

                NavigationService.Navigate(new Uri("/Views/ViewCheck.xaml", UriKind.Relative));
            }

            MyChecksListBox.SelectedIndex = -1;
        }

        private void RefreshData(object sender, EventArgs e)
        {            
            VM.LoadChecks();
        }

        private void QuickCheck(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/QuickCheck.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                if (VM.MyChecks != null)
                {
                    State["MyChecks"] = VM.MyChecks;
                }

                State["InitComplete"] = VM.InitComplete;

                if (!string.IsNullOrEmpty(LoginEmail.Text))
                {
                    State["LoginEmail"] = LoginEmail.Text;
                }

                if (!string.IsNullOrEmpty(LoginPassword.Password))
                {
                    State["LoginPassword"] = LoginPassword.Password;
                }

            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (App.RebindCheckList)
            {
                //One of the checks has changed, this seems to be the only way to force a refresh without reloading data.
                MyChecksListBox.ItemsSource = null;
                MyChecksListBox.ItemsSource = VM.MyChecks;
                App.RebindCheckList = false;
            }

            if (Account.Instance.AccountVerified())
            {
                LoggedOutPopup.IsOpen = false;
                ApplicationBar.IsVisible = true;
            }
            else
            {
                ApplicationBar.IsVisible = false;
                LoggedOutPopup.IsOpen = true;
            }

            if (_isNewPageInstance)
            {
                if (State.ContainsKey("MyChecks") && State.ContainsKey("InitComplete"))
                {
                    VM.RestoreState((ObservableCollection<Check>)State["MyChecks"], (bool)State["InitComplete"]);
                }

                if (State.ContainsKey("LoginEmail"))
                {
                    LoginEmail.Text = (string)State["LoginEmail"];
                }

                if (State.ContainsKey("LoginPassword"))
                {
                    LoginPassword.Password = (string)State["LoginPassword"];
                }
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

        private void CreateAccountClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask wb = new WebBrowserTask();
                wb.Uri = new Uri("http://www.pingdom.com");
                wb.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Something went wrong, try that again");
            }
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginEmail.Text) || string.IsNullOrEmpty(LoginPassword.Password))
            {
                MessageBox.Show("You must enter your Pingdom email address and password to continue.");
                return;
            }

            App.LoginEmail = LoginEmail.Text;
            App.LoginPassword = LoginPassword.Password;

            NavigationService.Navigate(new Uri("/Views/Login.xaml?start=true", UriKind.Relative));
        }

        private void SignOut(object sender, EventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("Are you sure you want to sign out?", "Sign out", MessageBoxButton.OKCancel);

            if (r == MessageBoxResult.OK)
            {
                Account.Instance.Logout();

                VM.InitComplete = false;
                ApplicationBar.IsVisible = false;
                LoggedOutPopup.IsOpen = true;

                VM.MyChecks.Clear();

                BackgroundAgentHelper agentHelper = new BackgroundAgentHelper();
                agentHelper.DisableAgent();

            }
        }

        private void SubmitReview(object sender, EventArgs e)
        {
            try
            {
                MarketplaceReviewTask review = new MarketplaceReviewTask();
                review.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Something went wrong, try that again");
            }
        }

        private void AboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        private void SupportClick(object sender, EventArgs e)
        {
            try
            {
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.AppendLine();
                debugInfo.AppendLine();
                debugInfo.AppendFormat("Current Date/Time: {0}", DateTime.Now.ToString());
                debugInfo.AppendLine();
                debugInfo.AppendFormat("OS Version: {0}", Environment.OSVersion.ToString());
                debugInfo.AppendLine();
                debugInfo.AppendFormat("Device: {0} {1}", Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer, Microsoft.Phone.Info.DeviceStatus.DeviceName);

                EmailComposeTask email = new EmailComposeTask();
                email.To = ""; // Removed
                email.Subject = "Pingdom Pulse v" + App.VersionNumber + " Feedback/Support Request";
                email.Body = debugInfo.ToString();
                email.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Something went wrong, try that again");
            }
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Settings.xaml", UriKind.Relative));
        }

    }
}