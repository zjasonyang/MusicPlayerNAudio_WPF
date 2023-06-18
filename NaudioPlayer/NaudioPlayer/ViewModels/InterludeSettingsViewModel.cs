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

        public ICommand AddTrackCommand { get; private set; }
        public ICommand RemoveTrackCommand { get; private set; }

        public ICommand ApplySettingsCommand { get; private set; }
        public ICommand CloseWindowCommand { get; private set; }


        public InterludeSettingsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            _interludeInterval = 1000;  // 初始化为1000毫秒
            _interludeFilePaths = new ObservableCollection<string>();  // 初始化為空列表

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
                OnPropertyChanged(nameof(InterludeFilePaths)); // Trigger the property changed event
            }
        }

        private void RemoveTrack(object obj)
        {
            string selectedTrack = obj as string;

            if (selectedTrack != null && _interludeFilePaths.Contains(selectedTrack))
            {
                _interludeFilePaths.Remove(selectedTrack);
                OnPropertyChanged(nameof(InterludeFilePaths)); // Trigger the property changed event
            }
        }

        private void ApplySettings(object obj)
        {
            // Save or apply your settings here
            _mainWindowViewModel.ApplyInterludeSettings();

            // Prepare the message to display
            var message = $"載入成功!\n插播檔案：\n{string.Join("\n", InterludeFilePaths)}\n插播間隔時間：{InterludeInterval} 毫秒";

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
