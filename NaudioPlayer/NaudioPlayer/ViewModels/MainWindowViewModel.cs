using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NaudioPlayer.Annotations;
using NaudioPlayer.Models;
using NaudioPlayer.Services;
using NaudioWrapper;
using NaudioPlayer.Extensions;
using System.Timers;
using System.Windows.Threading;
using System;
using NAudio.Wave;

namespace NaudioPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private enum PlaybackState
        {
            Playing, Stopped, Paused
        }

        private PlaybackState _playbackState;

        private ObservableCollection<Track> _playlist;
        

        private Track _currentlyPlayingTrack;
        private Track _currentlySelectedTrack;
        private AudioPlayer _audioPlayer;
        private System.Timers.Timer _timer;

        private string _title;
        private double _currentTrackLenght;
        private double _currentTrackPosition;
        private string _playPauseImageSource;
        private float _currentVolume;


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




        public ICommand ExitApplicationCommand { get; set; }
        public ICommand AddFileToPlaylistCommand { get; set; }
        public ICommand AddFolderToPlaylistCommand { get; set; }
        public ICommand SavePlaylistCommand { get; set; }
        public ICommand LoadPlaylistCommand { get; set; }

        public ICommand RewindToStartCommand { get; set; }
        public ICommand StartPlaybackCommand { get; set; }
        public ICommand StopPlaybackCommand { get; set; }
        public ICommand ForwardToEndCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }

        public ICommand TrackControlMouseDownCommand { get; set; }
        public ICommand TrackControlMouseUpCommand { get; set; }
        public ICommand VolumeControlValueChangedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Application.Current.MainWindow.Closing += MainWindow_Closing;

            Title = "NaudioPlayer";

            LoadCommands();

            Playlist = new ObservableCollection<Track>();
            
            _playbackState = PlaybackState.Stopped;

            PlayPauseImageSource = "../Images/play.png";
            CurrentVolume = 0.5F;

            //var timer = new System.Timers.Timer();
            //timer.Interval = 300;
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();

            _timer = new System.Timers.Timer(1000); // 1000 milliseconds or 1 second interval
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;

        }

        private void UpdateSeekBar()
        {
            if (_playbackState == PlaybackState.Playing)
            {
                CurrentTrackPosition = _audioPlayer.GetPositionInSeconds();
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_audioPlayer != null)
            {
                double newPosition = _audioPlayer.GetPositionInSeconds();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTrackPosition = newPosition;
                    Console.WriteLine($"Timer tick at {DateTime.Now}, position: {CurrentTrackPosition}");
                });
            }
        }

        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    if (_audioPlayer != null)
        //    {
        //        CurrentTrackPosition = _audioPlayer.GetPositionInSeconds();
        //        Console.WriteLine($"Timer tick at {DateTime.Now}, position: {CurrentTrackPosition}");
        //    }
        //}


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
            PlayPauseImageSource = "../Images/play.png";
            CommandManager.InvalidateRequerySuggested();
            CurrentTrackPosition = 0;
            
            if (_audioPlayer.PlaybackStopType == AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile)
            {
                CurrentlySelectedTrack = Playlist.NextItem(CurrentlyPlayingTrack);
                StartPlayback(null);
            }
            else if (_audioPlayer.PlaybackStopType == AudioPlayer.PlaybackStopTypes.PlaybackStoppedByUser)
            {
                if (CurrentlySelectedTrack != CurrentlyPlayingTrack)
                {
                    StartPlayback(null);
                }
            }
        }

        private void _audioPlayer_PlaybackResumed()
        {
            _playbackState = PlaybackState.Playing;
            PlayPauseImageSource = "../Images/pause.png";
        }

        private void _audioPlayer_PlaybackPaused()
        {
            _playbackState = PlaybackState.Paused;
            PlayPauseImageSource = "../Images/play.png";
        }

        private void LoadCommands()
        {
            // Menu commands
            ExitApplicationCommand = new RelayCommand(ExitApplication,CanExitApplication);
            AddFileToPlaylistCommand = new RelayCommand(AddFileToPlaylist, CanAddFileToPlaylist);
            AddFolderToPlaylistCommand = new RelayCommand(AddFolderToPlaylist, CanAddFolderToPlaylist);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, CanSavePlaylist);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist, CanLoadPlaylist);

            // Player commands
            RewindToStartCommand = new RelayCommand(RewindToStart, CanRewindToStart);
            StartPlaybackCommand = new RelayCommand(StartPlayback, CanStartPlayback);
            StopPlaybackCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            ForwardToEndCommand = new RelayCommand(ForwardToEnd, CanForwardToEnd);
            ShuffleCommand = new RelayCommand(Shuffle, CanShuffle);

            // Event commands
            TrackControlMouseDownCommand = new RelayCommand(TrackControlMouseDown, CanTrackControlMouseDown);
            TrackControlMouseUpCommand = new RelayCommand(TrackControlMouseUp, CanTrackControlMouseUp);
            VolumeControlValueChangedCommand = new RelayCommand(VolumeControlValueChanged, CanVolumeControlValueChanged);
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
        private bool CanExitApplication(object p)
        {
            return true;
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
                Playlist = new ObservableCollection<Track>(Playlist.OrderBy(z => z.FriendlyName).ToList());
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

        private bool CanSavePlaylist(object p)
        {
            return true;
        }

        private void LoadPlaylist(object p)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "PLAYLIST files (*.playlist) | *.playlist";
            if (ofd.ShowDialog() == true)
            {
                //Playlist = new PlaylistLoader().Load(ofd.FileName).ToObservableCollection();
                Playlist = ToObservableCollection(new PlaylistLoader().Load(ofd.FileName));
            }
        }

        private ObservableCollection<T> ToObservableCollection<T>(ICollection<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        private bool CanLoadPlaylist(object p)
        {
            return true;
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
            if (CurrentlySelectedTrack != null)
            {
                // If we are selecting the clip that is playing, just do play/pause otherwise create a new AudioPlayer to play another clip
                if (CurrentlyPlayingTrack != CurrentlySelectedTrack)
                {
                    if (CurrentlyPlayingTrack == null || _playbackState == PlaybackState.Stopped)
                    {
                        _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
                        _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
                        _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
                        _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
                        _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;
                        CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
                        CurrentlyPlayingTrack = CurrentlySelectedTrack;
                    }
                    else
                    {
                        if (_audioPlayer != null)
                        {
                            StopPlayback(null);
                            // This is here to stop stuttering of audio while one clip ends other begins
                            Thread.Sleep(500);
                        }
                    }
                }
                if (_playbackState == PlaybackState.Stopped)
                {
                    _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
                    _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
                    _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
                    _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
                    _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;
                    CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
                    CurrentlyPlayingTrack = CurrentlySelectedTrack;
                }
                _audioPlayer.TogglePlayPause(CurrentVolume);
                _timer.Start(); // Start updating the position
            }
        }

        //private void StartPlayback(object p)
        //{
        //    if (CurrentlySelectedTrack != null)
        //    {
        //        if (_playbackState == PlaybackState.Stopped)
        //        {
        //            _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
        //            _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
        //            _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
        //            _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
        //            _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;
        //            CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
        //            CurrentlyPlayingTrack = CurrentlySelectedTrack;
        //        }
        //        if (CurrentlySelectedTrack == CurrentlyPlayingTrack)
        //        {
        //            _audioPlayer.TogglePlayPause(CurrentVolume);
        //        }
        //    }
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
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackStopType = AudioPlayer.PlaybackStopTypes.PlaybackStoppedByUser;
                _audioPlayer.Stop();
                _timer.Stop(); // Stop updating the position
            }
        }
        private bool CanStopPlayback(object p)
        {
            if (_playbackState == PlaybackState.Playing || _playbackState == PlaybackState.Paused)
            {
                return true;
            }
            return false;
        }

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
            if (_audioPlayer != null)
            {
                _audioPlayer.Pause();
            }
        }

        private void TrackControlMouseUp(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.SetPosition(CurrentTrackPosition);
                _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);
            }
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
            if (_playbackState == PlaybackState.Paused)
            {
                return true;
            }
            return false;
        }

        private void VolumeControlValueChanged(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.SetVolume(CurrentVolume);
            }
        }

        private bool CanVolumeControlValueChanged(object p)
        {
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
