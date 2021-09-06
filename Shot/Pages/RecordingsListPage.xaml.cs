using System;
using System.Linq;
using Shot.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Shot.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordingsListPage : ContentPage
    {
        public RecordingsListPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as BaseViewModel).Init();
        }

        void OnSearchFieldTextChanged(object sender, TextChangedEventArgs e)
        {
            var list = (BindingContext as RecordingsListViewModel).Recordings;
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                recordingsList.ItemsSource = list;
            }
            else
            {
                recordingsList.ItemsSource = list.Where(x => x.Name.StartsWith(e.NewTextValue));
            }
        }
    }
}
