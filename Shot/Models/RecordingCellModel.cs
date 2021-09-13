using System;
using System.Windows.Input;
using Shot.ViewModels;

namespace Shot.Models
{
    public class RecordingCellModel : NotifyPropertyChanged
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string CreationTime { get; set; }
        public int Duration { get; set; }
        public string FileSize { get; set; }
        public string FileFormat { get; set; }
        public ICommand LongPressCommand { get; set; }
        public ICommand ClickPressCommand { get; set; }
        public bool IsSelected
        {
            get { return GetPropertyValue<bool>(); }
            set { SetPropertyValue(value); }
        }
    }
}
