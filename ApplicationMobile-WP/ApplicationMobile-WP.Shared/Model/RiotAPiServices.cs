using ApplicationMobile_WP.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using ApplicationMobile_WP.DataAccess.RemoteModel;
using System.Collections.ObjectModel;
using ApplicationMobile_WP.DataAccess;

namespace ApplicationMobile_WP.Model
{
    public class RiotAPIServices
    {
        private enum typeRequest { SUMMONER_BY_NAME, SUMMONER_BY_ID, LEAGUE, MATCH_HISTORY, RANKED_STATS,
                                   REALM, CHAMPIONS, SUMMONER_SPELLS, SUMMONER_NAMES_BY_IDS, ONE_MATCH};
        private const string apiKey = "e385e065-7997-445c-a75b-d7341085a550";
        private bool isRankedLeague = true, isRankedStats = true;
        public static string ddVersion;
        public static IDictionary<int, string> AllChampions;
        public static IDictionary<int, string> AllSummonerSpells;

        public async Task<Summoner> GetSummoner(string name, string region)
        {          
            Summoner summoner = new Summoner();

            var objectSummoner = await ExecuteSummonerRequest(name, region);
            summoner = SetSummonerInformations(objectSummoner, summoner);

            var objectLeague = await ExecuteLeagueRequest(summoner, region);
            if(isRankedLeague)
                summoner = SetLeagueInformations(summoner, objectLeague);

            var objectRankedStats = await ExecuteRankedStatsRequest(summoner, region);
            if(isRankedStats)
                summoner = SetSummonerStatistics(summoner, objectRankedStats);

            var objectMatchHistory = await ExecuteMatchHistoryRequest(summoner, region);
            summoner = SetMatchHistoryInformation(objectMatchHistory, summoner, region);

            RemoteDataAccessManager.MatchUpdated.Add(new KeyValuePair<int, string>(summoner.ID, region), false);
            UpdateMatchToInsert(summoner.MatchHistory,region,summoner.ID);

            summoner.Score = await RemoteDataAccessManager.GetScoreForSummoner(summoner.ID, region);
            summoner.Region = region;

            return summoner;
        }

        private void UpdateMatchToInsert(ObservableCollection<Match> matchHistory,string region, int SummonerId)
        {
            List<KeyValuePair<int,string>> idsOfMatchToInsert = new List<KeyValuePair<int,string>>();
            List<KeyValuePair<int, KeyValuePair<int, int>>> MatchingSummonerChampion = new List<KeyValuePair<int, KeyValuePair<int, int>>>();
            foreach(Match match in matchHistory)
            {
                idsOfMatchToInsert.Add(new KeyValuePair<int,string>(match.GameID,region));
                foreach(Summoner s in match.BlueTeam.Users)
                {
                    KeyValuePair<int,int> SummonerIdAndChampionId = new KeyValuePair<int,int>(s.ID, s.ChampionId);
                    MatchingSummonerChampion.Add(new KeyValuePair<int, KeyValuePair<int, int>>(match.GameID, SummonerIdAndChampionId));
                }
                foreach (Summoner s in match.RedTeam.Users)
                {
                    KeyValuePair<int, int> SummonerIdAndChampionId = new KeyValuePair<int, int>(s.ID, s.ChampionId);
                    MatchingSummonerChampion.Add(new KeyValuePair<int, KeyValuePair<int, int>>(match.GameID, SummonerIdAndChampionId));
                }
            }
            RemoteDataAccessManager.AddListOfMatch(idsOfMatchToInsert, MatchingSummonerChampion, new KeyValuePair<int,string>(SummonerId,region));
        }

        public static async Task InitializeSummonerSpells ()
        {
            string request = new RiotAPIServices().CreateRequest(typeRequest.SUMMONER_SPELLS, "euw", new string[] { "global" });

            var output = await new RiotAPIServices().GetAPIResponse(request, typeRequest.SUMMONER_SPELLS);
            var objectSummonerSpells = JObject.Parse(output)["data"];

            AllSummonerSpells = new Dictionary<int, string>();
            var currentSummonerSpell = objectSummonerSpells.First;
            while (currentSummonerSpell != null)
            {
                AllSummonerSpells.Add((int)currentSummonerSpell.First["id"], (string)currentSummonerSpell.First["key"]);
                currentSummonerSpell = currentSummonerSpell.Next;
            }
        }

