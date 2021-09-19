using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Shot.Enumerations;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using Xamarin.Forms;

namespace Shot.ViewModels
{
    public class BaseConsistentPlayerModel
    {
        public RecordingCellModel CurrentRecording { get; set; }
        public ObservableCollection<RecordingCellModel> Recordings { get; set; }
        public IPlayerService PlayerService { get; set; }
    }

    public class ConsistentPlayerViewModel : BaseViewModel<BaseConsistentPlayerModel>, IMessageSender
    {
        public IPlayerService PlayerService;
        private readonly INavigationService _navigationService;
        public bool IsPlayerOn => PlayingStatus != RecordingStatus.Stopped;

        public ObservableCollection<RecordingCellModel> Recordings
        {
            get { return GetPropertyValue<ObservableCollection<RecordingCellModel>>(); }
            set { SetPropertyValue(value); }
        }

        public RecordingCellModel CurrentRecording
        {
            get { return GetPropertyValue<RecordingCellModel>(); }
            set { SetPropertyValue(value); }
        }

        public string PlayPauseImageSource
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public RecordingStatus? PlayingStatus
        {
            get { return GetPropertyValue<RecordingStatus>(); }
            set
            {
                SetPropertyValue(value);
                PlayPauseImageSource = PlayingStatus == RecordingStatus.Running ? ImageNames.PlayerPauseImage : ImageNames.PlayerPlayImage;
                OnPropertyChanged(nameof(IsPlayerOn));
            }
        }

        public int CurrentSliderPosition
        {
            get { return GetPropertyValue<int>(); }
            set { SetPropertyValue(value); }
        }

        public ICommand PlayCommand => new Command(OnPlayPressed);

        public ConsistentPlayerViewModel(INavigationService navService) : base(navService)
        {
            _navigationService = navService;
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                if (IsPlayerOn)
                {
                    UpdateTimerData();
                }
                return true;
            });

            MessagingCenter.Subscribe<IMessageSender>(this, "NextRecording", (e) =>
            {
                OnForwardPressed();
            });
        }

        public virtual void OnForwardPressed()
        {
            PlayerService.Pause();
            var currentIndex = Recordings.IndexOf(CurrentRecording);
            RecordingCellModel nxtRecording;

            if (currentIndex + 1 < Recordings.Count)
                nxtRecording = Recordings[currentIndex + 1];
            else
                nxtRecording = Recordings[0];

            PlayerService.SetPlayer(nxtRecording.FilePath);
            CurrentRecording = nxtRecording;
            PlayerService.Play();
        }

        public override void Init(BaseConsistentPlayerModel parameter)
        {
            CurrentRecording = parameter.CurrentRecording;
            Recordings = parameter.Recordings;
            PlayerService = parameter.PlayerService;
            PlayingStatus = PlayerService.Status;
        }

        public virtual void UpdateTimerData()
        {
            if (PlayerService == null)
                return;

            CurrentSliderPosition = PlayerService.GetCurrentPosition();
            if (PlayerService.GetMetaDataDuration(CurrentRecording?.FilePath).Equals(PlayerService.GetCurrentPosition()))
                PlayingStatus = RecordingStatus.Paused;
        }

        public async Task OnConsistentPlayerPressed()
        {
            await _navigationService.NavigateTo<PlayerViewModel, BaseConsistentPlayerModel>(
                new BaseConsistentPlayerModel()
                {
                    CurrentRecording = CurrentRecording,
                    PlayerService = PlayerService,
                    Recordings = Recordings
                });
        }

        private void OnPlayPressed()
        {
            if (PlayerService.FilePath == CurrentRecording.FilePath)
            {
                switch (PlayingStatus)
                {
                    case RecordingStatus.Stopped:
                    case RecordingStatus.Paused:
                        PlayerService.Play();
                        break;
                    case RecordingStatus.Running:
                        PlayerService.Pause();
                        break;
                }
            }
            else
            {
                PlayerService.Pause();
                PlayerService.SetPlayer(CurrentRecording.FilePath);
                PlayerService.Play();
            }

            PlayingStatus = PlayerService.Status;
        }
    }
}
