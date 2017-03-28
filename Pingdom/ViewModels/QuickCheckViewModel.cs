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
using Pingdom.Models;
using Pingdom.Service;


namespace Pingdom.ViewModels
{
    public class QuickCheckViewModel : ViewModelBase
    {
        public SingleCheck CheckResult { get; private set; }
        public Visibility ResultVisibility { get; set; }
        public string HostAddress { get; set; }
        private Checks checkService;

        public QuickCheckViewModel()
        {
            ResultVisibility = Visibility.Collapsed;
            SetNoContentMessageVisibility(1);
            HostAddress = string.Empty;
        }

        public void RunCheck(string host){

            if(checkService == null){
                checkService = new Checks();
            }

            SetLoadingStatus(true);

            checkService.QuickCheck(host, (status, result) =>{

                SetLoadingStatus(false);

                if(status){

                    SetNoContentMessageVisibility(1);

                    CheckResult = result;
                    NotifyPropertyChanged("CheckResult");

                    HostAddress = host;
                    NotifyPropertyChanged("HostAddress");

                    ResultVisibility = Visibility.Visible;
                } else {
                    SetNoContentMessageVisibility(0);

                    ResultVisibility = Visibility.Collapsed;
                }

                NotifyPropertyChanged("ResultVisibility");
            });
        }

        public void RestoreState(SingleCheck c, string h)
        {
            CheckResult = c;
            NotifyPropertyChanged("CheckResult");

            HostAddress = h;
            NotifyPropertyChanged("HostAddress");

            SetNoContentMessageVisibility(1);

            ResultVisibility = Visibility.Visible;
            NotifyPropertyChanged("ResultVisibility");
        }

    }
}
