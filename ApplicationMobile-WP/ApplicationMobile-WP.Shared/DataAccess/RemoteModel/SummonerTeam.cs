using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.DataAccess.RemoteModel
{
    public class SummonerTeam
    {
        public string id { get; set; }
        public string teamid { get; set; }
        public int summonerid { get; set; }
        public string region { get; set; }
        public int championid { get; set; }
        public int spell1 { get; set; }
        public int spell2 { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public int goldearned { get; set; }
        public int creeps { get; set; }
        public int damagedealt { get; set; }
        public int damagetaken { get; set; }
        public int healing { get; set; }
        public int largestkillingspree { get; set; }
        public int timecrowdcontrol { get; set; }
        public int item1 { get; set; }
        public int item2 { get; set; }
        public int item3 { get; set; }
        public int item4 { get; set; }
        public int item5 { get; set; }
        public int item6 { get; set; }
        public int item7 { get; set; }
    }
}
