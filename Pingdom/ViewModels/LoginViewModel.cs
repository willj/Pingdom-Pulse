using System;
using System.Net;
using System.Windows;
using Pingdom.Helpers;

namespace Pingdom.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public string PadlockIcon { get; private set; }
        private Service.Account accountService;

        public LoginViewModel()
        {
            PadlockIcon = (App.CurrentTheme == Theme.Light) ? "/Images/padlock.light.jpg" : "/Images/padlock.dark.jpg";
        }

        public void Login(string email, string password, Action<bool> callback)
        {
            Account.Instance.StoreAccount(email, password);

            if (accountService == null)
            {
                accountService = new Service.Account();
            }

            SetLoadingStatus(true);

            accountService.GetAccountSettings((status, country) =>
            {
                SetLoadingStatus(false);

                if (!status)
                {
                    callback(false);
                }
                else
                {
                    Account.Instance.VerifyAccount(country);
                    callback(true);
                }
            });

        }
    }
}
