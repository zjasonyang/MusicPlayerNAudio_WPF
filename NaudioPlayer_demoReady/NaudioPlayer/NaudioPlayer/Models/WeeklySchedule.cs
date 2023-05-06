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


        private bool _mondayIsSelected;
        public bool MondayIsSelected
        {
            get { return _mondayIsSelected; }
            set
            {
                _mondayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        private bool _tuesdayIsSelected;
        public bool TuesdayIsSelected
        {
            get { return _tuesdayIsSelected; }
            set
            {
                _tuesdayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        private bool _wednesdayIsSelected;
        public bool WednesdayIsSelected
        {
            get { return _wednesdayIsSelected; }
            set
            {
                _wednesdayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        private bool _thursdayIsSelected;
        public bool ThursdayIsSelected
        {
            get { return _thursdayIsSelected; }
            set
            {
                _thursdayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        private bool _fridayIsSelected;
        public bool FridayIsSelected
        {
            get { return _fridayIsSelected; }
            set
            {
                _fridayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        private bool _saturdayIsSelected;
        public bool SaturdayIsSelected
        {
            get { return _saturdayIsSelected; }
            set
            {
                _saturdayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        private bool _sundayIsSelected;
        public bool SundayIsSelected
        {
            get { return _sundayIsSelected; }
            set
            {
                _sundayIsSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DaysOfWeekString));
            }
        }
        
        public string DaysOfWeekString
        {
            get
            {
                List<string> selectedDays = new List<string>();

                if (MondayIsSelected) selectedDays.Add("一");
                if (TuesdayIsSelected) selectedDays.Add("二");
                if (WednesdayIsSelected) selectedDays.Add("三");
                if (ThursdayIsSelected) selectedDays.Add("四");
                if (FridayIsSelected) selectedDays.Add("五");
                if (SaturdayIsSelected) selectedDays.Add("六");
                if (SundayIsSelected) selectedDays.Add("日");

                return string.Join(", ", selectedDays);
            }
        }

        public List<DayOfWeek> GetSelectedDaysOfWeek()
        {
            List<DayOfWeek> selectedDays = new List<DayOfWeek>();

            if (MondayIsSelected) selectedDays.Add(DayOfWeek.Monday);
            if (TuesdayIsSelected) selectedDays.Add(DayOfWeek.Tuesday);
            if (WednesdayIsSelected) selectedDays.Add(DayOfWeek.Wednesday);
            if (ThursdayIsSelected) selectedDays.Add(DayOfWeek.Thursday);
            if (FridayIsSelected) selectedDays.Add(DayOfWeek.Friday);
            if (SaturdayIsSelected) selectedDays.Add(DayOfWeek.Saturday);
            if (SundayIsSelected) selectedDays.Add(DayOfWeek.Sunday);

            return selectedDays;
        }

        public WeeklySchedule()
        {
            Name = "Default";
            PlaylistPath = "default";
            StartTime = TimeSpan.Parse("08:00").ToString(@"hh\:mm");
            EndTime = TimeSpan.Parse("22:00").ToString(@"hh\:mm");
            MondayIsSelected = false;
            TuesdayIsSelected = false;
            WednesdayIsSelected = false;
            ThursdayIsSelected = false;
            FridayIsSelected = false;
            SaturdayIsSelected = false;
            SundayIsSelected = true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}