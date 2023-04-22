using System;
using System.Collections.Generic;
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
}