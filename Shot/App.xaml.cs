using Ninject;
using Shot.DependencyInjection;
using Shot.Navigation;
using Shot.Pages;
using Shot.ViewModels;
using Xamarin.Forms;

namespace Shot
{
    public partial class App : Application
    {
        public StandardKernel Kernel { get; set; }

        public App()
        {
            InitializeComponent();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            Color barColor = currentTheme == OSAppTheme.Dark ? Color.FromHex("#191919") : Color.WhiteSmoke;

            var recordingPage = new NavigationPage(new RecordingsListPage()) { BarBackgroundColor = barColor };
            var provider = new ProviderRegisterationController(recordingPage.Navigation);
            Kernel = new StandardKernel(provider);

            var appNavigationService = Kernel.Get<INavigationService>();
            // view and view model mappings
            provider.Map(appNavigationService);
            recordingPage.BindingContext = Kernel.Get<RecordingsListViewModel>();
            MainPage = recordingPage;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
