using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Ninject;
using Shot.Extensions;
using Shot.Services;
using Shot.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Shot.Droid.Services;
using Android.Media;
using static Android.Views.View;
using Android.Content.Res;
using System;
using System.Threading.Tasks;

namespace Shot.Droid
{
    [Activity(Label = "Shot", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IMessageSender, IOnTouchListener
    {
        public static Context Context;
        private ImageView imageView;
        private Android.Views.View _consistentPlayerView;
        private bool isPlaying;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            Context = this;

            var metrics = Resources.DisplayMetrics;
            int height = metrics.HeightPixels;
            int width = metrics.WidthPixels;

            var inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
            _consistentPlayerView = inflater.Inflate(Resource.Layout.mainx, null);

            SetUpConsistentPlayer(width, height);
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

        private void SetUpConsistentPlayer(int width, int height)
        {
            var slider = _consistentPlayerView.FindViewById<SeekBar>(Resource.Id.seekBar);
            var title = _consistentPlayerView.FindViewById<TextView>(Resource.Id.recordingName);
            var button = _consistentPlayerView.FindViewById<FrameLayout>(Resource.Id.frameView);
            imageView = _consistentPlayerView.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.SetColorFilter(Android.Graphics.Color.White);
            slider.SetOnTouchListener(this);
            slider.Thumb.Mutate().SetAlpha(0);
            MediaMetadataRetriever mmr = new MediaMetadataRetriever();

            MessagingCenter.Subscribe<IMessageSender, string>(this, "CPI", (a, filePath) =>
            {
                var parent = (ViewGroup)_consistentPlayerView.Parent;
                if (Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Last().BindingContext is PlayerViewModel)
                {
                    if (parent != null)
                        parent.RemoveView(_consistentPlayerView);

                    return;
                }

                if (parent != null)
                    parent.RemoveView(_consistentPlayerView);

                mmr.SetDataSource(filePath);
                ushort.TryParse(mmr.ExtractMetadata(MetadataKey.Duration), out var result);

                slider.Max = result;
                title.Text = FileExtension.GetEnteredName(filePath);
                imageView.SetImageResource(Resource.Drawable.player_pause_icon);
                isPlaying = true;

                button.Click -= OnPlayPressed;
                button.Click += OnPlayPressed;

                _consistentPlayerView.Click -= async (s, e) => { await OnPlayerPressed(); };
                _consistentPlayerView.Click += async (s, e) => { await OnPlayerPressed(); };

                AddContentView(_consistentPlayerView, new ViewGroup.LayoutParams(width, height - 100));
            });

            MessagingCenter.Subscribe<IMessageSender, string>(this, "PT", (a, postion) =>
            {
                int.TryParse(postion, out var res);
                slider.Progress = res;
            });
        }

        private async Task OnPlayerPressed()
        {
            var vm = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Last().BindingContext as ConsistentPlayerViewModel;
            await vm.OnConsistentPlayerPressed();

            var parent = (ViewGroup)_consistentPlayerView.Parent;

            if (parent != null)
                parent.RemoveView(_consistentPlayerView);
        }

        private void OnPlayPressed(object sender, System.EventArgs e)
        {
            MessagingCenter.Send<IMessageSender, string>(this, "PlayPause", isPlaying.ToString());
            if (isPlaying)
                imageView.SetImageResource(Resource.Drawable.player_play_icon);
            else
                imageView.SetImageResource(Resource.Drawable.player_pause_icon);

            isPlaying = !isPlaying;
        }

        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            return true;
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            var metrics = Resources.DisplayMetrics;
            int height = metrics.HeightPixels;
            int width = metrics.WidthPixels;

            var parent = (ViewGroup)_consistentPlayerView.Parent;

            if (parent != null)
                parent.RemoveView(_consistentPlayerView);

            AddContentView(_consistentPlayerView, new ViewGroup.LayoutParams(width, height - 100));
        }
    }
}
