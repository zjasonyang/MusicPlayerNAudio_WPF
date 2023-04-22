using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NaudioPlayer.Models;

namespace NaudioPlayer.ViewModels
{
    public class WeeklyScheduleWindowViewModel : INotifyPropertyChanged
    {
        private WeeklySchedule _selectedSchedule;

        public ObservableCollection<WeeklySchedule> WeeklySchedules { get; set; }
        public WeeklySchedule SelectedSchedule
        {
            get { return _selectedSchedule; }
            set
            {
                _selectedSchedule = value;
                OnPropertyChanged(nameof(SelectedSchedule));
            }
        }

        public WeeklyScheduleWindowViewModel()
        {
            TimeOfDayOptions = new ObservableCollection<string>
            {
                "Morning",
                "Afternoon",
                "Night"
            };

            WeeklySchedules = new ObservableCollection<WeeklySchedule>
            {
                new WeeklySchedule { DayOfWeek = "一", TimeOfDay = "上午" },
                new WeeklySchedule { DayOfWeek = "一", TimeOfDay = "下午" },
                new WeeklySchedule { DayOfWeek = "一", TimeOfDay = "晚上" },
                new WeeklySchedule { DayOfWeek = "二", TimeOfDay = "上午" },
                new WeeklySchedule { DayOfWeek = "三", TimeOfDay = "Night" },
                new WeeklySchedule { DayOfWeek = "四", TimeOfDay = "Night" },
                new WeeklySchedule { DayOfWeek = "五", TimeOfDay = "Night" },
                new WeeklySchedule { DayOfWeek = "六", TimeOfDay = "Night" },
                new WeeklySchedule { DayOfWeek = "日", TimeOfDay = "Night" },
                // ... (repeat for each day of the week)
            };
        }

        public ObservableCollection<string> TimeOfDayOptions { get; set; }
        // INotifyPropertyChanged implementation
        // ...
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }


}
