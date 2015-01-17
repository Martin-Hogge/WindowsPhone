using ApplicationMobile_WP.DataAccess;
using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationMobile_WP.ViewModel
{
    public class SearchViewModel : ViewModelBase, INavigable
    {
        public RelayCommand SearchCommand { get; set; }
        public String SummonerName { get; set; }
        public RiotAPIServices Services { get; set; }
        public RelayCommand HubCommand { get; set; }
        public ObservableCollection<string> Regions { get; set; }
        public Summoner ChosenSummoner { get; set; }
        public String ErrorMessage 
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged();
            }
        }
        public String ChosenRegion
        {
            get { return chosenRegion; }
            set
            {
                chosenRegion = value;
                RaisePropertyChanged();
            }
        }
        public Boolean IsFavorite { get; set; }
        private string errorMessage;
        private string chosenRegion;
        public RelayCommand GoHomeCommand { get; set; }
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

        public SearchViewModel()
        {
            InitializeRegions();
            IsFavorite = true;

            ChosenSummoner = new Summoner();
            Services = new RiotAPIServices();

            GoHomeCommand = new RelayCommand(() =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Home");
            });

            HubCommand = new RelayCommand(() =>
            {
                ChosenSummoner.Region = ChosenRegion.ToLower();
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Hub", ChosenSummoner);
            });

            SearchCommand = new RelayCommand(async() =>
            {
                try
                {
                    ProgressActive = true;
                    await PerformRequests();
                    HubViewModel.ComeFromSearchPage = true;
                    HubCommand.Execute(null);
                }
                catch(RequestRiotAPIException e)
                {
                    ErrorMessage = e.Message +
                                   (e.Code == System.Net.HttpStatusCode.NotFound ? String.Format(" ({0})", SummonerName) : "");
                }
                finally
                {
                    ProgressActive = false;
                }
            });
        }

        private async Task PerformRequests()
        {

            ChosenSummoner = await Services.GetSummoner(SummonerName, ChosenRegion.ToLower());
            ChosenSummoner.IsFavorite = LocalDataAccessManager.AlreadyExistsInList(
                    await LocalDataAccessManager.GetListSummonersFromLocal(LocalDataAccessManager.favoriteFileName), ChosenSummoner);
            await LocalDataAccessManager.AddRecentResearch(new Summoner(
                    ChosenSummoner.ID, ChosenSummoner.IdIcon, ChosenSummoner.Name, ChosenSummoner.Region));
        }

        private void InitializeRegions()
        {
            Regions = new ObservableCollection<string>();
            Regions.Add("EUW");
            Regions.Add("NA");
            Regions.Add("EUNE");
            Regions.Add("BR");
            Regions.Add("RU");
            Regions.Add("TR");
            Regions.Add("LAN");
            Regions.Add("LAS");
            Regions.Add("OCE");

            ChosenRegion = Regions[0];
        }
        public void Activate(object parameter)
        {
            
        }

        public void Desactivate(object parameter)
        {
            
        }


    }
}
