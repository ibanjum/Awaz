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

            var recordingPage = new NavigationPage(new RecordingPage());
            var provider = new ProviderRegisterationController(recordingPage.Navigation);
            Kernel = new StandardKernel(provider);

            var appNavigationService = Kernel.Get<INavigationService>();

            // view and view model mappings
            provider.Map(appNavigationService);
            recordingPage.BindingContext = Kernel.Get<RecordingViewModel>();
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
