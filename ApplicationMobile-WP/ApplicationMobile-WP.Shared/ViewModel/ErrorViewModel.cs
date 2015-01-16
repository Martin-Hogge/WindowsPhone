using ApplicationMobile_WP.Common;
using ApplicationMobile_WP.DataAccess;
using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationMobile_WP.ViewModel
{
    class ErrorViewModel : ViewModelBase, INavigable
    {
        public enum ErrorType { GO_TO_SUMMONER, GO_TO_MATCH };

        public static ErrorType TypeOfError { get; set; }
        private string _content;
        public String Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }
        public RelayCommand ErrorCommand { get; set; }
        private Object[] listParameters { get; set; }
        private Summoner summoner { get; set; }
        private Match match { get; set; }
        private String region { get; set; }
        private bool RequestOk { get; set; }
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

        public ErrorViewModel()
        {
            ProgressActive = false;
            switch (TypeOfError)
            {
                case ErrorType.GO_TO_SUMMONER:
                    ErrorCommand = new RelayCommand(async () =>
                    {
                       ProgressActive = true;
                       summoner = await PerformRequestsGoToSummoner(summoner, listParameters);
                       if(RequestOk)
                            SingletonViewLocator.getInstance().NavigationService.NavigateTo("Hub", summoner);

                    });
                    break;
                case ErrorType.GO_TO_MATCH:
                    ErrorCommand = new RelayCommand(async () =>
                    {
                        ProgressActive = true;
                        match = await GoToDetailMatch(match);
                        if (RequestOk)
                            SingletonViewLocator.getInstance().NavigationService.NavigateTo("DetailMatch", match);
                    });
                    break;
                default:
                    break;
            }
        }

        private async Task<Match> GoToDetailMatch(Match match)
        {
            RiotAPIServices Services = new RiotAPIServices();
            RequestOk = false;
            int i409Error = 0;
            while (!RequestOk)
            {
                try
                {
                    match = await Services.GetNames(match, region);
                    RequestOk = true;
                }
                catch (RequestRiotAPIException e)
                {
                    if (!(e.Code == (System.Net.HttpStatusCode)409) || i409Error >= int.MaxValue)
                    {
                        Content = e.Message;
                        ProgressActive = false;
                        break;
                    }
                    else
                        i409Error++;
                }
            }
            return match;
        }

        public void Activate(object parameter)
        {
            listParameters = parameter as Object[];
            switch(TypeOfError)
            {
                case ErrorType.GO_TO_SUMMONER:
                    Content = (listParameters[1] as RequestRiotAPIException).Message;
                    summoner = listParameters[0] as Summoner;
                    break;
                case ErrorType.GO_TO_MATCH:
                    Content = (listParameters[2] as RequestRiotAPIException).Message;
                    match = listParameters[0] as Match;
                    region = listParameters[1] as String;
                    break;
            }
            
        }

        private async Task<Summoner> PerformRequestsGoToSummoner(Summoner summoner, Object[] listParameters)
        {
            RiotAPIServices Services = new RiotAPIServices();
            RequestOk = false;
            int i409Error = 0;
            while (!RequestOk)
            {
                try
                {
                    summoner = await Services.GetSummoner(summoner.Name, summoner.Region);
                    summoner.IsFavorite = LocalDataAccessManager.AlreadyExistsInList(
                        await LocalDataAccessManager.GetListSummonersFromLocal(LocalDataAccessManager.favoriteFileName), summoner);
                    await LocalDataAccessManager.AddRecentResearch(new Summoner(summoner.ID, summoner.IdIcon, summoner.Name, summoner.Region));
                    RequestOk = true;
                }
                catch (RequestRiotAPIException e)
                {
                    if (!(e.Code == (System.Net.HttpStatusCode)409) || i409Error >= int.MaxValue)
                    {
                        Content = e.Message;
                        ProgressActive = false;
                        break;
                    }
                    else
                        i409Error++;
                }
            }
            return summoner;
        }

        public void Desactivate(object parameter)
        {

        }
    }
}
