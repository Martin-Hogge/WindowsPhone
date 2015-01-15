using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace ApplicationMobile_WP.Model
{
    public class Match
    {
        public Team BlueTeam { get; set; }
        public Team RedTeam { get; set; }
        public string GameType { get; set; }
        public string GameMode { get; set; }
        public string SubType { get; set; }
        public int GameLength { get; set; }
        public int ChampionId { get; set; }
        public KDA KDA { get; set; }
        public Boolean Victory { get; set; }
        public double GoldEarned { get; set; }
        public int CreepsKilled { get; set; }
        public int?[] Items { get; set; }
        public int[] SummonerSpells { get; set; }
        public Brush ResultColor { get; set; }
        public int GameID { get; set; }
        public string ResultText { get; set; }
        public DetailMatchStats Stats { get; set; }

        public Match ()
        {
            Stats = new DetailMatchStats();
            BlueTeam = new Team();
            RedTeam = new Team();
        }

        public Match (Boolean victory)
        {
            SetResult(victory);
            InitializeItems();
            InitializeSummonerSpells();
            KDA = new KDA();
            BlueTeam = new Team();
            RedTeam = new Team();
        }

        public void SetResult(Boolean victory)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if (victory)
            {
                ResultColor = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                ResultText = loader.GetString("VICTORY");
            }
            else
            {
                ResultColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                ResultText = loader.GetString("DEFEAT");
            }   
        }

        public void InitializeItems ()
        {
            Items = new int?[7];
            for(int i = 0; i < Items.Length; i++)
            {
                Items[i] = 0;
            }
        }

        public void InitializeSummonerSpells()
        {
            SummonerSpells = new int[2];
            SummonerSpells[0] = 0;
            SummonerSpells[1] = 1;
        }
    }
}
