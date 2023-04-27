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

        private string _startTime;
        public string StartTime
        {
            get => _startTime;
            set
            {
                DateTime sdate = new DateTime();
                if(DateTime.TryParse(value, out sdate))
                {
                    _startTime = sdate.ToString("HH:mm");
                    OnPropertyChanged();
                }
            }
        }

        private string _endTime;
        public string EndTime
        {
            get => _endTime;
            set
            {
                DateTime edate = new DateTime();
                if(DateTime.TryParse(value, out edate))
                {
                    _endTime = edate.ToString("HH:mm");
                    OnPropertyChanged();
                }
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
            PlaylistPath = "default";
            StartTime = TimeSpan.Parse("08:00").ToString(@"hh\:mm");
            EndTime = TimeSpan.Parse("16:00").ToString(@"hh\:mm");
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}