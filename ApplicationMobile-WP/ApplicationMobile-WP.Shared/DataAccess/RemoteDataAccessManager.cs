using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.Model;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationMobile_WP.DataAccess
{
    public class RemoteDataAccessManager
    {
        public static Dictionary<KeyValuePair<int, string>, bool> MatchUpdated = new Dictionary<KeyValuePair<int, string>, bool>();

        public async static void AddListOfMatch(List<KeyValuePair<int, string>> listToAdd,
            List<KeyValuePair<int, KeyValuePair<int, int>>> MatchingSummonerChampion, KeyValuePair<int, string> SummonerInfo)
        {
            var tableMatch = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.Match>();
            var tableTeam = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.Team>();
            var tableSummonerTeam = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.SummonerTeam>();

            List<JToken> listTokenMatch = new List<JToken>();
            RiotAPIServices services = new RiotAPIServices();

            await SetListTokenMatch(listToAdd, tableMatch, listTokenMatch, services);

            foreach (JToken tokenMatch in listTokenMatch)
            {
                DataAccess.RemoteModel.Match match = new DataAccess.RemoteModel.Match();
                DataAccess.RemoteModel.Team blueTeam = new DataAccess.RemoteModel.Team("blue-" + (string)tokenMatch["matchId"], (string)tokenMatch["matchId"]);
                DataAccess.RemoteModel.Team redTeam = new DataAccess.RemoteModel.Team("red-" + (string)tokenMatch["matchId"], (string)tokenMatch["matchId"]);

                SetMatchInformation(listToAdd, tokenMatch, match);
                SetTeamsInformation(tokenMatch, blueTeam, redTeam);

                try
                {
                    await InsertMatchAndTeams(tableMatch, tableTeam, match, blueTeam, redTeam);
                    await InsertAllSummonerTeam(MatchingSummonerChampion, tableSummonerTeam, tokenMatch, match);
                    Debug.WriteLine(String.Format("Match({0}) insert in Azure Database", match.GameID));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("A match has not been insert due to an exception : See next line");
                    Debug.WriteLine(e.Message);
                }
            }
            MatchUpdated[SummonerInfo] = true;
        }

        private static async Task SetListTokenMatch(List<KeyValuePair<int, string>> listToAdd, IMobileServiceTable<RemoteModel.Match> tableMatch,
            List<JToken> listTokenMatch, RiotAPIServices services)
        {
            int i = 0;
            while (i < listToAdd.Count)
            {
                Debug.WriteLine(i);
                if ((await tableMatch.Where(x => x.id == listToAdd[i].Key.ToString()).ToListAsync()).Count == 0)
                {
                    try
                    {
                        listTokenMatch.Add(await services.GetOneMatch(listToAdd[i].Key, listToAdd[i].Value, false));
                    }
                    catch (RequestRiotAPIException)
                    {
                        Debug.WriteLine(i);
                    }
                }
                else
                    i++;
            }
        }

        //com
        private static async System.Threading.Tasks.Task InsertAllSummonerTeam(List<KeyValuePair<int, KeyValuePair<int, int>>> MatchingSummonerChampion,
            IMobileServiceTable<RemoteModel.SummonerTeam> tableSummonerTeam, JToken tokenMatch, DataAccess.RemoteModel.Match match)
        {
            var players = tokenMatch["participants"];
            var currentPlayer = players.First;

            while (currentPlayer != null)
            {
                DataAccess.RemoteModel.SummonerTeam summonerTeam = new DataAccess.RemoteModel.SummonerTeam();
                SetSummonerTeamInformation(MatchingSummonerChampion, tokenMatch, match, currentPlayer, summonerTeam);
                await tableSummonerTeam.InsertAsync(summonerTeam);
                currentPlayer = currentPlayer.Next;
            }
        }

        private static void SetSummonerTeamInformation(List<KeyValuePair<int, KeyValuePair<int, int>>> MatchingSummonerChampion,
            JToken tokenMatch, DataAccess.RemoteModel.Match match, JToken currentPlayer, DataAccess.RemoteModel.SummonerTeam summonerTeam)
        {
            summonerTeam.assists = currentPlayer["stats"].Value<int?>("assists") ?? 0;
            summonerTeam.championid = (int)currentPlayer["championId"];
            summonerTeam.creeps = currentPlayer["stats"].Value<int?>("minionsKilled") ?? 0;
            summonerTeam.damagedealt = currentPlayer["stats"].Value<int?>("totalDamageDealtToChampions") ?? 0;
            summonerTeam.damagetaken = currentPlayer["stats"].Value<int?>("totalDamageTaken") ?? 0;
            summonerTeam.deaths = currentPlayer["stats"].Value<int?>("deaths") ?? 0;
            summonerTeam.goldearned = (int)currentPlayer["stats"]["goldEarned"];
            summonerTeam.healing = currentPlayer["stats"].Value<int?>("totalHeal") ?? 0;
            summonerTeam.item1 = currentPlayer["stats"].Value<int?>("item0") ?? 0;
            summonerTeam.item2 = currentPlayer["stats"].Value<int?>("item1") ?? 0;
            summonerTeam.item3 = currentPlayer["stats"].Value<int?>("item2") ?? 0;
            summonerTeam.item4 = currentPlayer["stats"].Value<int?>("item3") ?? 0;
            summonerTeam.item5 = currentPlayer["stats"].Value<int?>("item4") ?? 0;
            summonerTeam.item6 = currentPlayer["stats"].Value<int?>("item5") ?? 0;
            summonerTeam.item7 = currentPlayer["stats"].Value<int?>("item6") ?? 0;
            summonerTeam.kills = currentPlayer["stats"].Value<int?>("kills") ?? 0;
            summonerTeam.largestkillingspree = currentPlayer["stats"].Value<int?>("largestKillingSpree") ?? 0;
            summonerTeam.region = match.Region;
            summonerTeam.spell1 = (int)currentPlayer["spell1Id"];
            summonerTeam.spell2 = (int)currentPlayer["spell2Id"];
            foreach (KeyValuePair<int, KeyValuePair<int, int>> entry in MatchingSummonerChampion)
            {
                if (entry.Key == match.GameID && entry.Value.Value == summonerTeam.championid)
                {
                    summonerTeam.summonerid = entry.Value.Key;
                    MatchingSummonerChampion.Remove(entry);
                    break;
                }
            }
            summonerTeam.teamid = ((int)currentPlayer["teamId"] == 100 ? "blue-" : "red-");
            summonerTeam.teamid += (string)tokenMatch["matchId"];
            summonerTeam.timecrowdcontrol = currentPlayer["stats"].Value<int?>("totalTimeCrowdControlDealt") ?? 0;
        }

        private static async System.Threading.Tasks.Task InsertMatchAndTeams(IMobileServiceTable<RemoteModel.Match> tableMatch, Microsoft.WindowsAzure.MobileServices.IMobileServiceTable<RemoteModel.Team> tableTeam, DataAccess.RemoteModel.Match match, DataAccess.RemoteModel.Team blueTeam, DataAccess.RemoteModel.Team redTeam)
        {
            try
            {
                await tableMatch.InsertAsync(match);
                await tableTeam.InsertAsync(blueTeam);
                await tableTeam.InsertAsync(redTeam);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private static void SetTeamsInformation(JToken tokenMatch, DataAccess.RemoteModel.Team blueTeam, DataAccess.RemoteModel.Team redTeam)
        {
            if ((int)tokenMatch["teams"].First["teamId"] == 100)
            {
                blueTeam.win = (bool)tokenMatch["teams"].First["winner"];
                redTeam.win = !blueTeam.win;
            }
            else
            {
                redTeam.win = (bool)tokenMatch["teams"].First["winner"];
                blueTeam.win = !redTeam.win;
            }
        }

        private static void SetMatchInformation(List<KeyValuePair<int, string>> listToAdd, JToken tokenMatch, DataAccess.RemoteModel.Match match)
        {
            match.GameID = (int)tokenMatch["matchId"];
            match.GameLength = (int)tokenMatch["matchDuration"] / 60;
            match.GameMode = (string)tokenMatch["matchMode"];
            match.GameType = (string)tokenMatch["matchType"];
            match.SubType = (string)tokenMatch["queueType"];
            match.id = match.GameID.ToString();
            match.Region = listToAdd[0].Value;
        }

        public static async Task<int> GetScoreForSummoner(int summonerId, string region)
        {
            var tableSummonerTeam = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.SummonerTeam>();
            var tableTeam = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.Team>();
            int score = 0;

            List<DataAccess.RemoteModel.SummonerTeam> listSummonerTeam = await tableSummonerTeam.Where(
                x => x.summonerid == summonerId && x.region == region).ToListAsync();

            foreach (DataAccess.RemoteModel.SummonerTeam summonerTeam in listSummonerTeam)
            {
                score = await CalculScore(tableTeam, score, summonerTeam);
            }
            if (listSummonerTeam.Count > 0)
                return score / listSummonerTeam.Count;
            else
                return 0;
        }

        private static async Task<int> CalculScore(Microsoft.WindowsAzure.MobileServices.IMobileServiceTable<RemoteModel.Team> tableTeam, int score, DataAccess.RemoteModel.SummonerTeam summonerTeam)
        {
            /*List<DataAccess.RemoteModel.Team> teams = await tableTeam.Where(x => x.id == summonerTeam.teamid).ToListAsync();
            if (teams[0].win)
                score += 1000;
            else
                score += 500;*/
            score += summonerTeam.kills * 100;
            score -= summonerTeam.deaths * 100;
            score += summonerTeam.assists * 50;
            score += summonerTeam.creeps * 2;
            score += summonerTeam.damagedealt / 100;
            score += summonerTeam.goldearned / 10;
            score += summonerTeam.healing / 10;
            score += summonerTeam.largestkillingspree * 100;
            score += summonerTeam.timecrowdcontrol * 2;
            return score;
        }

        public static async Task<List<Model.Match>> GetMoreMatchs(List<RemoteModel.Match> remoteList, List<Model.Match> currentList, int summonerId, string region)
        {
            int matchAdded = 0;
            foreach(RemoteModel.Match remoteMatch in remoteList)
            {
                 bool alreadyExists = false;
                 foreach(Model.Match currentMatch in currentList)
                 {
                     if (remoteMatch.GameID == currentMatch.GameID)
                     {
                         alreadyExists = true;
                         break;
                     }
                 }
                 if (!alreadyExists)
                 {
                     currentList.Add(await ConvertRemoteModelMatch(remoteMatch, summonerId, region));
                     matchAdded++;
                 }
                 if (matchAdded >= 10)
                     break;
            }
            return currentList;
        }

        public async static Task<Model.Match> ConvertRemoteModelMatch (DataAccess.RemoteModel.Match remoteMatch, int summonerId, string region)
        {
            var tableTeam = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.Team>();
            var tableSummonerTeam = App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.SummonerTeam>();

            Model.Match modelMatch = new Model.Match();
            bool blueTeamWin, redTeamWin;

            modelMatch.GameID = remoteMatch.GameID;
            modelMatch.GameLength = remoteMatch.GameLength;
            modelMatch.GameMode = remoteMatch.GameMode;
            modelMatch.GameType = remoteMatch.GameType;
            modelMatch.SubType = remoteMatch.SubType;

            List<DataAccess.RemoteModel.Team> teams = await (tableTeam.Where(x => x.matchid == remoteMatch.id)).ToListAsync();
            List<DataAccess.RemoteModel.SummonerTeam> blueTeamSummoners = new List<RemoteModel.SummonerTeam>();
            List<DataAccess.RemoteModel.SummonerTeam> redTeamSummoners = new List<RemoteModel.SummonerTeam>();
            if(teams[0].id.Contains("blue"))
            {
                blueTeamSummoners = await (tableSummonerTeam.Where(x => x.teamid == teams[0].id)).ToListAsync();
                redTeamSummoners = await (tableSummonerTeam.Where(x => x.teamid == teams[1].id)).ToListAsync();
                blueTeamWin = teams[0].win;
                redTeamWin = teams[1].win;
            }
            else
            {
                blueTeamSummoners = await (tableSummonerTeam.Where(x => x.teamid == teams[1].id)).ToListAsync();
                redTeamSummoners = await (tableSummonerTeam.Where(x => x.teamid == teams[0].id)).ToListAsync();
                blueTeamWin = teams[1].win;
                redTeamWin = teams[0].win;
            }

            foreach(DataAccess.RemoteModel.SummonerTeam summoner in blueTeamSummoners)
            {
                modelMatch.BlueTeam.Users.Add(new Summoner(summoner.summonerid, summoner.championid));
                if(summoner.summonerid == summonerId)
                {
                    modelMatch.CreepsKilled = summoner.creeps;
                    modelMatch.ChampionId = summoner.championid;
                    modelMatch.GoldEarned = summoner.goldearned;
                    modelMatch.KDA = new KDA(summoner.kills, summoner.deaths, summoner.assists);
                    modelMatch.SetResult(blueTeamWin);
                    modelMatch.Items = new int?[]{summoner.item1, summoner.item2, summoner.item3,
                        summoner.item4, summoner.item5, summoner.item6, summoner.item7};
                    modelMatch.SummonerSpells = new int[] { summoner.spell1, summoner.spell2 };
                }
            }

            foreach (DataAccess.RemoteModel.SummonerTeam summoner in redTeamSummoners)
            {
                modelMatch.RedTeam.Users.Add(new Summoner(summoner.summonerid, summoner.championid));
                if (summoner.summonerid == summonerId)
                {
                    modelMatch.CreepsKilled = summoner.creeps;
                    modelMatch.ChampionId = summoner.championid;
                    modelMatch.GoldEarned = summoner.goldearned;
                    modelMatch.KDA = new KDA(summoner.kills, summoner.deaths, summoner.assists);
                    modelMatch.SetResult(redTeamWin);
                    modelMatch.Items = new int?[]{summoner.item1, summoner.item2, summoner.item3,
                        summoner.item4, summoner.item5, summoner.item6, summoner.item7};
                    modelMatch.SummonerSpells = new int[] { summoner.spell1, summoner.spell2 };
                }
            }

            bool requestOk = false;
            while(!requestOk)
            {
                try
                {
                    modelMatch = await new RiotAPIServices().GetNames(modelMatch, region);
                    requestOk = true;
                }
                catch(RequestRiotAPIException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return modelMatch;
        }

        public static bool AlreadyExistsInMatchUpdated(int id, string region)
        {
            foreach(KeyValuePair<KeyValuePair<int,string>,bool> entry in MatchUpdated)
            {
                if (entry.Key.Key == id && entry.Key.Value == region)
                    return true;
            }
            return false;
        }
    }
}
