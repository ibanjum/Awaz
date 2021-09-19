using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Shot.Enumerations;
using Shot.Extensions;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using Xamarin.Forms;

namespace Shot.ViewModels
{
    public class PlayerViewModel : ConsistentPlayerViewModel
    {
        private readonly INavigationService _navigationService;

        public string CreationDateText => AppResources.CreationDateLabel;
        public string RecordingFormatText => AppResources.RecordingFormatLabel;
        public string RecordingSizeText => AppResources.RecorindSizeLabel;
        public string DeleteText => AppResources.DeleteLabel;
        public string ShareText => AppResources.ShareLabel;
        public string MoreText => AppResources.MoreLabel;
        public string RewindImageSource => ImageNames.RewindImage;
        public string ForwardImageSource => ImageNames.ForwardImage;
        public string ExpandImageSource => ImageNames.ExpandImage;

        public bool IsDragging
        {
            get { return GetPropertyValue<bool>(); }
            set { SetPropertyValue(value); }
        }
        public string TotalDurationText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string CurrentTimeText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public DateTime CreationDateValue
        {
            get { return GetPropertyValue<DateTime>(); }
            set { SetPropertyValue(value); }
        }

        public int SliderMaximum
        {
            get { return GetPropertyValue<int>(); }
            set { SetPropertyValue(value); }
        }

        public string RecordingName
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string RecordingFormatValue
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string RecordingSizeValue
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string Title
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public ICommand ForwardCommand => new Command(OnForwardPressed);
        public ICommand RewindCommand => new Command(OnRewindPressed);
        public ICommand DragStartedCommand => new Command(OnDragStarted);
        public ICommand DragCompletedCommand => new Command(OnDragCompleted);
        public ICommand MoreCommand => new Command(OnMorePressed);
        public ICommand ExpandCommand => new Command(async () => await OnExpandPressed());

        public PlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            SliderMaximum = 1;
        }

        public override void UpdateTimerData()
        {
            base.UpdateTimerData();

            if (PlayerService == null)
                return;

            CurrentTimeText = TimeSpan.FromMilliseconds(PlayerService.GetCurrentPosition()).ToString(@"hh\:mm\:ss");
        }

        private void UpdateRecording()
        {
            CreationDateValue = CurrentRecording.CreationTime;
            RecordingFormatValue = CurrentRecording.FileFormat;
            RecordingSizeValue = CurrentRecording.FileSize;
            Title = CurrentRecording.Name;
            TotalDurationText = TimeSpan.FromMilliseconds(CurrentRecording.Duration).ToString(@"hh\:mm\:ss");
            RecordingName = CurrentRecording.Name;
            SliderMaximum = CurrentRecording.Duration;
        }

        private void OnRewindPressed()
        {
            PlayerService.Pause();
            var currentIndex = Recordings.IndexOf(CurrentRecording);
            RecordingCellModel prevRecording;

            if (currentIndex - 1 >= 0)
                prevRecording = Recordings[currentIndex - 1];
            else
                prevRecording = Recordings[Recordings.Count - 1];

            PlayerService.SetPlayer(prevRecording.FilePath);
            CurrentRecording = prevRecording;
            PlayerService.Play();
        }

        private void OnDragStarted(object obj)
        {
            IsDragging = true;
        }

        private void OnDragCompleted(object obj)
        {
            PlayerService.SeekTo(CurrentSliderPosition, isRelative: false);
            IsDragging = false;
        }

        private async void OnMorePressed()
        {
            var selectedButton = await _navigationService.DisplayActionSheet(ActionSheetExtension.GetSelectedRecordingActionSheetModel(CurrentRecording.FilePath));

            if (selectedButton == AppResources.DeleteLabel)
            {
                FileExtension.DeleteFile(CurrentRecording.FilePath);
                await _navigationService.GoBack();
            }
            else if (selectedButton == AppResources.ShareLabel)
            {
                await ActionSheetExtension.ShareSingleFile(CurrentRecording.FilePath);
            }
        }

        private async Task OnExpandPressed()
        {
            await _navigationService.GoBackWithParam(
                new BaseConsistentPlayerModel()
                {
                    CurrentRecording = CurrentRecording,
                    PlayerService = PlayerService,
                    Recordings = Recordings
                });
        }
    }
}
