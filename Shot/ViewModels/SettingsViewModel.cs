using Xamarin.Essentials;
using System.Threading.Tasks;
using Shot.Navigation;
using Xamarin.Forms;
using System;
using System.Windows.Input;
using System.Diagnostics;

namespace Shot.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public string Title => AppResources.SettingsTitle;
        public string ThemeText => AppResources.ThemeLabel;
        public string LightText => AppResources.LightLabel;
        public string DarkText => AppResources.DarkLabel;

        public bool IsDarkModeOn
        {
            get { return GetPropertyValue<bool>(); }
            set
            {
                SetPropertyValue(value);
                ThemeClicked.Execute(null);
            }
        }

        private ICommand ThemeClicked => new Command(OnThemeClicked);
        private const string CURRENT_THEME_KEY = "CurrentTheme";

        public SettingsViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        public override Task Init()
        {
            var currentTheme = Application.Current.RequestedTheme;
            IsDarkModeOn = currentTheme == OSAppTheme.Dark;
            return Task.CompletedTask;
        }

        private async void OnThemeClicked()
        {
            Application.Current.UserAppTheme = IsDarkModeOn ? OSAppTheme.Dark : OSAppTheme.Light;

            try
            {
                await SecureStorage.SetAsync(CURRENT_THEME_KEY, Application.Current.RequestedTheme.ToString());
            }
            catch
            {
                Debugger.Break();
            }
        }
    }
}
