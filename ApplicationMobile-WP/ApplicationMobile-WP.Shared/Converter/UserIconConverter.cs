using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ApplicationMobile_WP.Model
{
    public class UserIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var idIcon = (int)value;
            return new BitmapImage(new Uri
                (
                    String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/profileicon/{1}.png", RiotAPIServices.ddVersion, idIcon))
                ); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
