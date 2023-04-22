using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaudioPlayer.Models
{
    public class WeeklySchedule
    {
        public string DayOfWeek { get; set; }
        public string TimeOfDay { get; set; }
        public bool IsSelected { get; set; }
    }
}