using Ninject.Modules;
using Shot.Navigation;
using Shot.Pages;
using Shot.Services;
using Shot.ViewModels;
using Xamarin.Essentials.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Essentials.Implementation;

namespace Shot.DependencyInjection
{
    public class ProviderRegisterationController : NinjectModule
    {
        private readonly INavigation _navigation;

        public ProviderRegisterationController(INavigation nav)
        {
            _navigation = nav;
        }


        public override void Load()
        {
            //Navigation Service
            Bind<INavigation>().ToMethod(x => _navigation).InSingletonScope();
            Bind<INavigationService>().To<NavigationService>().InSingletonScope();

            // ViewModels
            Bind<RecordingViewModel>().ToSelf();
            Bind<RecordingsListViewModel>().ToSelf();
            Bind<PlayerViewModel>().ToSelf();
            Bind<SettingsViewModel>().ToSelf();

            //Services
            Bind<IPermissionsService>().To<PermissionsService>().InSingletonScope();
            Bind<IPermissions>().To<PermissionsImplementation>().InSingletonScope();
            Bind<IDevicePlatform>().To<Services.DevicePlatform>().InSingletonScope();
        }

        public void Map(INavigationService navigationService)
        {
            // Register view mappings
            navigationService.RegisterMapping(
                typeof(RecordingViewModel),
                typeof(RecordingPage));

            navigationService.RegisterMapping(
                typeof(RecordingsListViewModel),
                typeof(RecordingsListPage));

            navigationService.RegisterMapping(
                typeof(PlayerViewModel),
                typeof(PlayerPage));

            navigationService.RegisterMapping(
                typeof(SettingsViewModel),
                typeof(SettingsPage));
        }
    }
}
