using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ApplicationMobile_WP.Model
{
    public class ItemIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int itemId = (int)value;
            if(itemId != 0)
            {
                return new BitmapImage(new Uri
                (
                    String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/item/{1}.png", RiotAPIServices.ddVersion, itemId))
                ); 
            }
            else
            {
                return new BitmapImage(new Uri("ms-appx:/Images/NoItem.png"));
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
