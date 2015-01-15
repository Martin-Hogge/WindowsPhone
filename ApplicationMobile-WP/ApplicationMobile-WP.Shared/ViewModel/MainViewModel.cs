using ApplicationMobile_WP.BackgroundConfiguration;
using ApplicationMobile_WP.DataAccess;
using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;
using WinRTXamlToolkit.Controls;

namespace ApplicationMobile_WP.ViewModel
{
    public class MainViewModel : ViewModelBase, INavigable
    {
        public static ObservableCollection<Summoner> Favoris { get; set; }
        public static ObservableCollection<Summoner> RecentSearches { get; set; }
        public RelayCommand<Summoner> HubCommand { get; set; }
        public RelayCommand SearchCommand { get; set; }
        private RiotAPIServices Services { get; set; }

        public MainViewModel()
        {
            Services = new RiotAPIServices();
            HubCommand = new RelayCommand<Summoner>(async summoner =>
            {
                LoadingViewModel.ComeFromBackButton = false;
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Loading");
                summoner = await PerformRequests(summoner);
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Hub", summoner);
            });

            SearchCommand = new RelayCommand(() =>
            {
                SingletonViewLocator.getInstance().NavigationService.NavigateTo("Search");
            });
        }

        private async Task<Summoner> PerformRequests(Summoner summoner)
        {
            bool requestOk = false;
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
                    Debug.WriteLine(e.Message);
                }
            }
            return summoner;
        }

        public void Activate(object parameter)
        {

        }

        public void Desactivate(object parameter)
        {

        }

    }
}