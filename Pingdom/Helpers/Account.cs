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
using System.IO.IsolatedStorage;
using System.Text;
using System.Security.Cryptography;

namespace Pingdom.Helpers
{
    public sealed class Account
    {
        private static readonly Account instance = new Account();

        private Account()
        {
            LoadAccount();
        }

        /// <summary>
        /// Get instance of the Account Singleton
        /// </summary>
        public static Account Instance
        {
            get
            {
                return instance;
            }
        }

        private string authString;
        private bool accountVerified;

        /// <summary>
        /// Returns Pingdom App Key
        /// </summary>
        /// <returns></returns>
        public string GetAppKey()
        {
            // Removed
        }


        /// <summary>
        /// Loads account from App Settings
        /// </summary>
        public void LoadAccount()
        {
            
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
        }

        /// <summary>
        /// Check account details are stored and have been verified.
        /// </summary>
        /// <returns></returns>
        public bool AccountVerified()
        {
            if (!string.IsNullOrEmpty(authString) && accountVerified == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetAuthString()
        {
            return this.authString;
        }

        public void Logout()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }

        public void StoreAccount(string email, string password)
        {
            byte[] byteArray = UTF8Encoding.UTF8.GetBytes(email + ":" + password);

            string base64 = System.Convert.ToBase64String(byteArray);

            byte[] unprotectedAuthBytes = UTF8Encoding.UTF8.GetBytes(base64);
            byte[] protectedAuthBytes = ProtectedData.Protect(unprotectedAuthBytes, null);

            IsolatedStorageSettings.ApplicationSettings["protected_auth_string"] = protectedAuthBytes;
            IsolatedStorageSettings.ApplicationSettings["email"] = email;
            IsolatedStorageSettings.ApplicationSettings["account_verified"] = false;

            IsolatedStorageSettings.ApplicationSettings.Save();

            LoadAccount();
        }

        public void VerifyAccount(string country)
        {
            IsolatedStorageSettings.ApplicationSettings["country"] = country;
            IsolatedStorageSettings.ApplicationSettings["account_verified"] = true;

            IsolatedStorageSettings.ApplicationSettings.Save();

            LoadAccount();
        }

        public bool UseLocalDateFormat()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("date_format"))
            {
                if (IsolatedStorageSettings.ApplicationSettings["date_format"].ToString() == "local")
                {
                    return true;
                }
            }

            return false;
        }

    }
}
