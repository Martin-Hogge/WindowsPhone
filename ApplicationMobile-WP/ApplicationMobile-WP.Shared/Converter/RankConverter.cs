using ApplicationMobile_WP.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ApplicationMobile_WP.Model
{
    public class RankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Rank rank = (Rank)value;
            switch (rank.Tier)
            {
                case "BRONZE": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Bronze.png"));

                case "SILVER": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Silver.png"));

                case "GOLD": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Gold.png"));

                case "PLATINUM": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Platinium.png"));

                case "DIAMOND": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Diamond.png"));

                case "MASTER": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Master.png"));
                    
                case "CHALLENGER": return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Challenger.png"));

                default: return new BitmapImage(new Uri("ms-appx:/Images/Ranks/Unranked.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
