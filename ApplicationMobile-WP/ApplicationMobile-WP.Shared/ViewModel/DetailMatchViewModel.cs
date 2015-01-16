using ApplicationMobile_WP.DataAccess;
using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace ApplicationMobile_WP.ViewModel
{
    public class DetailMatchViewModel : ViewModelBase, INavigable
    {
        public Match Match { get; set; }
        public Team BlueTeam { get; set; }
        public Team RedTeam { get; set; }
        public ObservableCollection<Summoner> Users { get; set; }
        public RelayCommand<Summoner> HubCommand { get; set; }
        public RiotAPIServices Services { get; set; }
        public RelayCommand GoHomeCommand { get; set; }
        public RelayCommand SearchCommand { get; set; }

        public DetailMatchViewModel ()
        {
            Services = new RiotAPIServices();
            HubCommand = new RelayCommand<Summoner>(async summoner =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Loading");
                summoner = await PerformRequests(summoner);
                
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Hub", summoner);
            });

            GoHomeCommand = new RelayCommand(() =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Home");
            });

            SearchCommand = new RelayCommand(() =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Search");
            });
        }

        private async Task<Summoner> PerformRequests(Summoner summoner)
        {
            bool requestOk = false;
            int i409Error = 0;
            while (!requestOk)
            {
                try
                {
                    summoner = await Services.GetSummoner(summoner.Name, summoner.Region);
                    summoner.IsFavorite = LocalDataAccessManager.AlreadyExistsInList(
                        await LocalDataAccessManager.GetListSummonersFromLocal(LocalDataAccessManager.favoriteFileName), summoner);
                    await LocalDataAccessManager.AddRecentResearch(new Summoner(summoner.ID, summoner.IdIcon, summoner.Name, summoner.Region));
                    requestOk = true;
                }
                catch (RequestRiotAPIException e)
                {
                    if (!(e.Code == (System.Net.HttpStatusCode)409) || i409Error >= int.MaxValue)
                    {
                        GoToErrorPage(summoner);
                        break;
                    }
                    else
                        i409Error++;
                }
            }
            return summoner;
        }

        private static void GoToErrorPage(Summoner summoner)
        {
            ErrorViewModel.TypeOfError = ErrorViewModel.ErrorType.GO_TO_SUMMONER;
            SingletonViewLocator.getInstance().NavigationService.NavigateTo("Error",
                new Object[] { summoner, new RequestRiotAPIException(System.Net.HttpStatusCode.Ambiguous) });
        }

        public void Activate(object parameter)
        {
            Match = parameter as Match;
        }

        public void Desactivate(object parameter)
        {
            
        }
    }
}
