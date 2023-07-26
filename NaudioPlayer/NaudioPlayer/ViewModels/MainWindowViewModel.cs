using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NAudio.Wave;
using NaudioPlayer.Annotations;
using NaudioPlayer.Extensions;
using NaudioPlayer.Models;
using NaudioPlayer.Services;
using NaudioPlayer.Views;
using NaudioWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;

using System.Windows.Input;

namespace NaudioPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // 建立一個 singleton 
        private static MainWindowViewModel instance = null;

        private enum PlaybackState
        {
            Playing, Stopped, Paused
        }


        private PlaybackState _playbackState;
       
        private ObservableCollection<Track> _playlist;

        private Track _currentlyPlayingTrack;
        private Track _currentlySelectedTrack;

        private AudioPlayer _audioPlayer;
        //獨立開插播的 player
        private AudioPlayer _interludePlayer;

        private Timer _timer;
        private Timer _interludeTimer;

        private InterludeSettingsViewModel _interludeSettings;

        
        private List<Track> _interludeTracks;
        private int _interludeIndex;


        private Track _savedTrack;
        private double _savedTrackPosition;

        private string _title;
        private double _currentTrackLenght;
        private double _currentTrackPosition;
        private double _uiTrackPosition;

        private string _playPauseImageSource;
        private float _currentVolume=0.5f;
        private float _previousVolume;

        private bool _isInterludeEnabled;
        public bool IsInterludeEnabled
        {
            get { return _isInterludeEnabled; }
            set
            {
                _isInterludeEnabled = value;
                OnPropertyChanged();
            }
        }
        private bool _isPlayingInterlude;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string PlayPauseImageSource
        {
            get { return _playPauseImageSource; }
            set
            {
                if (value == _playPauseImageSource) return;
                _playPauseImageSource = value;
                OnPropertyChanged(nameof(PlayPauseImageSource));
            }
        }

        public float CurrentVolume
        {
            get { return _currentVolume; }
            set
            {

                if (value == _currentVolume) return;
                _currentVolume = value;
                OnPropertyChanged(nameof(CurrentVolume));
                if (_audioPlayer != null)
                {
                    _audioPlayer.SetVolume(value);
                }

            }
        }
        
        public double CurrentTrackLenght
        {
            get { return _currentTrackLenght; }
            set
            {
                if (value.Equals(_currentTrackLenght)) return;
                _currentTrackLenght = value;
                OnPropertyChanged(nameof(CurrentTrackLenght));
                OnPropertyChanged(nameof(CurrentTrackLenghtString)); // Add this line
            }
        }
       
        public string CurrentTrackLenghtString
        {
            get => TimeSpan.FromSeconds(CurrentTrackLenght).ToString(@"mm\:ss");
        }

        public double CurrentTrackPosition
        {
            get { return _currentTrackPosition; }
            set
            {
                if (value.Equals(_currentTrackPosition)) return;
                _currentTrackPosition = value;
                OnPropertyChanged(nameof(CurrentTrackPosition));
                OnPropertyChanged(nameof(CurrentTrackPositionString)); // Add this line
            }
        }

        public string CurrentTrackPositionString
        {
            get => TimeSpan.FromSeconds(CurrentTrackPosition).ToString(@"mm\:ss");
        }

        public double UiTrackPosition
        {
            get { return _uiTrackPosition; }
            set
            {
                //Debug.WriteLine("UiTrackPosition : " + value);
                if (value.Equals(_uiTrackPosition)) return;
                _uiTrackPosition = value;
                OnPropertyChanged(nameof(UiTrackPosition));
            }
        }

        private bool _isUserDragging;
        public bool IsUserDragging
        {
            get { return _isUserDragging; }
            set
            {
                if (_isUserDragging == value) return;
                _isUserDragging = value;
                OnPropertyChanged(nameof(IsUserDragging));
            }
        }


        public Track CurrentlySelectedTrack
        {
            get { return _currentlySelectedTrack; }
            set
            {
                if (Equals(value, _currentlySelectedTrack)) return;
                _currentlySelectedTrack = value;
                OnPropertyChanged(nameof(CurrentlySelectedTrack));
            }
        }

        public Track CurrentlyPlayingTrack
        {
            get { return _currentlyPlayingTrack; }
            set
            {
                if (Equals(value, _currentlyPlayingTrack)) return;
                _currentlyPlayingTrack = value;
                OnPropertyChanged(nameof(CurrentlyPlayingTrack));
            }
        }

        public ObservableCollection<Track> Playlist
        {
            get { return _playlist; }
            set
            {
                if (Equals(value, _playlist)) return;
                _playlist = value;
                OnPropertyChanged(nameof(Playlist));
            }
        }

        private ObservableCollection<WeeklySchedule> LoadScheduleFromJson()
        {
            if (File.Exists("weeklySchedules.json"))
            {
                string json = File.ReadAllText("weeklySchedules.json");
                return JsonConvert.DeserializeObject<ObservableCollection<WeeklySchedule>>(json);
            }
            return null;
        }

        //已播過幾首歌, 給插播用
        private int _songsPlayedSinceLastInterlude = 0;

        public ICommand ExitApplicationCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; private set; }
        public ICommand AddFileToPlaylistCommand { get; set; }
        public ICommand AddFolderToPlaylistCommand { get; set; }
        public ICommand SavePlaylistCommand { get; set; }
        public ICommand LoadPlaylistCommand { get; set; }

        public ICommand RewindToStartCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand StopPlaybackCommand { get; set; }
        public ICommand ForwardToEndCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }

        public ICommand TrackControlMouseDownCommand { get; set; }
        public ICommand TrackControlMouseUpCommand { get; set; }
        public ICommand UiTrackValueChangedCommand { get; set; }
        public ICommand VolumeControlValueChangedCommand { get; set; }
        public ICommand MuteUnmuteCommand { get; set; }

        public ICommand DeleteTrackCommand { get; private set; }

        public ICommand OpenWeeklyScheduleCommand { get; private set; }
        public ICommand OpenInterludeSettingsCommand { get; private set; }

        public ICommand AddScheduleCommand { get; set; }
        public ICommand EditScheduleCommand { get; set; }
        public ICommand DeleteScheduleCommand { get; set; }

        public ICommand ToggleInterludeCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {

            Title = "九太播放器";
            Playlist = new ObservableCollection<Track>();
            //LoadPlaylist(@"_playlist\default.playlist");
                      
            string playlistPath = GetPlaylistPathForCurrentTime();
            LoadPlaylist(@"Resources\Playlist\default.playlist");
            StartPlayback(null);

            _interludeSettings = new InterludeSettingsViewModel(this);       

            _playbackState = PlaybackState.Stopped;
            _timer = new System.Timers.Timer(1000); // 1000 milliseconds or 1 second interval
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;

            _interludeTimer = new System.Timers.Timer(10000); // 10 秒
            _interludeTimer.AutoReset = false; // 我們不希望定時器自動重置
            _interludeTimer.Elapsed += InterludeTimer_Elapsed; // 綁定事件處理器

            LoadCommands();
        }

        public static MainWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainWindowViewModel();
                }
                return instance;
            }
        }

        // 用來更新UI 
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            if (_audioPlayer != null)
            {
                double newPosition = _audioPlayer.GetPositionInSeconds();
                float currentVolume = _audioPlayer.GetVolume();
                int currentVolumePercentage = (int)(currentVolume * 100);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTrackPosition = newPosition;
                    if (!IsUserDragging)  // Add this check
                    {
                        UiTrackPosition = newPosition;
                    }
                });
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
            }
        }

        private void _audioPlayer_PlaybackStopped()
        {
            _playbackState = PlaybackState.Stopped;
            CommandManager.InvalidateRequerySuggested();

            if (_audioPlayer == null)
            {
                return;
            }

            if (_audioPlayer.PlaybackStopType == AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile)
            {
                if (IsInterludeEnabled && !_isPlayingInterlude && _songsPlayedSinceLastInterlude >= _interludeSettings.InterludeAfterXSongs)
                {
                    _songsPlayedSinceLastInterlude = 0;
                    StartInterlude();
                }
                else
                {
                    CurrentlySelectedTrack = Playlist.NextItem(CurrentlyPlayingTrack);
                    StartPlayback(null);
                }
            }
            else if (_audioPlayer.PlaybackStopType == AudioPlayer.PlaybackStopTypes.PlaybackStoppedByUser)
            {
                StartPlayback(null);
            }
        }




        private void _audioPlayer_PlaybackResumed()
        {
            _playbackState = PlaybackState.Playing;
        }

        private void _audioPlayer_PlaybackPaused()
        {
            _playbackState = PlaybackState.Paused;
        }

        private void LoadCommands()
        {
            // Menu commands
            ExitApplicationCommand = new RelayCommand(ExitApplication,_ => true);
            MinimizeWindowCommand = new RelayCommand(MinimizeWindow,_ => true);
            AddFileToPlaylistCommand = new RelayCommand(AddFileToPlaylist, CanAddFileToPlaylist);
            AddFolderToPlaylistCommand = new RelayCommand(AddFolderToPlaylist, CanAddFolderToPlaylist);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, _ => true);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist, _ => true);

            // Player commands
            RewindToStartCommand = new RelayCommand(RewindToStart, CanRewindToStart);
            PlayPauseCommand = new RelayCommand(PlayPause, _ => true);
            StopPlaybackCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            ForwardToEndCommand = new RelayCommand(ForwardToEnd, CanForwardToEnd);
            ShuffleCommand = new RelayCommand(Shuffle, CanShuffle);

            // Event commands
            TrackControlMouseDownCommand = new RelayCommand(TrackControlMouseDown, CanTrackControlMouseDown);
            TrackControlMouseUpCommand = new RelayCommand(TrackControlMouseUp, CanTrackControlMouseUp);
            UiTrackValueChangedCommand = new RelayCommand(UiTrackValueChanged, _ => true);
            VolumeControlValueChangedCommand = new RelayCommand(VolumeControlValueChanged, _ => true);
            MuteUnmuteCommand = new RelayCommand(MuteUnmute, _ => true);

            // Playlist Delete Track
            DeleteTrackCommand = new RelayCommand(DeleteTrack, _ => true);

            // Schedule commands
            OpenWeeklyScheduleCommand = new RelayCommand(OpenWeeklySchedule, _ => true);
            OpenInterludeSettingsCommand = new RelayCommand(OpenInterludeSettings,_ => true);
           
            // interlude toggle command
            ToggleInterludeCommand = new RelayCommand(ToggleInterlude, _ => true);
        }

         // 插播音軌 ( 廣告 )
        private void InterludeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 保存當前播放的音樂和播放位置，然後開始播放插播音樂
            _savedTrack = CurrentlyPlayingTrack;
            _savedTrackPosition = CurrentTrackPosition;
            //兩秒淡出
            _audioPlayer.VolumeFade(0.0f, 2000);
            _audioPlayer.Pause();

            //確認 符合以下條件開始插播 
            // 1. 插播開關 (IsInterludeEnabled)
            // 2. 
            if (IsInterludeEnabled && _interludeSettings.InterludeFilePaths.Count > 0)
            {
                string interludeTrackPath = _interludeSettings.InterludeFilePaths[0];  // 使用插播列表中的第一个文件
                _interludePlayer = new AudioPlayer(interludeTrackPath, CurrentVolume);
                _interludePlayer.PlaybackStopped += _interludePlayer_PlaybackStopped;
                _interludePlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);
            }
        }


        private void _interludePlayer_PlaybackStopped()
        {            
            _interludePlayer.Dispose();
            _interludePlayer = null;
            CurrentlyPlayingTrack = _savedTrack;
            _audioPlayer.SetPosition(_savedTrackPosition);
            _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);
        }

        public void ApplyInterludeSettings()
        {
            // 更新_timer 和 _interludeTracks
            _interludeTimer = new System.Timers.Timer(_interludeSettings.InterludeInterval);
            _interludeTimer.Elapsed += InterludeTimer_Elapsed;
            _interludeTimer.AutoReset = true;
          
            _interludeTracks = _interludeSettings.InterludeFilePaths.Select(path => new Track(path)).ToList();
            _interludeIndex = 0;

            if (_interludePlayer != null)
            {
                _interludePlayer.Dispose();
            }
        }

        private void StartInterlude()
        {
            // If an interlude is already playing, do nothing
            if (_isPlayingInterlude)
            {
                return;
            }

            // If a song is playing, pause it and save the current song and position
            if (_playbackState == PlaybackState.Playing)
            {
                _audioPlayer.Pause();
                _savedTrack = CurrentlyPlayingTrack;
                _savedTrackPosition = CurrentTrackPosition;
            }

            // If no interludes are available, do nothing
            if (_interludeSettings.InterludeFilePaths.Count == 0)
            {
                return;
            }

            // Pick the first interlude track for now
            string interludeTrackPath = _interludeSettings.InterludeFilePaths[0];

            if (_interludePlayer != null)
            {
                _interludePlayer.Dispose();
                _interludePlayer = null;
            }

            // Initialize and setup the interlude player
            _interludePlayer = new AudioPlayer(interludeTrackPath, CurrentVolume);
            _interludePlayer.PlaybackStopped += _interludePlayer_PlaybackStopped;

            // Start the interlude
            _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);

            // Set state to indicate an interlude is playing
            _isPlayingInterlude = true;
            _playbackState = PlaybackState.Playing;
        }

        private void StopInterlude()
        {
            // If no interlude is playing, do nothing
            if (!_isPlayingInterlude)
            {
                return;
            }

            // Stop the interlude player and release its resources
            if (_interludePlayer != null)
            {
                _interludePlayer.Dispose();
                _interludePlayer = null;
            }

            // Set state to indicate no interlude is playing
            _isPlayingInterlude = false;

            // If a song was playing before the interlude, resume it
            if (_savedTrack != null)
            {
                CurrentlyPlayingTrack = _savedTrack;
                _audioPlayer.SetPosition(_savedTrackPosition);
                _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused,CurrentVolume);
                _playbackState = PlaybackState.Playing;
            }
        }

        private void ToggleInterlude(object obj)
        {
            IsInterludeEnabled = !IsInterludeEnabled;
        }



        // Menu commands
        private void ExitApplication(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
            }
            
            Application.Current.Shutdown();
        }
      
        private void MinimizeWindow(object obj)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        
        private void AddFileToPlaylist(object p)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Audio files (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            var result = ofd.ShowDialog();
            if (result == true)
            {
                var friendlyName = ofd.SafeFileName.Remove(ofd.SafeFileName.Length - 4);
                var filepath = ofd.FileName;
                //var track = new Track(ofd.FileName, friendlyName);
                
                
                // Read the duration using NAudio
                using (var reader = new AudioFileReader(filepath))
                {
                    var duration = reader.TotalTime;
                    var track = new Track(filepath, friendlyName, Playlist.Count + 1, duration);
                    Playlist.Add(track);
                } 
            }
        }
        private bool CanAddFileToPlaylist(object p)
        {
            if (_playbackState == PlaybackState.Stopped)
            {
                return true;
            }
            return false;
        }

        private void AddFolderToPlaylist(object p)
        {
            var cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            var result = cofd.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var folderName = cofd.FileName;
                var audioFiles = Directory.EnumerateFiles(folderName, "*.*", SearchOption.AllDirectories)
                                          .Where(f=>f.EndsWith(".wav") || f.EndsWith(".mp3") || f.EndsWith(".wma") || f.EndsWith(".ogg") || f.EndsWith(".flac"));
                int trackNumber = Playlist.Count + 1;
                foreach (var audioFile in audioFiles)
                {
                    var removePath = Path.GetFileName(audioFile);
                    var friendlyName = removePath.Remove(removePath.Length - 4);
                    //var track = new Track(audioFile, friendlyName);
                    using (var reader = new AudioFileReader(audioFile))
                    {
                        var duration = reader.TotalTime;
                        var track = new Track(audioFile, friendlyName, trackNumber, duration);
                        Playlist.Add(track);
                        trackNumber++;
                    }
                }
                Playlist = new ObservableCollection<Track>(Playlist.OrderBy(z => z.Number).ToList());
            }
        }
        private bool CanAddFolderToPlaylist(object p)
        {
            if (_playbackState == PlaybackState.Stopped)
            {
                return true;
            }
            return false;
        }

        private void SavePlaylist(object p)
        {
            foreach (var track in Playlist)
            {
                Console.WriteLine($"Track filepath: {track.Filepath}");
            }
            var sfd = new SaveFileDialog();
            sfd.CreatePrompt = false;
            sfd.OverwritePrompt = true;
            sfd.Filter = "PLAYLIST files (*.playlist) | *.playlist";
            if (sfd.ShowDialog() == true)
            {
                var ps = new PlaylistSaver();
                ps.Save(Playlist, sfd.FileName);
            }
        }
        

        private void LoadPlaylist(object filePathObj = null)
        {
            string filePath = filePathObj as string;

            if (filePath == null)
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "PLAYLIST files (*.playlist) | *.playlist";
                if (ofd.ShowDialog() == true)
                {
                    filePath = ofd.FileName;
                }
            }

            // Resolve to absolute path if filePath is relative
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath));
            }

            if (filePath == null)
            {
                Console.WriteLine("File path is null");
            }
            else if (!File.Exists(filePath))
            {
                Console.WriteLine($"File does not exist: {filePath}");
            }
            else
            {
                var newPlaylist = new PlaylistLoader().Load(filePath);

                if (newPlaylist.Count == 0)
                {
                    MessageBox.Show("The playlist is empty. Please add some songs.");
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Playlist = ToObservableCollection(newPlaylist);
                    });

                    Console.WriteLine($"Successfully loaded {filePath} playlist");
                    Console.WriteLine("Playlist count after loading: " + Playlist.Count);
                }
            }
        }

        


        // Schedule command

        private string GetPlaylistPathForCurrentTime()
        {
            var currentDateTime = DateTime.Now;
            var currentDayOfWeek = currentDateTime.DayOfWeek;
            var currentTime = currentDateTime.TimeOfDay;

            foreach (var schedule in LoadScheduleFromJson())
            {
                TimeSpan scheduleStartTime = TimeSpan.Parse(schedule.StartTime);
                TimeSpan scheduleEndTime = TimeSpan.Parse(schedule.EndTime);
                List<DayOfWeek> scheduleDaysOfWeek = schedule.GetSelectedDaysOfWeek();

                if (scheduleDaysOfWeek.Contains(currentDayOfWeek) &&
                    currentTime >= scheduleStartTime &&
                    currentTime <= scheduleEndTime)
                {
                    return schedule.PlaylistPath;
                }
            }

            return null;
        }

        private void OpenWeeklySchedule(object obj)
        {
            var weeklyScheduleWindow = new WeeklyScheduleWindow();
            weeklyScheduleWindow.ShowDialog();
        }
        

        private void OpenInterludeSettings(object obj)
        {
            // Create an instance of InterludeSettingsWindow
            var settingsWindow = new InterludeSettingsWindow(_interludeSettings);

            // Show the InterludeSettingsWindow
            settingsWindow.ShowDialog();
        }
        

        private ObservableCollection<T> ToObservableCollection<T>(ICollection<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        // Player commands
        private void RewindToStart(object p)
        {
            _audioPlayer.SetPosition(0);
        }
        private bool CanRewindToStart(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private void StartPlayback(object p)
        {
            if (_playbackState == PlaybackState.Playing || _isPlayingInterlude)
            {
                return;
            }

            if (CurrentlyPlayingTrack != CurrentlySelectedTrack || _playbackState == PlaybackState.Stopped || _audioPlayer == null)
            {
                if (_audioPlayer != null)
                {
                    _audioPlayer.PlaybackStopped -= _audioPlayer_PlaybackStopped;  // Unsubscribe from the event
                    _audioPlayer.Dispose();
                }

                _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
                _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
                _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
                _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;

                CurrentlyPlayingTrack = CurrentlySelectedTrack;
                CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
            }

            _audioPlayer.TogglePlayPause(CurrentVolume);
            
            _playbackState = PlaybackState.Playing;

            _timer.Start();
        }


        //private void StartPlayback(object p)
        //{
        //    Debug.WriteLine("StartPlayback() was called from " + new StackTrace());
        //    Debug.WriteLine("_playbackState: " + _playbackState);
        //    if (_playbackState == PlaybackState.Playing || _isPlayingInterlude)
        //    {
        //        return;
        //    }

        //    // Only dispose of the old AudioPlayer and create a new one if we're starting a new track
        //    if (CurrentlyPlayingTrack != CurrentlySelectedTrack || _playbackState == PlaybackState.Stopped || _audioPlayer == null || _playbackState == PlaybackState.Paused)
        //    {
        //        if (_audioPlayer != null)
        //        {
        //            _audioPlayer.Dispose();
        //            _audioPlayer = null;
        //        }

        //        // Create a new audio player for the selected track
        //        _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
        //        _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
        //        _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
        //        _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
        //        _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;

        //        // Update the currently playing track
        //        CurrentlyPlayingTrack = CurrentlySelectedTrack;
        //        CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
        //        Debug.WriteLine("currentlyPlaying track : " + CurrentlyPlayingTrack.FriendlyName);
        //    }

        //    // Start playback
        //    _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);

        //    // Update the playback state
        //    _playbackState = PlaybackState.Playing;
        //    Debug.WriteLine("Updated _playbackState: " + _playbackState);


        //    _timer.Start();
        //}


        private bool CanStartPlayback(object p)
        {
            if (CurrentlySelectedTrack != null)
            {
                return true;
            }
            return false;
        }

        private void StopPlayback(object p)
        {
            if (_audioPlayer == null)
            {
                return;
            }
             
            _audioPlayer.Stop();
            _audioPlayer.Dispose();
            _audioPlayer = null;
            _timer.Stop();

            _playbackState = PlaybackState.Stopped;
        }

        private bool CanStopPlayback(object p)
        {
            if (_playbackState == PlaybackState.Playing || _playbackState == PlaybackState.Paused)
            {
                return true;
            }
            return false;
        }

        private void PausePlayback(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Pause();
                _playbackState = PlaybackState.Paused;
            }
        }

        private void PlayPause(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                PausePlayback(p);
            }
            else if (_playbackState == PlaybackState.Paused || _playbackState == PlaybackState.Stopped)
            {
                if (CurrentlySelectedTrack != null)
                {
                    StartPlayback(p);
                }
            }
        }

        //private void PlayPause(object p)
        //{
        //    Debug.WriteLine("PlayPause() was called. _playbackState: " + _playbackState);
        //    if (_playbackState == PlaybackState.Playing)
        //    {
        //        PausePlayback(p);
        //    }
        //    else if (_playbackState == PlaybackState.Paused || _playbackState == PlaybackState.Stopped)
        //    {
        //        if (CurrentlySelectedTrack != null)
        //        {
        //            if (CurrentlyPlayingTrack != CurrentlySelectedTrack) // Check if a different track is selected
        //            {
        //                StopPlayback(null); // Stop the current playback
        //            }
        //            StartPlayback(p); // Start playback of the selected track
        //        }
        //    }
        //}




        private void ForwardToEnd(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
                _audioPlayer.SetPosition(_audioPlayer.GetLenghtInSeconds());
            }
        }
        private bool CanForwardToEnd(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private void Shuffle(object p)
        {
            Playlist = Playlist.Shuffle();
        }
        private bool CanShuffle(object p)
        {
            if (_playbackState == PlaybackState.Stopped)
            {
                return true;
            }
            return false;
        }

        // Events
        private void TrackControlMouseDown(object p)
        {
            Debug.WriteLine("trackControlMouse Down trigger");
            if (_audioPlayer != null)
            {
                _audioPlayer.Pause();       
            }
            IsUserDragging = true;
        }
        private void TrackControlMouseUp(object p)
        {
            if (_audioPlayer != null)
            {
                Debug.WriteLine("UiTrackPosition in TrackControlMouseUp : " + UiTrackPosition);
                _audioPlayer.SetPosition(UiTrackPosition);
                _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);
            }
            IsUserDragging = false;
        }

        private bool CanTrackControlMouseDown(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }
        private bool CanTrackControlMouseUp(object p)
        {
            //if (_playbackState == PlaybackState.Playing) 
            //{ 
            //    return true;
            //}
            //return false;
            return true;
        }

        private void UiTrackValueChanged(object p)
        {
            if (_audioPlayer != null && IsUserDragging)
            {
                //Debug.WriteLine("current UiTrackposition value: " + UiTrackPosition);
                _audioPlayer.SetPosition(UiTrackPosition);
            }
        }
        private void VolumeControlValueChanged(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.SetVolume(CurrentVolume);
            }
        }

        private void MuteUnmute(object obj)
        {
            if (_audioPlayer != null)
            {
                if (_audioPlayer.GetVolume() > 0)
                {
                    _previousVolume = _audioPlayer.GetVolume();
                    _audioPlayer.SetVolume(0);
                    CurrentVolume = 0;
                }
                else
                {
                    _audioPlayer.SetVolume(_previousVolume);
                    CurrentVolume = _previousVolume;
                }
            }
        }

        // Playlist Track Delete

        private void DeleteTrack(object track)
        {
            if (track is Track trackToDelete)
            {
                _playlist.Remove(trackToDelete);
            }
        }

       

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
