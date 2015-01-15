using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace ApplicationMobile_WP.Model
{
    public class Summoner
    {
        public String Name { get; set; }
        public int IdIcon { get; set; }
        public int ChampionId { get; set; }
        public Rank RankSolo { get; set; }
        public Rank RankTeam5v5 { get; set; }
        public Rank RankTeam3v3 { get; set; }
        public KDA KDA { get; set; }
        public int Level { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public int ID { get; set; }
        public Stats Stats { get; set; }
        public ObservableCollection<Match> MatchHistory { get; set; }
        public string Region { get; set; }
        public bool IsFavorite { get; set; }
        public int Score { get; set; }


        public Summoner()
        {
            RankSolo = new Rank();
            RankTeam3v3 = new Rank();
            RankTeam5v5 = new Rank();
            KDA = new KDA();
            Stats = new Stats();
            MatchHistory = new ObservableCollection<Match>();
        }

        public Summoner (int id, int championID) :base()
        {
            ID = id;
            ChampionId = championID;
        }

        public Summoner(String name, int id, int championID)
        {
            Name = name;
            ID = id;
            ChampionId = championID;
        }

        public Summoner(String name, int idIcon) : this()
        {
            Name = name;
            IdIcon = idIcon;
        }

        public Summoner (int id, int idIcon, string name, string region) :this(name, idIcon)
        {
            ID = id;
            Region = region;
        }
    }
}
