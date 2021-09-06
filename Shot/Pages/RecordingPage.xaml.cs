using System;
using System.Collections.Generic;
using LiveChartsCore.SkiaSharpView.Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using Shot.ViewModels;

namespace Shot.Pages
{
    public partial class RecordingPage : ContentPage
    {
        public RecordingPage()
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
