using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ApplicationMobile_WP.Model
{
    public class ChampionIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int championId = (int)value;
            string championName = RiotAPIServices.AllChampions[championId];

            return new BitmapImage(new Uri
                (
                    String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/champion/{1}.png", RiotAPIServices.ddVersion, championName))
                ); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
