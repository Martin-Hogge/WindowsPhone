using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.Model
{
    public class Rank
    {
        public String Tier { get; set; }
        public String Division { get; set; }
        public int? LeaguePoints { get; set; }

        public Rank()
        {
            Tier = "UNRANKED";
        }

        public Rank (String tier, String division, int lp)
        {
            Tier = tier;
            Division = division;
            LeaguePoints = lp;
        }
    }
}
