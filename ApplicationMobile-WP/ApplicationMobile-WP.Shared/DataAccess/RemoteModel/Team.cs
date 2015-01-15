using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.DataAccess.RemoteModel
{
    public class Team
    {
        public string id { get; set; }
        public bool win { get; set; }
        public string matchid { get; set; }

        public Team(string _id, string _matchid)
        {
            id = _id;
            matchid = _matchid;
        }
    }
}
