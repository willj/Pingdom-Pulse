using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace Pingdom.Helpers
{
    public class StatusIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string image = string.Empty;

            switch ((string)value)
            {
                case "up":
                    image = "/Images/status.up.png";
                    break;
                case "down":
                    image = "/Images/status.down.png";
                    break;
                case "unconfirmed_down":
                    image = "/Images/status.unconfirmed_down.png";
                    break;
                case "unknown":
                    if(App.CurrentTheme == Theme.Light){
                        image = "/Images/status.unknown.light.png";
                    }

                    if(App.CurrentTheme == Theme.Dark){
                        image = "/Images/status.unknown.dark.png";
                    }
                    break;
                case "paused":
                    if(App.CurrentTheme == Theme.Light){
                        image = "/Images/status.paused.light.png";
                    }

                    if(App.CurrentTheme == Theme.Dark){
                        image = "/Images/status.paused.dark.png";
                    }
                    break;
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusTextConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if((string)value == "unconfirmed_down"){
                return "unconfirmed";
            } else {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateFormatValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d = (DateTime)value;

            if (Account.Instance.UseLocalDateFormat())
            {
                return d.ToShortDateString() + " " + d.ToLongTimeString();
            }
            else
            {
                return d.ToString("ddd d MMM ") + d.ToLongTimeString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MinuteSuffixConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int mins = 0;

            if (int.TryParse(value.ToString(), out mins))
            {
                if (mins == 1)
                {
                    return string.Format("{0} minute", mins);
                }
                else
                {
                    return string.Format("{0} minutes", mins);
                }
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MiliSecondSuffixConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() + " ms";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SecondsValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int seconds = 0;

            if (int.TryParse(value.ToString(), out seconds))
            {
                if (seconds < 60)
                {
                    return string.Format("{0} secs", seconds);
                }
                
                double mins = Math.Round((double)(seconds / 60));

                if (mins < 60)
                {
                    return string.Format("{0} mins", mins);
                }

                double hrs = Math.Round((double)(mins / 60), 1);

                return string.Format("{0} hrs", hrs);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DurationValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int seconds = 0;

            if (int.TryParse(value.ToString(), out seconds))
            {
                string days = string.Empty;
                string hrs = string.Empty;
                string mins = string.Empty;
                string secs = string.Empty;

                TimeSpan t = new TimeSpan(0, 0, seconds);

                if (t.Days > 0)
                {
                    if (t.Days == 1)
                    {
                        days = string.Format("{0} day ", t.Days);
                    }
                    else
                    {
                        days = string.Format("{0} days ", t.Days);
                    }
                }

                if (t.Hours > 0)
                {
                    if (t.Hours == 1)
                    {
                        hrs = string.Format("{0} hr ", t.Hours);
                    }
                    else
                    {
                        hrs = string.Format("{0} hrs ", t.Hours);
                    }
                }

                if (t.Minutes > 0)
                {
                    if (t.Minutes == 1)
                    {
                        mins = string.Format("{0} min ", t.Minutes);
                    }
                    else
                    {
                        mins = string.Format("{0} mins ", t.Minutes);
                    }
                }

                if (t.Seconds > 0)
                {
                    secs = string.Format("{0} secs", t.Seconds);
                }

                return string.Format("{0}{1}{2}{3}", days, hrs, mins, secs);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImagePathConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage bmp = new BitmapImage(new Uri(value.ToString(), UriKind.Relative));

            ImageBrush brush = new ImageBrush();

            brush.ImageSource = bmp;
            brush.Stretch = Stretch.None;

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ErrorIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string image = string.Empty;

            if ((bool)value == true)
            {
                if (App.CurrentTheme == Theme.Light)
                {
                    image = "/Images/error.light.png";
                }

                if (App.CurrentTheme == Theme.Dark)
                {
                    image = "/Images/error.dark.png";
                }
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
