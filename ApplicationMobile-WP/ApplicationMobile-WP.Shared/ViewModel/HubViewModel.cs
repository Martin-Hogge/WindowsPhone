using ApplicationMobile_WP.DataAccess;
using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace ApplicationMobile_WP.ViewModel
{
    public class HubViewModel : ViewModelBase, INavigable
    {
        public Summoner User { get; set; }
        public RelayCommand<Match> DetailMatchCommand { get; set; }
        public RelayCommand UpdateScoreCommand { get; set; }
        public RelayCommand GetMoreMatchCommand { get; set; }
        public RelayCommand FavoriteCommand { get; set; }
        public RiotAPIServices Services { get; set; }
        public RelayCommand GoHomeCommand { get; set; }
        public RelayCommand SearchCommand { get; set; }
        private ObservableCollection<Match> matchHistory;
        private string nbMatchesAdded;
        private List<DataAccess.RemoteModel.Match> remoteMatchs { get; set; }
        private string matchUpdateText;
        private Match selectedMatch;
        private string alertMessageFavorite;
        private string colorAlertMessage;
        private string textButtonFavorite;
        private int score;
        public ObservableCollection<Match> MatchHistory
        {
            get { return matchHistory; }
            set
            {
                matchHistory = value;
                RaisePropertyChanged();
            }
        }
        public String NbMatchesAdded
        {
            get { return nbMatchesAdded; }
            set
            {
                nbMatchesAdded = value;
                RaisePropertyChanged();
            }
        }
        public String MatchUpdateText
        {
            get { return matchUpdateText; }
            set
            {
                matchUpdateText = value;
                RaisePropertyChanged();
            }
        }
        public Match SelectedMatch
        {
            get { return selectedMatch; }
            set
            {
                selectedMatch = value;
                RaisePropertyChanged();
            }
        }
        public String AlertMessageFavorite
        {
            get { return alertMessageFavorite; }
            set
            {
                alertMessageFavorite = value;
                RaisePropertyChanged();
            }
        }
        public String ColorAlertMessage
        {
            get { return colorAlertMessage; }
            set
            {
                colorAlertMessage = value;
                RaisePropertyChanged();
            }
        }
        public String TextButtonFavorite
        {
            get { return textButtonFavorite; }
            set
            {
                textButtonFavorite = value;
                RaisePropertyChanged();
            }
        }
        public int Score
        {
            get { return score; }
            set
            {
                score = value;
                RaisePropertyChanged();
            }
        }
       
        private string updateScoreColor;
        public String UpdateScoreColor
        {
            get { return updateScoreColor; }
            set
            {
                updateScoreColor = value;
                RaisePropertyChanged();
            }
        }
        private bool _progressActive;
        public bool ProgressActive
        {
            get { return _progressActive; }
            set
            {
                _progressActive = value;
                RaisePropertyChanged();
            }
        }
        public static bool ComeFromSearchPage { get; set; }
        public HubViewModel()
        {
            Services = new RiotAPIServices();
            GoHomeCommand = new RelayCommand(() =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Home");
            });

            SearchCommand = new RelayCommand(() =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Search");
            });

            DetailMatchCommand = new RelayCommand<Match>(async match =>
            {
                match = await GoToDetailMatch(match);
            });

            UpdateScoreCommand = new RelayCommand(() =>
            {
                UpdateScore();
            });

            GetMoreMatchCommand = new RelayCommand(async() =>
            {
                ProgressActive = true;
                await UpdateMatchHistory();
                ProgressActive = false;
            });

            FavoriteCommand = new RelayCommand(async () =>
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                if (User.IsFavorite)
                    await RemoveFromFavorites(loader);
                else
                    await AddToFavorites(loader);

                SetTextButtonFavorite(loader);
            });
        }

        private async Task AddToFavorites(ResourceLoader loader)
        {
            try
            {
                await LocalDataAccessManager.AddFavorite(new Summoner(User.ID, User.IdIcon, User.Name, User.Region));
                ColorAlertMessage = "Green";
                AlertMessageFavorite = loader.GetString("AddFavoriteSuccess");
                User.IsFavorite = true;
            }
            catch (LocalStorageException e)
            {
                ColorAlertMessage = "Red";
                AlertMessageFavorite = e.Message;
            }
        }

        private async Task RemoveFromFavorites(ResourceLoader loader)
        {
            await LocalDataAccessManager.RemoveFromFavorites(new Summoner(User.ID, User.IdIcon, User.Name, User.Region));
            ColorAlertMessage = "Green";
            AlertMessageFavorite = loader.GetString("RemoveFavoriteSuccess");
            User.IsFavorite = false;
        }

        private async Task UpdateMatchHistory()
        {
            var loader = new ResourceLoader();
            if (remoteMatchs == null)
            {
                await InitializeRemoteMatchs();
            }
            int previousNbMatchs = MatchHistory.Count;
            MatchHistory = new ObservableCollection<Match>(
                await RemoteDataAccessManager.GetMoreMatchs(remoteMatchs, new List<Match>(MatchHistory), User.ID, User.Region));
            int currentNbMatchs = MatchHistory.Count;
            NbMatchesAdded = (currentNbMatchs - previousNbMatchs) + " " + loader.GetString("MatchAdded");
        }

        private async void UpdateScore()
        {
            var loader = new ResourceLoader();
            Score = await RemoteDataAccessManager.GetScoreForSummoner(User.ID, User.Region);
            SetMatchUpdateText(loader);
        }

        private async Task<Match> GoToDetailMatch(Match match)
        {
            bool requestOk = false;
            int i409Error = 0;
            while (!requestOk)
            {
                try
                {
                    match = await Services.GetNames(match, User.Region);
                    requestOk = true;
                }
                catch (RequestRiotAPIException e)
                {
                    if (!(e.Code == (System.Net.HttpStatusCode)409) || i409Error >= int.MaxValue)
                    {
                        GoToErrorPage(match);
                        break;
                    }
                    else
                        i409Error++;
                }
            }
            if(requestOk)
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("DetailMatch", match);
            return match;
        }

        private void GoToErrorPage(Match match)
        {
            ErrorViewModel.TypeOfError = ErrorViewModel.ErrorType.GO_TO_MATCH;
            SingletonViewLocator.getInstance().NavigationService.NavigateTo("Error",
                new Object[] { match, User.Region, new RequestRiotAPIException(System.Net.HttpStatusCode.Ambiguous) });
        }

        private async Task InitializeRemoteMatchs()
        {
            remoteMatchs = new List<DataAccess.RemoteModel.Match>();
            List<DataAccess.RemoteModel.SummonerTeam> summoners = await (App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.SummonerTeam>().Where(x => x.summonerid == User.ID)).ToListAsync();
            List<DataAccess.RemoteModel.Team> teams = new List<DataAccess.RemoteModel.Team>();

            await GetTeams(summoners, teams);

            foreach (DataAccess.RemoteModel.Team t in teams)
            {
                List<DataAccess.RemoteModel.Match> matchs = await (App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.Match>().Where(x => x.id == t.matchid)).ToListAsync();
                remoteMatchs.Add(matchs[0]);
            }
        }

        private static async Task GetTeams(List<DataAccess.RemoteModel.SummonerTeam> summoners, List<DataAccess.RemoteModel.Team> teams)
        {
            foreach (DataAccess.RemoteModel.SummonerTeam st in summoners)
            {
                List<DataAccess.RemoteModel.Team> tempListTeam = await (App.lolServiceMobileClient.GetTable<DataAccess.RemoteModel.Team>().Where(x => x.id == st.teamid)).ToListAsync();
                foreach (DataAccess.RemoteModel.Team t in tempListTeam)
                {
                    teams.Add(t);
                }
            }
        }

        public void Activate(object parameter)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            User = new Summoner();
            User = parameter as Summoner;
            SetTextButtonFavorite(loader);
            Score = User.Score;
            MatchHistory = User.MatchHistory;

            SetMatchUpdateText(loader);
        }

        private void SetMatchUpdateText(Windows.ApplicationModel.Resources.ResourceLoader loader)
        {
            if (RemoteDataAccessManager.MatchUpdated[new KeyValuePair<int, string>(User.ID, User.Region)])
            {
                MatchUpdateText = loader.GetString("MatchUpToDate");
                UpdateScoreColor = "Green";
            }
            else
            {
                MatchUpdateText = loader.GetString("MatchNotUpToDate");
                UpdateScoreColor = "Orange";
            }
        }

        private void SetTextButtonFavorite(ResourceLoader loader)
        {
            if (User.IsFavorite)
                TextButtonFavorite = loader.GetString("RemoveFavorite");
            else
                TextButtonFavorite = loader.GetString("AddFavorite");
        }

        public void Desactivate(object parameter)
        {

        }
    }
}
