using System;
using System.Windows.Input;
using Shot.Enumerations;
using Shot.Extensions;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using Xamarin.Forms;

namespace Shot.ViewModels
{
    public class PlayerViewModel : BaseViewModel<RecordingCellModel>
    {
        private readonly IPlayerService _playerService;
        private readonly INavigationService _navigationService;

        public string CreationDateText => AppResources.CreationDateLabel;
        public string RecordingFormatText => AppResources.RecordingFormatLabel;
        public string RecordingSizeText => AppResources.RecorindSizeLabel;
        public string DeleteText => AppResources.DeleteLabel;
        public string ShareText => AppResources.ShareLabel;
        public string MoreText => AppResources.MoreLabel;

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

        public int CurrentSliderPosition
        {
            get { return GetPropertyValue<int>(); }
            set { SetPropertyValue(value); }
        }

        public int SliderMaximum
        {
            get { return GetPropertyValue<int>(); }
            set { SetPropertyValue(value); }
        }

        public string CurrentTimeText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string PlayPauseImageSource
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string CreationDateValue
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

        public string RewindButtonText => string.Format("-{0}", AppResources.TenSecButtonLabel);

        public string ForwardButtonText => string.Format("+{0}", AppResources.TenSecButtonLabel);

        public RecordingStatus PlayingStatus
        {
            get { return GetPropertyValue(_playerService.Status); }
            set
            {
                SetPropertyValue(value);
                PlayPauseImageSource = PlayingStatus == RecordingStatus.Running ? ImageNames.PlayerPauseImage : ImageNames.PlayerPlayImage;
            }
        }

        public RecordingCellModel RecordingCellModel
        {
            get { return GetPropertyValue<RecordingCellModel>(); }
            set { SetPropertyValue(value); }
        }

        public ICommand PlayCommand => new Command(OnPlayPressed);
        public ICommand ForwardCommand => new Command(OnForwardPressed);
        public ICommand RewindCommand => new Command(OnRewindPressed);
        public ICommand DragStartedCommand => new Command(OnDragStarted);
        public ICommand DragCompletedCommand => new Command(OnDragCompleted);
        public ICommand MoreCommand => new Command(OnMorePressed);

        private bool isMediaPlayerOn;

        public PlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _playerService = DependencyService.Get<IPlayerService>();
            SliderMaximum = 1;
            CurrentSliderPosition = 0;
            PlayingStatus = _playerService.Status;
        }

        public override void Init(RecordingCellModel recordingCellModel)
        {
            RecordingCellModel = recordingCellModel;
            _playerService.SetPlayer(RecordingCellModel?.FilePath);
            isMediaPlayerOn = true;
            SetPlayerTimer();
            SliderMaximum = RecordingCellModel.Duration;
            CreationDateValue = RecordingCellModel.CreationTime;
            RecordingFormatValue = RecordingCellModel.FileFormat;
            RecordingSizeValue = RecordingCellModel.FileSize;
            Title = RecordingCellModel.Name;
            TotalDurationText = TimeSpan.FromMilliseconds(RecordingCellModel.Duration).ToString(@"hh\:mm\:ss");
        }

        public override void NavigateBack()
        {
            base.NavigateBack();
            isMediaPlayerOn = false;
        }

        private void OnPlayPressed()
        {
            switch (PlayingStatus)
            {
                case RecordingStatus.Stopped:
                case RecordingStatus.Paused:
                    _playerService.Play();
                    break;
                case RecordingStatus.Running:
                    _playerService.Pause();
                    break;
            }

            PlayingStatus = _playerService.Status;
        }

        private void OnForwardPressed()
        {
            _playerService.SeekTo(10000, isRelative: true);
        }

        private void OnRewindPressed()
        {
            _playerService.SeekTo(-10000, isRelative: true);
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

        private void SetPlayerTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                if (isMediaPlayerOn)
                {
                    CurrentTimeText = TimeSpan.FromMilliseconds(_playerService.GetCurrentPosition()).ToString(@"hh\:mm\:ss");
                    if (!IsDragging)
                    {
                        CurrentSliderPosition = _playerService.GetCurrentPosition();
                    }
                    if (_playerService.GetMetaDataDuration(RecordingCellModel?.FilePath).Equals(_playerService.GetCurrentPosition()))
                        PlayingStatus = RecordingStatus.Stopped;

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private async void OnMorePressed()
        {
            var selectedButton = await _navigationService.DisplayActionSheet(ActionSheetExtension.GetSelectedRecordingActionSheetModel(RecordingCellModel.FilePath));

            if (selectedButton == AppResources.DeleteLabel)
            {
                isMediaPlayerOn = false;
                FileExtension.DeleteFile(RecordingCellModel.FilePath);
                await _navigationService.GoBack();
            }
            else if (selectedButton == AppResources.ShareLabel)
            {
                await ActionSheetExtension.ShareSingleFile(RecordingCellModel.FilePath);
            }
        }
    }
}
