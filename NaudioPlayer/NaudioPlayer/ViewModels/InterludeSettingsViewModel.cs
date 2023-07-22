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

namespace NaudioPlayer.ViewModels
{
    public class InterludeSettingsViewModel : INotifyPropertyChanged
    {
        private MainWindowViewModel _mainWindowViewModel;

        //private List<string> _interludeFilePaths;
        //public List<string> InterludeFilePaths

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


        private double _interludeInterval;
        public double InterludeInterval
        {
            get { return _interludeInterval; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Interlude interval must be greater than 0.");
                }
                if (_interludeInterval != value)
                {
                    _interludeInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _interludeIntervalMinutes;
        public double InterludeIntervalMinutes
        {
            get { return _interludeInterval / 60000; } // 1 minute = 60000 milliseconds
            set
            {
                if (_interludeIntervalMinutes != value)
                {
                    _interludeIntervalMinutes = value;
                    _interludeInterval = value * 60000; // convert minutes back to milliseconds
                    OnPropertyChanged(nameof(InterludeIntervalMinutes));
                    OnPropertyChanged(nameof(InterludeInterval));
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

        public List<TimeSpan> InterludeTimes { get; set; } // For specific time interludes
        public int InterludeAfterXSongs { get; set; } // For track-count-based interludes


        public ICommand AddTrackCommand { get; private set; }
        public ICommand RemoveTrackCommand { get; private set; }

        public ICommand ApplySettingsCommand { get; private set; }
        public ICommand CloseWindowCommand { get; private set; }


        public InterludeSettingsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            _interludeInterval = 30000;  // 初始化为30秒
            _interludeFilePaths = new ObservableCollection<string>();  // 初始化為空列表
            _interludeFileNames = new ObservableCollection<string>();

            AddTrackCommand = new RelayCommand(AddTrack, _ => true);
            RemoveTrackCommand = new RelayCommand(RemoveTrack, _ => true);
            ApplySettingsCommand = new RelayCommand(ApplySettings, _ => true);
            CloseWindowCommand = new RelayCommand(CloseWindow, _ => true);
        }

        private void AddTrack(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                _interludeFilePaths.Add(openFileDialog.FileName);
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


        private void ApplySettings(object obj)
        {
            // Save or apply your settings here
            _mainWindowViewModel.ApplyInterludeSettings();

            // Prepare the message to display
            var message = $"載入成功!\n插播檔案：\n{string.Join("\n", InterludeFilePaths)}\n插播間隔時間：{InterludeIntervalMinutes} 分鐘";

            // Show the message box
            MessageBox.Show(message, "設定已載入", MessageBoxButton.OK, MessageBoxImage.Information);

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
