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
using Microsoft.Phone.Shell;

namespace Pingdom.Views
{
    public partial class QuickCheck : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        public QuickCheckViewModel VM;

        public QuickCheck()
        {
            InitializeComponent();

            _isNewPageInstance = true;

            VM = new QuickCheckViewModel();
            DataContext = VM;

            SystemTray.SetProgressIndicator(this, VM.GetProgressIndicator());
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.Enter)
            {
                RunCheck();
            }
        }

        private void RunCheckClicked(object sender, EventArgs e)
        {
            RunCheck();
        }

        private void RunCheck()
        {
            if (string.IsNullOrEmpty(HostTextBox.Text))
            {
                MessageBox.Show("You must enter a host/domain name to check.");
            }
            else
            {
                this.Focus();

                string host = HostTextBox.Text;

                if (host.StartsWith("http://"))
                {
                    host = host.Replace("http://", "");
                    HostTextBox.Text = host;
                }

                VM.RunCheck(host);
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                if (!string.IsNullOrEmpty(HostTextBox.Text))
                {
                    State["HostTextBox"] = HostTextBox.Text;
                }

                if (VM.HostAddress != null)
                {
                    State["HostAddress"] = VM.HostAddress;
                }

                if (VM.CheckResult != null)
                {
                    State["CheckResult"] = VM.CheckResult;
                }
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                if (State.ContainsKey("HostTextBox"))
                {
                    HostTextBox.Text = (string)State["HostTextBox"];
                }

                if (State.ContainsKey("HostAddress") && State.ContainsKey("CheckResult"))
                {
                    VM.RestoreState((Models.SingleCheck)State["CheckResult"], (string)State["HostAddress"]);
                }
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

    }
}