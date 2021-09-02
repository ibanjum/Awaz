using System;
using System.Threading.Tasks;
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

        public string DurationText
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

        public PlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            _playerService = DependencyService.Get<IPlayerService>();
            PlayingStatus = RecordingStatus.Stopped;
            SetPlayerTimer();
        }

        public override void Init(RecordingCellModel recordingCellModel)
        {
            RecordingCellModel = recordingCellModel;
        }

        private void OnPlayPressed()
        {
            switch (PlayingStatus)
            {
                case RecordingStatus.Stopped:
                    _playerService.Play(RecordingCellModel?.FilePath);
                    break;
                case RecordingStatus.Paused:
                    _playerService.Resume();
                    break;
                case RecordingStatus.Running:
                    _playerService.Pause();
                    break;
            }

            PlayingStatus = _playerService.Status;
        }

        private void OnForwardPressed()
        {
            _playerService.SeekTo(5, true);
        }

        private void OnRewindPressed()
        {
            _playerService.SeekTo(5, false);
        }

        private void SetPlayerButtonText()
        {
            switch (PlayingStatus)
            {
                case RecordingStatus.Stopped:
                    PlayCommandText = AppResources.PlayText;
                    break;
                case RecordingStatus.Running:
                    PlayCommandText = AppResources.RecordingPauseLabel;
                    break;
                case RecordingStatus.Paused:
                    PlayCommandText = AppResources.RecordingResumeLabel;
                    break;
            }
        }

        private void SetPlayerTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                DurationText = _playerService.GetCurrentPosition();
                return true;
            });
        }
    }
}
