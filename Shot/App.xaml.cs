using System;
using System.Diagnostics;
using System.Windows.Input;
using Ninject;
using Shot.DependencyInjection;
using Shot.Navigation;
using Shot.Pages;
using Shot.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Shot
{
    public partial class App : Application
    {
        public StandardKernel Kernel { get; set; }

        private ICommand SetThemeCommand => new Command(SetTheme);
        private const string CURRENT_THEME_KEY = "CurrentTheme";

        public App()
        {
            InitializeComponent();

            SetThemeCommand.Execute(null);

            var recordingPage = new NavigationPage(new RecordingsListPage());
            var provider = new ProviderRegisterationController(recordingPage.Navigation);
            Kernel = new StandardKernel(provider);

            var appNavigationService = Kernel.Get<INavigationService>();

            // view and view model mappings
            provider.Map(appNavigationService);
            recordingPage.BindingContext = Kernel.Get<RecordingsListViewModel>();
            MainPage = recordingPage;
        }

        private async void SetTheme()
        {
            try
            {
                var currentThemeString = await SecureStorage.GetAsync(CURRENT_THEME_KEY);
                if (!string.IsNullOrEmpty(currentThemeString))
                {
                    var currentTheme = (OSAppTheme)Enum.Parse(typeof(OSAppTheme), currentThemeString);
                    Current.UserAppTheme = currentTheme;
                }
            }
            catch
            {
                Debugger.Break();
            }
        }
    }
}
