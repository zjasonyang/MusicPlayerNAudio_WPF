using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NaudioPlayer.Models;
using NaudioPlayer.Extensions;
using NAudio.Wave;

namespace NaudioPlayer.Services
{
    public class PlaylistLoader
    {
        public ICollection<Track> Load(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                var tracks = new Collection<Track>();
                int trackNumber = 1;

                var line = sr.ReadLine();
                do
                {
                    var filepath = line;
                    var friendlyName = filepath.RemovePath().Remove(filepath.RemovePath().Length - 4);
                    
                    using (var reader = new AudioFileReader(filepath))
                    {
                        var duration = reader.TotalTime;
                        tracks.Add(new Track(filepath, friendlyName, trackNumber, duration));
                    }

                    line = sr.ReadLine();

                    //tracks.Add(new Track(line,line.RemovePath().Remove(line.RemovePath().Length - 4)));
                    trackNumber++;
                } while (line != null);

                return tracks;
            }
        }
    }
}
