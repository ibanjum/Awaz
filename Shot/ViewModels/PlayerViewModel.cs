using System;
using System.Threading;
using System.Windows.Input;
using Shot.Enumerations;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using Xamarin.Forms;

namespace Shot.ViewModels
{
    public class PlayerViewModel : BaseViewModel<RecordingCellModel>
    {
        private readonly IPlayerService _playerService;

        public string PlayCommandText
        {
            get { return GetPropertyValue<string>(); }
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

        public RecordingStatus PlayingStatus
        {
            get { return GetPropertyValue(_playerService.Status); }
            set
            {
                SetPropertyValue(value);
                SetPlayerButtonText();
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

        private bool isMediaPlayerOn;

        public PlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
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
            SliderMaximum = recordingCellModel.Duration;
            TotalDurationText = TimeSpan.FromMilliseconds(recordingCellModel.Duration).ToString(@"hh\:mm\:ss");
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
            _playerService.SeekTo(5000, isRelative: true);
        }

        private void OnRewindPressed()
        {
            _playerService.SeekTo(-5000, isRelative: true);
        }

        private void SetPlayerButtonText()
        {
            switch (PlayingStatus)
            {
                case RecordingStatus.Stopped:
                case RecordingStatus.Paused:
                    PlayCommandText = AppResources.PlayText;
                    break;
                case RecordingStatus.Running:
                    PlayCommandText = AppResources.RecordingPauseLabel;
                    break;
            }
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

        public override void NavigateBack()
        {
            base.NavigateBack();
            isMediaPlayerOn = false;
        }
    }
}