        public static async Task InitializeChampions ()
        {
            string request = new RiotAPIServices().CreateRequest(typeRequest.CHAMPIONS, "euw", new string[] { "global" });

            var output = await new RiotAPIServices().GetAPIResponse(request, typeRequest.CHAMPIONS);
            var objectChampions = JObject.Parse(output)["data"];

            AllChampions = new Dictionary<int,string>();
            var currentChampion = objectChampions.First;
            while(currentChampion != null)
            {
                AllChampions.Add((int)currentChampion.First["id"], (string)currentChampion.First["key"]);
                currentChampion = currentChampion.Next;
            }
        }

        public static async Task SetDDVersion()
        {
            string request = new RiotAPIServices().CreateRequest(typeRequest.REALM, "euw", new string[] { "global" });

            var output = await new RiotAPIServices().GetAPIResponse(request, typeRequest.REALM);
            var objectRealm = JObject.Parse(output);
            ddVersion = (string)objectRealm["v"];
        }

        private Summoner SetMatchHistoryInformation(JToken objectMatchHistory, Summoner summoner, string region)
        {
            var currentGame = objectMatchHistory.First;
            while(currentGame != null)
            {
                Match match = new Match((Boolean)currentGame["stats"]["win"]);

                SetGeneralMatchInformation(currentGame, match);
                SetPlayersInformation(summoner, region, currentGame, match);
                
                for (int i = 0; i < match.Items.Length; i++)
                {
                    match.Items[i] = currentGame["stats"].Value<int?>(String.Format("item{0}", i)) ?? 0;
                }

                summoner.MatchHistory.Add(match);
                currentGame = currentGame.Next;
            }
            return summoner;
        }

        private static void SetPlayersInformation(Summoner summoner, string region, JToken currentGame, Match match)
        {
            var players = currentGame["fellowPlayers"];
            if (players != null)
            {
                var currentPlayer = players.First;

                while (currentPlayer != null)
                {
                    Summoner s = new Summoner((int)currentPlayer["summonerId"],
                                              (int)currentPlayer["championId"]);
                    s.Region = region;
                    if ((int)currentPlayer["teamId"] == 100)
                        match.BlueTeam.Users.Add(s);
                    else
                        match.RedTeam.Users.Add(s);
                    currentPlayer = currentPlayer.Next;
                }
            }

            if (match.BlueTeam.Users.Count < match.RedTeam.Users.Count)
                match.BlueTeam.Users.Add(new Summoner(summoner.Name, summoner.ID, (int)currentGame["championId"]));
            else
                match.RedTeam.Users.Add(new Summoner(summoner.Name, summoner.ID, (int)currentGame["championId"]));
        }

        private static void SetGeneralMatchInformation(JToken currentGame, Match match)
        {
            match.GameType = (string)currentGame["gameType"];
            match.GameID = (int)currentGame["gameId"];
            match.GameMode = (string)currentGame["gameMode"];
            match.SubType = (string)currentGame["subType"];
            match.ChampionId = (int)currentGame["championId"];
            match.SummonerSpells[0] = (int)currentGame["spell1"];
            match.SummonerSpells[1] = (int)currentGame["spell2"];
            match.CreepsKilled = currentGame["stats"].Value<int?>("minionsKilled") ?? 0;
            match.GoldEarned = (int)currentGame["stats"]["goldEarned"];
            match.KDA.Kill = currentGame["stats"].Value<double?>("championsKilled") ?? 0;
            match.KDA.Death = currentGame["stats"].Value<double?>("numDeaths") ?? 0;
            match.KDA.Assist = currentGame["stats"].Value<double?>("assists") ?? 0;
            match.GameLength = (int)currentGame["stats"]["timePlayed"] / 60;

            match.Stats = new DetailMatchStats();

            match.Stats.DamageDealt = currentGame["stats"].Value<int?>("totalDamageDealtToChampions") ?? 0;
            match.Stats.DamageTaken = currentGame["stats"].Value<int?>("totalDamageTaken") ?? 0;
            match.Stats.GoldEarned = (int)match.GoldEarned;
            match.Stats.Healing = currentGame["stats"].Value<int?>("totalHeal") ?? 0;
            match.Stats.KillSimMax = currentGame["stats"].Value<int?>("largestMultiKill") ?? 0;
            match.Stats.MinionsSlain = match.CreepsKilled;
            match.Stats.TimeCrowdControl = currentGame["stats"].Value<int?>("totalTimeCrowdControlDealt") ?? 0;
        }

