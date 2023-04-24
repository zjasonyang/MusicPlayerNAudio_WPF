using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaudioPlayer.Models
{
    public class WeeklySchedule
    {
        //public string DayOfWeek { get; set; }
        public string TimeOfDay { get; set; }
        public bool IsSelected { get; set; }

        public string Name { get; set; }
        public string PlaylistPath { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
    }

    public class SelectableDay
    {
        //for checkbox
        public DayOfWeek Day { get; set; }
        public string DayName => Day.ToString();
        public bool IsSelected { get; set; }
    }
}