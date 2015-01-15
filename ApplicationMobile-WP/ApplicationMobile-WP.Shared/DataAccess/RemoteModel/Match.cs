using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMobile_WP.DataAccess.RemoteModel
{
    public class Match
    {
        public string id { get; set; }
        public int GameLength { get; set; }
        public string GameType { get; set; }
        public string GameMode { get; set; }
        public string SubType { get; set; }
        public int GameID { get; set; }
        public string Region { get; set; }

        public Match ()
        {

        }

        public Match(int gameLength, string gameType, string gameMode, string subType, int gameID)
        {
            GameLength = gameLength;
            GameType = gameType;
            GameMode = gameMode;
            SubType = subType;
            GameID = gameID;

            id = GameID.ToString();
        }
    }
}