        private async Task<JToken> ExecuteMatchHistoryRequest(Summoner summoner, string region)
        {
            string request = CreateRequest(typeRequest.MATCH_HISTORY, region, new string[] { summoner.ID.ToString() });

            var output = await GetAPIResponse(request, typeRequest.MATCH_HISTORY);
            var objectMatchHistory = JObject.Parse(output)["games"];
            return objectMatchHistory;
        }

        private Summoner SetSummonerStatistics(Summoner summoner, JToken objectRankedStats)
        {
            //SELECT champion with ID 0
            var globalStats = objectRankedStats.FirstOrDefault(jt => (int)jt["id"] == 0);

            summoner.Stats.Pentakill = (int)globalStats["stats"]["totalPentaKills"];
            summoner.Stats.Quadrakill = (int)globalStats["stats"]["totalQuadraKills"];
            summoner.Stats.Triple = (int)globalStats["stats"]["totalTripleKills"];
            summoner.Stats.Double = (int)globalStats["stats"]["totalDoubleKills"];
            summoner.Stats.ChampionsSlain = (int)globalStats["stats"]["totalChampionKills"];
            summoner.Stats.DamageDealt = (int)globalStats["stats"]["totalDamageDealt"];
            summoner.Stats.Healing = (int)globalStats["stats"]["totalHeal"];
            summoner.Stats.KillingSpree = (int)globalStats["stats"]["killingSpree"];
            summoner.Stats.LargestKillingSpree = (int)globalStats["stats"]["maxLargestKillingSpree"];
            summoner.Stats.LongestGame = (int)globalStats["stats"]["maxTimePlayed"];
            summoner.Stats.LongestTimeAlive = (int)globalStats["stats"]["maxTimeSpentLiving"];
            summoner.Stats.MagicDamageDealt = (int)globalStats["stats"]["totalMagicDamageDealt"];
            summoner.Stats.MinionsSlain = (int)globalStats["stats"]["totalMinionKills"];
            summoner.Stats.MonstersSlain = (int)globalStats["stats"]["totalNeutralMinionsKilled"];
            summoner.Stats.MostKills = (int)globalStats["stats"]["maxChampionsKilled"];
            summoner.Stats.PhysicalDamageDealt = (int)globalStats["stats"]["totalPhysicalDamageDealt"];
            summoner.Stats.MostDeaths = (int)globalStats["stats"]["maxNumDeaths"];
            summoner.Stats.TurretsDestroyed = (int)globalStats["stats"]["totalTurretsKilled"];
            summoner.Stats.Assists = (int)globalStats["stats"]["totalAssists"];
            summoner.Stats.Deaths = (int)globalStats["stats"]["totalDeathsPerSession"];

            summoner.Victories = (int)globalStats["stats"]["totalSessionsWon"];
            summoner.Defeats = (int)globalStats["stats"]["totalSessionsLost"];
            summoner.Stats.NbGamesPlayed = summoner.Victories + summoner.Defeats;

            if (summoner.Stats.NbGamesPlayed > 0)
            {
                summoner.KDA.Kill = (double)summoner.Stats.ChampionsSlain / summoner.Stats.NbGamesPlayed;
                summoner.KDA.Assist = (double)summoner.Stats.Assists / summoner.Stats.NbGamesPlayed;
                summoner.KDA.Death = (double)summoner.Stats.Deaths / summoner.Stats.NbGamesPlayed;
            }
            else
            {
                summoner.KDA = new KDA(0, 0, 0);
            }
            return summoner;
        }

