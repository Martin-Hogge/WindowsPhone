/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ApplicationMobile_WP"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace ApplicationMobile_WP.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public NavigationService NavigationService { get; set; }
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var navigationService = this.CreateNavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);

            SimpleIoc.Default.Register<IDialogService, DialogService>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<HubViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<LoadingViewModel>();
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }

        private INavigationService CreateNavigationService()
        {
            NavigationService = new NavigationService();
            NavigationService.Configure("Hub", typeof(HubPage));
            NavigationService.Configure("DetailMatch", typeof(DetailMatchPage));
            NavigationService.Configure("Search", typeof(SearchPage));
            NavigationService.Configure("Loading", typeof(LoadingPage));
            NavigationService.Configure("Home", typeof(MainPage));
            NavigationService.Configure("Error", typeof(ErrorPage));

            return NavigationService;
        }
    }
}