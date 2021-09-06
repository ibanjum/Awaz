using Android.Content;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SearchBar), typeof(Shot.Droid.CustomUI.IconColorSearchBarRenderer))]
namespace Shot.Droid.CustomUI
{
    public class IconColorSearchBarRenderer : SearchBarRenderer
    {
        public IconColorSearchBarRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);
            var icon = Control?.FindViewById(Context.Resources.GetIdentifier("android:id/search_mag_icon", null, null));
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            var iconColor = currentTheme == OSAppTheme.Dark ? Android.Graphics.Color.White : Android.Graphics.Color.Black;
            (icon as ImageView)?.SetColorFilter(iconColor);

            var plateId = Resources.GetIdentifier("android:id/search_plate", null, null);
            var plate = Control.FindViewById(plateId);
            plate.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }
    }
}
