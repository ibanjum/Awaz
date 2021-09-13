using System.Threading.Tasks;
using Shot.Models;
using Xamarin.Essentials;

namespace Shot.Extensions
{
    public static class ActionSheetExtension
    {
        public static ActionSheetModel GetSelectedRecordingActionSheetModel(string filePath)
        {
            return new ActionSheetModel()
            {
                Title = FileExtension.GetEnteredName(filePath),
                Cancel = AppResources.CancelLabel,
                Distruction = AppResources.DeleteLabel,
                Buttons = new string[] { AppResources.ShareLabel }
            };
        }

        public static ActionSheetModel GetDeleteConfirmationActionSheet(int count)
        {
            string title = string.Empty;
            string deleteText = string.Empty;
            switch (count)
            {
                case 0:
                    break;
                case 1:
                    title = AppResources.OneDeleteConfirmationLabel;
                    deleteText = AppResources.DeleteLabel;
                    break;
                default:
                    title = AppResources.DeleteConfirmationLabel;
                    deleteText = string.Format(AppResources.DeleteCountLabel, count);
                    break;
            }
            return new ActionSheetModel()
            {
                Title = title,
                Cancel = AppResources.CancelLabel,
                Distruction = deleteText
            };
        }

        public static async Task ShareSingleFile(string filePath)
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = string.Format(AppResources.SeletedRecordingCountLabel, 1),
                File = new ShareFile(filePath)
            });
        }
    }
}
