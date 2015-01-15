using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ApplicationMobile_WP.Model
{
    class GameTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string type = value as string;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            switch(type)
            {
                case "NONE": return loader.GetString("NONE");
                case "NORMAL": return loader.GetString("NORMAL");
                case "BOT": return loader.GetString("BOT");
                case "RANKED_SOLO_5x5": return loader.GetString("RANKED_SOLO_5x5");
                case "RANKED_PREMADE_3x3": return loader.GetString("RANKED_PREMADE_3x3");
                case "RANKED_PREMADE_5x5": return loader.GetString("RANKED_PREMADE_5x5");
                case "ODIN_UNRANKED": return loader.GetString("ODIN_UNRANKED");
                case "RANKED_TEAM_3x3": return loader.GetString("RANKED_TEAM_3x3");
                case "RANKED_TEAM_5x5": return loader.GetString("RANKED_TEAM_5x5");
                case "NORMAL_3x3": return loader.GetString("NORMAL_3x3");
                case "BOT_3x3": return loader.GetString("BOT_3x3");
                case "CAP_5x5": return loader.GetString("CAP_5x5");
                case "ARAM_UNRANKED_5x5": return loader.GetString("ARAM_UNRANKED_5x5");
                case "ONEFORALL_5x5": return loader.GetString("ONEFORALL_5x5");
                case "FIRSTBLOOD_1x1": return loader.GetString("FIRSTBLOOD_1x1");
                case "FIRSTBLOOD_2x2": return loader.GetString("FIRSTBLOOD_2x2");
                case "SR_6x6": return loader.GetString("SR_6x6");
                case "URF": return loader.GetString("URF");
                case "URF_BOT": return loader.GetString("URF_BOT");
                case "NIGHTMARE_BOT": return loader.GetString("NIGHTMARE_BOT");
                case "ASCENSION": return loader.GetString("ASCENSION");
                case "HEXAKILL": return loader.GetString("HEXAKILL");
                default: return loader.GetString("NONE");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
