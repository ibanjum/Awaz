using Shot.ViewModels;
using Xamarin.Forms;

namespace Shot.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as BaseViewModel).Init();
        }
    }
}
