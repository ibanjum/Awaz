using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Ninject;
using Shot.Droid.Services;
using Shot.Enumerations;
using Shot.Extensions;
using Shot.Services;
using Shot.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Shot.Droid
{
    [Activity(Label = "Shot", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Intent _recordingService;
        public static Context Context;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            Context = this;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override async void OnDestroy()
        {
            var filePath = await SecureStorage.GetAsync(ConsStrings.RecordingStartedKey);
            if (!string.IsNullOrEmpty(filePath))
            {
                FileExtension.DeleteFile(filePath);
            }

            base.OnDestroy();
        }
    }
}
