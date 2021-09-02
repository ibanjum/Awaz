using System;
using System.Collections.ObjectModel;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using System.IO;
using Xamarin.Forms;
using Shot.Extensions;

namespace Shot.ViewModels
{
    public class RecordingsListViewModel : BaseViewModel
    {
        private readonly IPlayerService _playerService;
        private readonly INavigationService _navigationService;

        public string RecordingsListTitle => AppResources.RecordingsListTitle;

        public ObservableCollection<RecordingCellModel> Recordings
        {
            get { return GetPropertyValue<ObservableCollection<RecordingCellModel>>(); }
            set { SetPropertyValue(value); }
        }

        public RecordingCellModel SelectedRecording
        {
            get { return GetPropertyValue<RecordingCellModel>(); }
            set
            {
                SetPropertyValue(value);
                if (SelectedRecording == null)
                    return;

                _navigationService.NavigateTo<PlayerViewModel, RecordingCellModel>(SelectedRecording);
            }
        }

        public RecordingsListViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _playerService = DependencyService.Get<IPlayerService>();
            Recordings = new ObservableCollection<RecordingCellModel>();
            PopulateRecordings();
        }

        private void PopulateRecordings()
        {
            Recordings.Clear();
            var pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "recordings");
            var filePaths = Directory.GetFiles(pathString);
            foreach (var filePath in filePaths)
            {
                Recordings.Add(new RecordingCellModel()
                {
                    Name = GetEnteredName(filePath),
                    FilePath = filePath,
                    CreationTime = MediaExtension.GetCreationTime(filePath),
                    Duration = _playerService.GetDuration(filePath)
                });
            }
        }

        private string GetEnteredName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var split = fileName.Split('.');
            var val = split[0].Replace("-", " ");
            return val;
        }
    }
}
