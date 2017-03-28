using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Pingdom.Views
{
    public partial class About : PhoneApplicationPage
    {
        public string LogoSource { get; set; }

        public About()
        {
            InitializeComponent();

            VersionString.Text = "Version " + App.VersionNumber;

            MbwLogo.Source = new BitmapImage(new Uri((App.CurrentTheme == Theme.Light) ? "/Images/mbw.light.jpg" : "/Images/mbw.dark.jpg", UriKind.Relative));
            
            PulseImage.Source = new BitmapImage(new Uri((App.CurrentTheme == Theme.Light) ? "/Images/pulse.thin.light.jpg" : "/Images/pulse.thin.dark.jpg", UriKind.Relative));
        }

        private void MBWClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask wb = new WebBrowserTask();
                wb.Uri = new Uri("http://www.madebywill.net");
                wb.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Something went wrong, try that again");
            }
        }
    }
}