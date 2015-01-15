using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ApplicationMobile_WP.Model
{
    public class SummonerSpellConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int summonerSpellId = (int)value;
            string key = RiotAPIServices.AllSummonerSpells[summonerSpellId];
            return new BitmapImage(new Uri
                (
                    String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/spell/{1}.png", RiotAPIServices.ddVersion, key))
                ); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
