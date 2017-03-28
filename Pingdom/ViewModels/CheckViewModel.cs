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
using Pingdom.ViewModels;
using Pingdom.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pingdom.ViewModels
{
    public class CheckViewModel : ViewModelBase
    {
        public Check Check { get; set; }
        public CheckSummary CheckSummary { get; set; }
        public ObservableCollection<Outage> Outages { get; set; }

        private bool StatusIsChanging = false;
        private Checks checkService;

        public CheckViewModel()
        {
            Outages = new ObservableCollection<Outage>();
            SetNoContentMessageVisibility(1);
        }
         

        public void LoadSummary()
        {
            if (checkService == null)
            {
                checkService = new Checks();
            }

            if (CheckSummary == null)
            {
                SetLoadingStatus(true);

                checkService.GetCheckSummary(Check.Id, Check.CreatedTimeStamp, (status, summary) =>
                {
                    SetLoadingStatus(false);

                    CheckSummary = summary;

                    NotifyPropertyChanged("CheckSummary");
                });
            }
        }

        public void LoadOutages()
        {
            if (checkService == null)
            {
                checkService = new Checks();
            }

            if(Outages.Count < 1)
            {
                SetLoadingStatus(true);

                checkService.GetOutages(Check.Id, Check.CreatedTimeStamp, (status, outages) =>
                {
                    Outages.Clear();

                    SetLoadingStatus(false);

                    SetNoContentMessageVisibility(outages.Count);

                    foreach (Outage o in outages)
                    {
                        Outages.Add(o);
                    }
                });

            }
        }

        public void RestoreSummary(CheckSummary c)
        {
            CheckSummary = c;

            NotifyPropertyChanged("CheckSummary");
        }

        public void PauseResumeCheck()
        {
            if (!StatusIsChanging)
            {
                StatusIsChanging = true;
                SetLoadingStatus(true);

                if (Check.Status == "paused")
                {
                    checkService.ResumeCheck(Check.Id, (status) =>
                    {
                        if (status)
                        {
                            //Refresh the check data
                            checkService.GetCheck(Check.Id, (s, c) =>
                            {
                                SetLoadingStatus(false);

                                if (status)
                                {
                                    Check.Hostname = c.Hostname;
                                    Check.CreatedTimeStamp = c.CreatedTimeStamp;
                                    Check.Name = c.Name;
                                    Check.Status = c.Status;
                                    Check.LastErrorTime = c.LastErrorTime;
                                    Check.LastResponseTime = c.LastResponseTime;
                                    Check.LastTestTime = c.LastTestTime;
                                    Check.Resolution = c.Resolution;
                                }
                                else
                                {
                                    Check.Status = "unknown";
                                }

                                NotifyPropertyChanged("Check");
                                App.RebindCheckList = true;

                                StatusIsChanging = false;
                            });                                                    
                        }
                    });
                }
                else
                {
                    checkService.PauseCheck(Check.Id, (status) =>
                    {
                        SetLoadingStatus(false);

                        if (status)
                        {
                            Check.Status = "paused";
                            NotifyPropertyChanged("Check");
                            App.RebindCheckList = true;
                        }

                        StatusIsChanging = false;
                    });
                }
            }
            else
            {
                MessageBox.Show("too quick");
            }
        }

        public void LoadCheck(int checkId)
        {
            SetLoadingStatus(true);

            if (checkService == null)
            {
                checkService = new Checks();
            }

            checkService.GetCheck(checkId, (s, c) =>
            {
                SetLoadingStatus(false);

                if (s)
                {
                    Check = c;
                    App.SelectedCheck = c;
                    NotifyPropertyChanged("Check");

                    LoadSummary();
                }
            });          

        }

    }
}
