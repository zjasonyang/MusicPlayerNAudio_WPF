using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NaudioPlayer.ViewModels
{
    public class InterludeSettingsViewModel : INotifyPropertyChanged
    {
        private MainWindowViewModel _mainWindowViewModel;

        private ObservableCollection<string> _interludeFilePaths;
        public ObservableCollection<string> InterludeFilePaths
        {
            get { return _interludeFilePaths; }
            set
            {
                if (_interludeFilePaths != value)
                {
                    _interludeFilePaths = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _interludeFileNames;
        public ObservableCollection<string> InterludeFileNames
        {
            get { return _interludeFileNames; }
            set
            {
                if (_interludeFileNames != value)
                {
                    _interludeFileNames = value;
                    OnPropertyChanged();
                }
            }
        }

        public int InterludeIntervalMinutes { get; set; }
        public int InterludeAfterXSongs { get; set; }

        private bool _isInterludeAfterXSongsEnabled;
        public bool IsInterludeAfterXSongsEnabled
        {
            get { return _isInterludeAfterXSongsEnabled; }
            set
            {
                if (_isInterludeAfterXSongsEnabled != value)
                {
                    _isInterludeAfterXSongsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isInterludeTimeIntervalEnabled;
        public bool IsInterludeTimeIntervalEnabled
        {
            get { return _isInterludeTimeIntervalEnabled; }
            set
            {
                if (_isInterludeTimeIntervalEnabled != value)
                {
                    _isInterludeTimeIntervalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSpecificInterludeTimesEnabled;
        public bool IsSpecificInterludeTimesEnabled
        {
            get { return _isSpecificInterludeTimesEnabled; }
            set
            {
                if (_isSpecificInterludeTimesEnabled != value)
                {
                    _isSpecificInterludeTimesEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private DispatcherTimer _interludeTimer;

        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _progressSeconds;
        public int ProgressSeconds
        {
            get { return _progressSeconds; }
            set
            {
                if (_progressSeconds != value)
                {
                    _progressSeconds = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<TimeSpan> InterludeTimes { get; set; }
        //private List<TimeSpan> _interludeTimes;
        //public List<TimeSpan> InterludeTimes
        //{
        //    get { return _interludeTimes; }
        //    set
        //    {
        //        if (_interludeTimes != value)
        //        {
        //            _interludeTimes = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        private DateTime? _selectedTime;
        public DateTime? SelectedTime
        {
            get { return _selectedTime; }
            set
            {
                if (_selectedTime != value)
                {
                    _selectedTime = value;
                    OnPropertyChanged("SelectedTime");
                }
            }
        }

        private string _selectedTrack;
        public string SelectedTrack
        {
            get { return _selectedTrack; }
            set
            {
                if (_selectedTrack != value)
                {
                    _selectedTrack = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddTrackCommand { get; private set; }
        public ICommand RemoveTrackCommand { get; private set; }

        public ICommand AddScheduleTimeCommand { get; private set; }
        public ICommand RemoveScheduleTimeCommand { get; private set; }

        public ICommand ApplySettingsCommand { get; private set; }
        public ICommand CloseWindowCommand { get; private set; }

        public ICommand StartTimerCommand { get; private set; }

        public InterludeSettingsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            _interludeFilePaths = new ObservableCollection<string>();  // 初始化為空列表
            _interludeFileNames = new ObservableCollection<string>();

            AddTrackCommand = new RelayCommand(AddTrack, _ => true);
            RemoveTrackCommand = new RelayCommand(RemoveTrack, _ => true);

            AddScheduleTimeCommand = new RelayCommand(AddScheduleTime, CanAddScheduleTime);
            RemoveScheduleTimeCommand = new RelayCommand(RemoveScheduleTime, CanRemoveScheduleTime);

            ApplySettingsCommand = new RelayCommand(ApplySettings, _ => true);
            CloseWindowCommand = new RelayCommand(CloseWindow, _ => true);

            StartTimerCommand = new RelayCommand(StartTimer, _ => true);

            InterludeTimes = new ObservableCollection<TimeSpan>();
            
            
            _interludeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _interludeTimer.Tick += TimerTick;

        }


        private void TimerTick(object sender, EventArgs e)
        {
            ProgressValue += 1;
            ProgressSeconds += 1;
        }
        private void StartTimer(object obj)
        {
            _interludeTimer.Start();
        }


        private void AddTrack(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                _interludeFilePaths.Add(openFileDialog.FileName);
                Debug.WriteLine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this));

                _interludeFileNames.Add(System.IO.Path.GetFileName(openFileDialog.FileName));
                OnPropertyChanged(nameof(InterludeFilePaths));
                OnPropertyChanged(nameof(InterludeFileNames)); // Don't forget to notify changes
            }
        }
        private void RemoveTrack(object obj)
        {
            string selectedTrack = SelectedTrack;

            if (selectedTrack != null && _interludeFilePaths.Contains(selectedTrack))
            {
                int index = _interludeFilePaths.IndexOf(selectedTrack);
                _interludeFilePaths.Remove(selectedTrack);
                _interludeFileNames.RemoveAt(index); // remove the corresponding filename
                OnPropertyChanged(nameof(InterludeFilePaths));
                OnPropertyChanged(nameof(InterludeFileNames)); // Don't forget to notify changes
            }
        }

        private void AddScheduleTime(object obj)
        {
            if (SelectedTime.HasValue)
            {
                // Convert the selected time to a TimeSpan
                TimeSpan selectedTimeSpan = SelectedTime.Value.TimeOfDay;

                // Add the selected time to the list
                InterludeTimes.Add(selectedTimeSpan);
            }
        }
        private bool CanAddScheduleTime(object obj)
        {
            // code to determine whether a time can be added
            // for example, check that a time is selected and not already in the list
            return SelectedTime.HasValue && !InterludeTimes.Contains(SelectedTime.Value.TimeOfDay);
        }

        private void RemoveScheduleTime(object obj)
        {
            if (SelectedTime.HasValue)
            {
                // Convert the selected time to a TimeSpan
                TimeSpan selectedTimeSpan = SelectedTime.Value.TimeOfDay;

                // Remove the selected time from the list
                InterludeTimes.Remove(selectedTimeSpan);
            }
        }
        private bool CanRemoveScheduleTime(object obj)
        {
            // code to determine whether a time can be removed
            // for example, check that a time is selected and is in the list
            // Check that a time is selected and is in the list
            return SelectedTime.HasValue && InterludeTimes.Contains(SelectedTime.Value.TimeOfDay);
        }
    


        // 用 event 的方式來通知 套用設定 
        public event Action SettingsApplied;
        private void ApplySettings(object obj)
        {
            // Save or apply your settings here
            _mainWindowViewModel.ApplyInterludeSettings();

            // Prepare the message to display
            var message = $"載入成功!\n插播檔案：\n{string.Join("\n", InterludeFilePaths)}\n插播間隔時間：{InterludeIntervalMinutes} 分鐘";

            // Show the message box
            MessageBox.Show(message, "設定已載入", MessageBoxButton.OK, MessageBoxImage.Information);

            // Raise the event
            SettingsApplied?.Invoke();

            // also fire startTimer command

            _interludeTimer.Start();
            // If you're using a Window to host your ViewModel, you can use this line to close the Window.
            // If you're using a different host, you may need to use a different method to close it.
            if (obj is Window window)
            {
                window.Close();
            }


        }

        private void CloseWindow(object obj)
        {
            if(obj is Window window)
            {
                window.Close();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
