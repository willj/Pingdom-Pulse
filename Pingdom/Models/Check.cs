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

namespace Pingdom.Models
{
    public class Check
    {
        public string Hostname { get; set; }
        public int Id { get; set; }
        public int CreatedTimeStamp { get; set; }
        public DateTime LastErrorTime { get; set; }
        public int LastResponseTime { get; set; }
        public DateTime LastTestTime { get; set; }
        public string Name { get; set; }
        public int Resolution { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public bool HasRecentError { get; set; }
    }

    public class CheckSummary
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AverageResponse { get; set; }
        public int TotalUp { get; set; }
        public int TotalDown { get; set; }
        public int TotalUnknown { get; set; }

        public string UptimePercentString
        {
            get 
            { 
                //Unknown time is ignored for this calculation

                if (TotalDown == 0)
                {
                    return "100%";
                }

                double TotalTime = TotalUp + TotalDown;

                double percent = (TotalUp / TotalTime) * 100;

                return Math.Round(percent, 2).ToString() + "%";
            }
        }

        public string StartEndDates
        {
            get
            {
                return string.Format("{0:M} - {1:M}", StartTime, EndTime); 
            }
        }
    }

    public class SingleCheck
    {
        public string Status { get; set; }
        public int ResponseTime { get; set; }
        public string ProbeLocation { get; set; }
    }

    public class Outage
    {
        public string Status { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }

        public string Duration
        {
            get
            {
                double TotalSeconds = TimeTo.Subtract(TimeFrom).TotalSeconds;

                return TotalSeconds.ToString();
            }
        }

    }

}
