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
using System.ComponentModel;
using Microsoft.Phone.Shell;

namespace Pingdom.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool IsLoading { get; private set; }

        protected void SetLoadingStatus(bool status)
        {
            if (IsLoading != status)
            {
                IsLoading = status;
                NotifyPropertyChanged("IsLoading");

                if (pi != null)
                {
                    pi.IsVisible = status;
                }
            }
        }

        public Visibility NoContentMessageVisibility { get; private set; }

        protected void SetNoContentMessageVisibility(int itemCount)
        {
            if (itemCount < 1)
            {
                NoContentMessageVisibility = Visibility.Visible;
            }
            else
            {
                NoContentMessageVisibility = Visibility.Collapsed;
            }
            NotifyPropertyChanged("NoContentMessageVisibility");
        }

        public ProgressIndicator pi;

        public ProgressIndicator GetProgressIndicator()
        {
            if (pi == null)
            {
                pi = new ProgressIndicator();
                pi.IsIndeterminate = true;
            }

            return pi;
        }
    }
}