        private async Task<JToken> ExecuteRankedStatsRequest(Summoner summoner, string region)
        {
            string request = CreateRequest(typeRequest.RANKED_STATS, region, new string[] { summoner.ID.ToString(), "4" });

            var output = await GetAPIResponse(request, typeRequest.RANKED_STATS);
            if(isRankedStats)
            {
                var objectRankedStats = JObject.Parse(output)["champions"];
                return objectRankedStats;
            }
            else
            {
                return null;
            } 
        }

        private Summoner SetLeagueInformations(Summoner summoner, JToken objectLeague)
        {
            while (objectLeague != null)
            {
                var entries = objectLeague["entries"];
                var currentEntry = entries.First;
                string participantID = (string)objectLeague["participantId"];

                currentEntry = SetOneLeague(summoner, objectLeague, currentEntry, participantID);
                objectLeague = objectLeague.Next;
            }
            return summoner;
        }

        private static JToken SetOneLeague(Summoner summoner, JToken objectLeague, JToken currentEntry, string participantID)
        {
            switch ((string)objectLeague.First.Next.Next)
            {
                case "RANKED_SOLO_5x5":
                    summoner.RankSolo.Tier = (string)objectLeague.First.Next;
                    while ((string)currentEntry.First.Next != summoner.Name)
                    {
                        currentEntry = currentEntry.Next;
                    }
                    summoner.RankSolo.Division = (string)currentEntry["division"];
                    summoner.RankSolo.LeaguePoints = (int)currentEntry["leaguePoints"];
                    break;
                case "RANKED_TEAM_3x3":
                    summoner.RankTeam3v3.Tier = (string)objectLeague.First.Next;
                    while ((string)currentEntry.First != participantID)
                    {
                        currentEntry = currentEntry.Next;
                    }
                    summoner.RankTeam3v3.Division = (string)currentEntry["division"];
                    summoner.RankTeam3v3.LeaguePoints = (int)currentEntry["leaguePoints"];
                    break;
                case "RANKED_TEAM_5x5":
                    summoner.RankTeam5v5.Tier = (string)objectLeague.First.Next;
                    while ((string)currentEntry.First != participantID)
                    {
                        currentEntry = currentEntry.Next;
                    }
                    summoner.RankTeam5v5.Division = (string)currentEntry["division"];
                    summoner.RankTeam5v5.LeaguePoints = (int)currentEntry["leaguePoints"];
                    break;
            }
            return currentEntry;
        }

        private async Task<JToken> ExecuteLeagueRequest(Summoner summoner, string region)
        {
            string request = CreateRequest(typeRequest.LEAGUE, region, new string[] { summoner.ID.ToString() });

            var output = await GetAPIResponse(request, typeRequest.LEAGUE);
            if(isRankedLeague)
            {
                var objectLeague = JObject.Parse(output)[summoner.ID.ToString()].First;
                return objectLeague;
            }
            else
            {
                return null;
            }
            
        }

        private Summoner SetSummonerInformations(JToken objectSummoner, Summoner summoner)
        {
            summoner.ID = (int)objectSummoner["id"];
            summoner.Name = (string)objectSummoner["name"];
            summoner.IdIcon = (int)objectSummoner["profileIconId"];
            summoner.Level = (int)objectSummoner["summonerLevel"];
            return summoner;
        }

        private async Task<JToken> ExecuteSummonerRequest(string name, string region)
        {
            string request = CreateRequest(typeRequest.SUMMONER_BY_NAME, region, new string[] { name });

            var output = await GetAPIResponse(request, typeRequest.SUMMONER_BY_NAME);
            name = name.Replace(" ", string.Empty);
            var objectSummoner = JObject.Parse(output)[name.ToLower()];

            return objectSummoner;
        }

