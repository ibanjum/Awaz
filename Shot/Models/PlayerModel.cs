using System.Collections.ObjectModel;
using Shot.Enumerations;

namespace Shot.Models
{
    public class PlayerModel
    {
        public RecordingCellModel CurrentRecording { get; set; }
        public ObservableCollection<RecordingCellModel> Recordings { get; set; }
        public MediaStatus PlayingStatus { get; set; }
    }
}
