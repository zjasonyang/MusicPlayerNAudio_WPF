using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NaudioPlayer.Annotations;
using NaudioWrapper;

namespace NaudioPlayer.Models
{
    public class Track : INotifyPropertyChanged
    {
        private string _friendlyName;
        private string _filepath;

        private int _number;
        private TimeSpan _time;

        public string FriendlyName
        {
            get { return _friendlyName; }
            set
            {
                if (value == _friendlyName) return;
                _friendlyName = value;
                OnPropertyChanged(nameof(FriendlyName));
            }
        }

        public string Filepath
        {
            get { return _filepath; }
            set
            {
                if (value == _filepath) return;
                _filepath = value;
                OnPropertyChanged(nameof(Filepath));
            }
        }

        public int Number
        {
            get { return _number; }
            set
            {
                if (value == _number) return;
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                if (value == _time) return;
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Track(string filepath, string friendlyName, int number, TimeSpan time)
        {
            Filepath = filepath;
            FriendlyName = friendlyName;
            Number = number;
            Time = time;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
