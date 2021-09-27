using System;
using System.Collections.ObjectModel;
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
    public class PlayerViewModel : BaseViewModel<PlayerModel>
    {
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;

        public string CreationDateText => AppResources.CreationDateLabel;
        public string RecordingFormatText => AppResources.RecordingFormatLabel;
        public string RecordingSizeText => AppResources.RecorindSizeLabel;
        public string DeleteText => AppResources.DeleteLabel;
        public string ShareText => AppResources.ShareLabel;
        public string MoreText => AppResources.MoreLabel;
        public string RewindImageSource => ImageNames.RewindImage;
        public string ForwardImageSource => ImageNames.ForwardImage;
        public string ExpandImageSource => ImageNames.ExpandImage;

        public RecordingCellModel CurrentRecording
        {
            get { return GetPropertyValue<RecordingCellModel>(); }
            set
            {
                SetPropertyValue(value);
                UpdateRecording();
            }
        }

        public ObservableCollection<RecordingCellModel> Recordings
        {
            get { return GetPropertyValue<ObservableCollection<RecordingCellModel>>(); }
            set { SetPropertyValue(value); }
        }

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

        public MediaStatus PlayingStatus
        {
            get { return GetPropertyValue<MediaStatus>(); }
            set
            {
                SetPropertyValue(value);
                PlayPauseImageSource = PlayingStatus == MediaStatus.Running ? ImageNames.PlayerPauseImage : ImageNames.PlayerPlayImage;
            }
        }

        public string PlayPauseImageSource
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public int CurrentSliderPosition
        {
            get { return GetPropertyValue<int>(); }
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
            _playerService = DependencyService.Get<IPlayerService>();
            SliderMaximum = 1;
            UpdateTimerData();
        }

        public override void Init(PlayerModel parameter)
        {
            Recordings = parameter.Recordings;
            CurrentRecording = parameter.CurrentRecording;
            PlayingStatus = parameter.PlayingStatus;
        }

        private void UpdateTimerData()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                if (PlayingStatus == MediaStatus.Running)
                {
                    CurrentSliderPosition = _playerService.GetCurrentPosition();
                    CurrentTimeText = TimeSpan.FromMilliseconds(_playerService.GetCurrentPosition()).ToString(@"hh\:mm\:ss");
                    if (_playerService.GetMetaDataDuration(CurrentRecording?.FilePath).Equals(_playerService.GetCurrentPosition()))
                        OnForwardPressed();
                }

                return true;
            });
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
            _playerService.Pause();
            var currentIndex = Recordings.IndexOf(CurrentRecording);
            RecordingCellModel prevRecording;

            if (currentIndex - 1 >= 0)
                prevRecording = Recordings[currentIndex - 1];
            else
                prevRecording = Recordings[Recordings.Count - 1];

            _playerService.SetPlayer(prevRecording.FilePath);
            CurrentRecording = prevRecording;
        }

        private void OnForwardPressed()
        {
            _playerService.Pause();
            _playerService.Stop();
            var currentIndex = Recordings.IndexOf(CurrentRecording);
            RecordingCellModel nxtRecording;

            if (currentIndex + 1 < Recordings.Count)
                nxtRecording = Recordings[currentIndex + 1];
            else
                nxtRecording = Recordings[0];

            _playerService.SetPlayer(nxtRecording.FilePath);
            CurrentRecording = nxtRecording;
        }

        private void OnDragStarted(object obj)
        {
            IsDragging = true;
        }

        private void OnDragCompleted(object obj)
        {
            _playerService.SeekTo(CurrentSliderPosition, isRelative: false);
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
               new PlayerModel()
               {
                   Recordings = Recordings,
                   CurrentRecording = CurrentRecording
               });
        }
    }
}
