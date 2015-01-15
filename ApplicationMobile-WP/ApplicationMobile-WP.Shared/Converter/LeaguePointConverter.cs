using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ApplicationMobile_WP.Model
{
    public class LeaguePointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String tier = (String)value;
            if (tier.Equals("UNRANKED"))
                return "";
            return "LP";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
