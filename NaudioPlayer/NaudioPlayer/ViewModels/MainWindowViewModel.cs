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
using System.Windows.Threading;

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

        private InterludeSettingsViewModel _interludeSettings;
        private TimeSpan _interludeInterval;
        private int _interludeAfterXSongs;
        private List<TimeSpan> _interludeTimes;
        private List<string> _interludeTracks;
        private DateTime? _nextInterludeTime;
        private DateTime _lastInterludePlayedTime;

        private bool _isInterludeAfterXSongsEnabled;
        private bool _isInterludeTimeIntervalEnabled;
        //for debug
        private string _interludeCountdown;
        public string InterludeCountdown
        {
            get { return _interludeCountdown; }
            set
            {
                _interludeCountdown = value;
                OnPropertyChanged();
            }
        }
        private int shouldInterludeCalled;

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
        private int _songsPlayedSinceLastInterlude = -1;

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
            Debug.WriteLine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this));


            _interludeSettings.SettingsApplied += ApplyInterludeSettings;
            _interludeTimes = new List<TimeSpan>();
            _interludeTracks = new List<string>();
            _lastInterludePlayedTime = DateTime.Now;


            _playbackState = PlaybackState.Stopped;
            _timer = new System.Timers.Timer(1000); // 1000 milliseconds or 1 second interval
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;

        
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
            
            if (!_isPlayingInterlude && _isInterludeTimeIntervalEnabled && _interludeSettings.InterludeFilePaths.Count > 0 && ShouldPlayInterlude())
            {
                PlayInterlude();
            }

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

            if (_nextInterludeTime.HasValue)
            {
                TimeSpan timeRemaining = _nextInterludeTime.Value - DateTime.Now;
                InterludeCountdown = $"Next interlude in {timeRemaining.TotalSeconds} seconds";
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
                if (_isPlayingInterlude)
                {
                    _isPlayingInterlude = false; // Reset here after an interlude finishes playing
                    PlayNextSong();
                }
                else if (IsInterludeEnabled && _songsPlayedSinceLastInterlude >= _interludeSettings.InterludeAfterXSongs)
                {
                    PlayInterlude();
                }
                else
                {
                    PlayNextSong();
                }
            }

            else if (_audioPlayer.PlaybackStopType == AudioPlayer.PlaybackStopTypes.PlaybackStoppedByUser)
            {
                // Unsubscribe from the event
                if (_audioPlayer != null)
                {
                    _audioPlayer.PlaybackStopped -= _audioPlayer_PlaybackStopped;
                }

                // Re-initialize _audioPlayer with the new track
                _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
                _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;

                // Start playing the new track
                _audioPlayer.Play(NAudio.Wave.PlaybackState.Stopped, CurrentVolume);
                _playbackState = PlaybackState.Playing;
            }
        }

        private void PlayNextSong()
        {
            CurrentlySelectedTrack = Playlist.NextItem(CurrentlyPlayingTrack);
            Debug.WriteLine(" currently selected track: " + CurrentlySelectedTrack.FriendlyName);

            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackStopped -= _audioPlayer_PlaybackStopped; // Unsubscribe
                _audioPlayer.Dispose();
            }

            _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
            _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;

            CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
            CurrentlyPlayingTrack = CurrentlySelectedTrack;

            _audioPlayer.Play(NAudio.Wave.PlaybackState.Stopped, CurrentVolume);
            _playbackState = PlaybackState.Playing;

            _songsPlayedSinceLastInterlude++;
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


        private bool ShouldPlayInterlude()
        {
            shouldInterludeCalled++;
            Debug.WriteLine("shouldPlayInterlude called: " + shouldInterludeCalled);

            // Check if it's time to play an interlude based on the number of songs played
            if (_isInterludeAfterXSongsEnabled)
            {
                if (_interludeAfterXSongs > 0 && _songsPlayedSinceLastInterlude >= _interludeAfterXSongs)
                {
                    Debug.WriteLine("every x song passed.");
                    return true;
                }
            }


            // Check if it's time to play an interlude based on the interval
            if (_isInterludeTimeIntervalEnabled)
            {
                if (_interludeInterval != null)
                {
                    int interludeIntervalSeconds = (int)(_interludeInterval.TotalMinutes * 60);
                    Debug.WriteLine("interlude Interval Seconds:" + interludeIntervalSeconds);
                    Debug.WriteLine("progress seconds: " + _interludeSettings.ProgressSeconds);
                    if (_interludeSettings.ProgressSeconds >= interludeIntervalSeconds)
                    {
                        Debug.WriteLine("every x minutes passed.");
                        // Reset the progress seconds here
                        _interludeSettings.ProgressSeconds = 0;
                        _interludeSettings.ProgressValue = 0;
                        return true;
                    }
                }
            }

            // Check if it's time to play an interlude based on the fixed times
            if (_interludeTimes != null && _interludeTimes.Contains(DateTime.Now.TimeOfDay))
            {
                Debug.WriteLine("fixed time passed.");
                // Remove the current time from the list so it won't be checked again today
                _interludeTimes.Remove(DateTime.Now.TimeOfDay);
                return true;
            }

            // If none of the conditions are met, don't play an interlude
            Debug.WriteLine("FALSE");
            return false;
        }

        private void PlayInterlude()
        {
            // Check if there are any interludes to play
            if (_interludeTracks.Count == 0)
            {
                // No interludes to play, reset interlude conditions
                _lastInterludePlayedTime = DateTime.Now;
                _songsPlayedSinceLastInterlude = 0;

                // Continue with the next song
                StartPlayback(null);
                return;
            }

            // Choose a random track from the interlude tracks list
            var random = new Random();
            int index = random.Next(_interludeTracks.Count);
            string interludeTrack = _interludeTracks[index];

            // Stop the current playback
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
                _audioPlayer.PlaybackStopped -= _audioPlayer_PlaybackStopped;  // Unsubscribe here before disposing
                
                _audioPlayer.Dispose();
            }

            // Stop the current playback
            PausePlayback(null);

            // Create a new audio player for the interlude track
            _audioPlayer = new AudioPlayer(interludeTrack, CurrentVolume);

            // Set up the PlaybackStopped event handler
            _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;

            // Specify that the playback should stop when the end of the file is reached
            _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;

            // Start playback of the interlude track
            _audioPlayer.Play(NAudio.Wave.PlaybackState.Stopped, CurrentVolume);

            // after play interlude, set flag back to false
            _isPlayingInterlude = false;

            _playbackState = PlaybackState.Playing;

            _lastInterludePlayedTime = DateTime.Now;

            _songsPlayedSinceLastInterlude = 0;

        }

        public void ApplyInterludeSettings()
        {
            _interludeInterval = TimeSpan.FromMinutes(_interludeSettings.InterludeIntervalMinutes);
            _interludeAfterXSongs = _interludeSettings.InterludeAfterXSongs;
            _interludeTimes = new List<TimeSpan>(_interludeSettings.InterludeTimes);
            _interludeTracks = new List<string>(_interludeSettings.InterludeFilePaths);

            _isInterludeAfterXSongsEnabled = _interludeSettings.IsInterludeAfterXSongsEnabled;
            _isInterludeTimeIntervalEnabled = _interludeSettings.IsInterludeTimeIntervalEnabled;

            Debug.WriteLine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this));
            _songsPlayedSinceLastInterlude = -1;

            var nextTime = _interludeTimes.OrderBy(t => t).FirstOrDefault(t => DateTime.Now.TimeOfDay < t);

            _nextInterludeTime = nextTime is TimeSpan validTime
                ? DateTime.Today + validTime
                : (DateTime?)null;
        }

        private void ToggleInterlude(object obj)
        {
            Debug.WriteLine("ToggleInterlude: " + IsInterludeEnabled + " next Time: "+ _nextInterludeTime);
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
        private void InitializeScheduleChecking()
        {
            // Create a timer that fires every minute
            _scheduleCheckTimer = new DispatcherTimer();
            _scheduleCheckTimer.Interval = TimeSpan.FromMinutes(1);
            _scheduleCheckTimer.Tick += CheckActiveSchedule;
            _scheduleCheckTimer.Start();
        }

        private void CheckActiveSchedule(object sender, EventArgs e)
        {
            string playlistPath = GetPlaylistPathForCurrentTime();

            // Check if the playlist is different from the currently loaded playlist
            if (playlistPath != null && playlistPath != _currentPlaylistPath)
            {
                LoadPlaylist(playlistPath);
                _currentPlaylistPath = playlistPath; // Store the current playlist path
            }
        }

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
            var settingsWindow = new InterludeSettingsWindow(_interludeSettings);
            settingsWindow.Show();
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
            
            if (_playbackState == PlaybackState.Playing)
            {
                return;
            }

            if (!_isPlayingInterlude)
            {
                _songsPlayedSinceLastInterlude++;
            }

            _isPlayingInterlude = false;
           

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
            _lastInterludePlayedTime = DateTime.Now;

            _timer.Start();

            

            // Check if an interlude should be played
            if (_songsPlayedSinceLastInterlude >= _interludeSettings.InterludeAfterXSongs)
            {
                if (_interludeTracks.Count > 0)
                {
                    var random = new Random();
                    int index = random.Next(_interludeTracks.Count);
                    string interludeTrack = _interludeTracks[index];

                    // Stop the current playback
                    StopPlayback(null);

                    // Create a new audio player for the interlude track
                    _audioPlayer = new AudioPlayer(interludeTrack, CurrentVolume);
                    _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;

                    // Start playback of the interlude track
                    _audioPlayer.Play(NAudio.Wave.PlaybackState.Stopped, CurrentVolume);
                    _playbackState = PlaybackState.Playing;

                    // Mark the flag that an interlude is currently being played
                    _isPlayingInterlude = true;

                    _songsPlayedSinceLastInterlude = 0; // reset the counter
                }
                // Increase the counter for songs played since last interlude
                
            }
        }
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

        private void ForwardToEnd(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
                _audioPlayer.SetPosition(_audioPlayer.GetLenghtInSeconds());

                _audioPlayer.Stop();
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
