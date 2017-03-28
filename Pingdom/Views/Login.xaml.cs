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

namespace Pingdom.Views
{
    public partial class Login : PhoneApplicationPage
    {
        private LoginViewModel VM;

        public Login()
        {
            InitializeComponent();

            VM = new LoginViewModel();
            DataContext = VM;
            Loaded += new RoutedEventHandler(Login_Loaded);
        }

        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            VM.Login(App.LoginEmail, App.LoginPassword, (status) =>
            {
                if (!status)
                {
                    //Don't need to do this as the base error handling does it...
                    //MessageBox.Show("There was a problem authenticating your account, check your email address and password and try again.");
                }

                NavigationService.GoBack();

            });
        }
    }
}