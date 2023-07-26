using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace NaudioWrapper
{
    public class AudioPlayer
    {
        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        public PlaybackStopTypes PlaybackStopType { get; set; }

        private AudioFileReader _audioFileReader;

        private DirectSoundOut _output;

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;

        //public AudioPlayer(string filepath, float volume)
        //{
        //    PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;

        //    _audioFileReader = new AudioFileReader(filepath) { Volume = volume };

        //    _output = new DirectSoundOut(200);
        //    _output.PlaybackStopped += _output_PlaybackStopped;

        //    var wc = new WaveChannel32(_audioFileReader);
        //    wc.PadWithZeroes = false;

        //    _output.Init(wc);
        //}

        //修改為相對路徑
        public AudioPlayer(string filepath, float volume)
        {
            PlaybackStopType = PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;

            if (Path.IsPathRooted(filepath))
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                Uri baseUri = new Uri(basePath);
                Uri fullUri = new Uri(filepath);
                Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
                filepath = Uri.UnescapeDataString(relativeUri.ToString());
            }

            _audioFileReader = new AudioFileReader(filepath) { Volume = volume };

            _output = new DirectSoundOut(200);
            _output.PlaybackStopped += _output_PlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;

            _output.Init(wc);
        }

        public void Play(PlaybackState playbackState, double currentVolumeLevel)
        {
            Debug.WriteLine("Play() method called");
            if (_output == null || _audioFileReader == null) return;

            if (playbackState == PlaybackState.Stopped || playbackState == PlaybackState.Paused)
            {
                Debug.WriteLine("Starting playback...");
                _output.Play();
            }

            _audioFileReader.Volume = (float) currentVolumeLevel;
            
            if (PlaybackResumed != null)
            {
                Debug.WriteLine("Raising PlaybackResumed event...");
                PlaybackResumed();
            }
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("_output_PlaybackStopped event handler called");
            Dispose();
            if (PlaybackStopped != null)
            {
                Debug.WriteLine("Raising PlaybackStopped event...");
                PlaybackStopped();
            }
        }

        public void Stop()
        {
            Debug.WriteLine("Stop() method called");
            if (_output != null)
            {
                _output.Stop();
            }
        }

        public void Pause()
        {
            Debug.WriteLine("Pause() method called");
            if (_output != null)
            {
                _output.Pause();

                if (PlaybackPaused != null)
                {
                    Debug.WriteLine("Raising PlaybackPaused event...");
                    PlaybackPaused();
                }
            }
        }

        public void TogglePlayPause(double currentVolumeLevel)
        {
            Debug.WriteLine("TogglePlayPause() method called");
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    Debug.WriteLine("Pausing playback...");
                    Pause();
                }
                else
                {
                    Debug.WriteLine("Resuming playback...");
                    Play(_output.PlaybackState, currentVolumeLevel);
                }
            }
            else
            {
                Debug.WriteLine("Starting playback...");
                Play(PlaybackState.Stopped, currentVolumeLevel);
            }
        }

        public void Dispose()
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    _output.Stop();
                }

                _output.Dispose();
                _output = null;
            }
            if (_audioFileReader != null)
            {
                //_audioFileReader.Dispose();
                //_audioFileReader = null;
                try
                {
                    _audioFileReader.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception when disposing _audioFileReader: " + ex.ToString());
                }
                _audioFileReader = null;
            }
        }

        public double GetLenghtInSeconds()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.TotalTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }

        public double GetPositionInSeconds()
        {
            return _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds : 0;
        }

        public float GetVolume()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.Volume;
            }
            return 1;
        }

        public void SetPosition(double value)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
            }
        }

        public void SetVolume(float value)
        {
            if (_output != null)
            {
                _audioFileReader.Volume = value;
            }
        }

        public void VolumeFade(float targetVolume, int fadeDurationMilliseconds)
        {
            // 我們需要使用一個新的線程來進行淡出操作，以避免阻塞主線程
            new Thread(() =>
            {
                float startVolume = _audioFileReader.Volume;

                float deltaVolume = targetVolume - startVolume;
                int fadeSteps = fadeDurationMilliseconds / 50; // 我們每 50ms 調整一次音量
                float volumeStep = deltaVolume / fadeSteps;

                for (int i = 0; i < fadeSteps; i++)
                {
                    _audioFileReader.Volume += volumeStep;
                    Thread.Sleep(50);
                }

                _audioFileReader.Volume = targetVolume; // 確保最後的音量達到目標音量

            }).Start();
        }
    }
}
