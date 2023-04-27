using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NaudioPlayer.Models
{
    public class WeeklySchedule : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _playlistPath;
        public string PlaylistPath
        {
            get => _playlistPath;
            set
            {
                _playlistPath = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _startTime;
        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _endTime;
        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        private List<DayOfWeek> _daysOfWeek;
        public List<DayOfWeek> DaysOfWeek
        {
            get => _daysOfWeek;
            set
            {
                _daysOfWeek = value;
                OnPropertyChanged();
            }
        }

        public WeeklySchedule()
        {
            Name = "New Schedule";
            PlaylistPath = "newPath";
            //StartTime = TimeSpan.Parse("08:00");
            //EndTime = TimeSpan.Parse("16:00");
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}