        private string CreateRequest(typeRequest type, string region, string[] parameters)
        {
            string request = "https://" + region + ".api.pvp.net/api/lol/" + region + "/";

            char symbolBeforeApi = '?';
            if (parameters.Length > 1)
                symbolBeforeApi = '&';

            switch(type)
            {
                case typeRequest.SUMMONER_NAMES_BY_IDS:
                    request += "v1.4/summoner/" + parameters[0];
                    for(int i = 1; i < parameters.Length; i++)
                    {
                        request += "," + parameters[i];
                    }
                    request += "/name";
                    symbolBeforeApi = '?';
                    break;
                case typeRequest.SUMMONER_BY_NAME:
                    request += "v1.4/summoner/by-name/" + parameters[0];
                    break;
                case typeRequest.SUMMONER_BY_ID:
                    request += "v1.4/summoner/" + parameters[0];
                    break;
                case typeRequest.LEAGUE:
                    request += "v2.5/league/by-summoner/" + parameters[0];
                    break;
                case typeRequest.MATCH_HISTORY:
                    request += "v1.3/game/by-summoner/" + parameters[0] + "/recent";
                    break;
                case typeRequest.RANKED_STATS:
                    request += "v1.3/stats/by-summoner/" + parameters[0] + "/ranked?season=SEASON" + parameters[1];
                    break;
                case typeRequest.ONE_MATCH:
                    request += "v2.2/match/" + parameters[0] + "?IncludeTimeline=" + parameters[1];
                    break;
                case typeRequest.REALM:
                    request = "https://" + parameters[0] + ".api.pvp.net/api/lol/static-data/" + region + "/v1.2/realm";
                    break;
                case typeRequest.CHAMPIONS:
                    request = "https://" + parameters[0] + ".api.pvp.net/api/lol/static-data/" + region + "/v1.2/champion";
                    break;
                case typeRequest.SUMMONER_SPELLS:
                    request = "https://" + parameters[0] + ".api.pvp.net/api/lol/static-data/" + region + "/v1.2/summoner-spell";
                    break;
                default: return "ERROR_TYPE";
            }
            request += symbolBeforeApi + "api_key=" + apiKey;
            return request;
        }

        private async Task<string> GetAPIResponse(string request, typeRequest type)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(request);
            webRequest.Method = "GET";

            string content = null;
            HttpStatusCode code = HttpStatusCode.OK;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await webRequest.GetResponseAsync())
                {
                    Debug.WriteLine(request);
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        content = await sr.ReadToEndAsync();

                    code = response.StatusCode;
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        content = sr.ReadToEnd();

                    code = response.StatusCode;

                    if(type == typeRequest.SUMMONER_BY_NAME || code != HttpStatusCode.NotFound)
                        throw new RequestRiotAPIException(code);
                    else
                    {
                        switch(type)
                        {
                            case typeRequest.RANKED_STATS:
                                isRankedStats = false;
                                break;
                            case typeRequest.LEAGUE:
                                isRankedLeague = false;
                                break;
                        }
                    }
                }

            }
            catch (Exception)
            {
                
            }
            return content;
        }

        private async Task<string> GetNameSummonerById (int idSummoner, string region)
        {
            string request = CreateRequest(typeRequest.SUMMONER_BY_ID, region, new string[] { idSummoner.ToString() });

            var output = await GetAPIResponse(request, typeRequest.SUMMONER_BY_ID);
            var objectSummoner = JObject.Parse(output)[idSummoner];

            return (string)objectSummoner["name"];
        }

        public async Task<Match> GetNames(Match match, string region)
        {
            string[] parameters = new string[match.BlueTeam.Users.Count + match.RedTeam.Users.Count];
            int i = 0;
            foreach(Summoner s in match.BlueTeam.Users)
            {
                parameters[i] = s.ID.ToString();
                i++;
            }
            foreach (Summoner s in match.RedTeam.Users)
            {
                parameters[i] = s.ID.ToString();
                i++;
            }
            string request = CreateRequest(typeRequest.SUMMONER_NAMES_BY_IDS, region, parameters);
            var output = await GetAPIResponse(request, typeRequest.SUMMONER_NAMES_BY_IDS);
            var allNames = JObject.Parse(output);

            foreach(Summoner s in match.BlueTeam.Users)
            {
                s.Name = (string)allNames[s.ID.ToString()];
            }

            foreach (Summoner s in match.RedTeam.Users)
            {
                s.Name = (string)allNames[s.ID.ToString()];
            }

            return match;
        }

        public async Task<JToken> GetOneMatch(int id, string region, bool includeTimeline)
        {
            string request = CreateRequest(typeRequest.ONE_MATCH, region, new string[] { id.ToString(), includeTimeline.ToString() });
            var output = await GetAPIResponse(request, typeRequest.ONE_MATCH);
            var objectMatch = JObject.Parse(output);
            return objectMatch;
        }
    }
}
