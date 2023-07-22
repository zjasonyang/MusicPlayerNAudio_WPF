using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NaudioPlayer.Models;
using NaudioPlayer.Extensions;
using NAudio.Wave;
using System.Diagnostics;

namespace NaudioPlayer.Services
{
    public class PlaylistLoader
    {
        public ICollection<Track> Load(string filePath)
        {
            string basePath = Path.GetDirectoryName(filePath);
            basePath = Path.GetFullPath(Path.Combine(basePath, "../../"));


            using (var sr = new StreamReader(filePath))
            {
                var tracks = new Collection<Track>();
                int trackNumber = 1;

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue; // Skip empty lines
                    }

                    // Combine the base path with the relative path to get the absolute path
                    var absoluteFilePath = Path.Combine(basePath, "resources", line);

                    var friendlyName = Path.GetFileNameWithoutExtension(line);

                    using (var reader = new AudioFileReader(absoluteFilePath))
                    {
                        var duration = reader.TotalTime;
                        tracks.Add(new Track(absoluteFilePath, friendlyName, trackNumber, duration));
                    }

                    trackNumber++;
                }

                return tracks;
            }
        }
    

    //public ICollection<Track> Load(string filePath)
    //{
    //    using (var sr = new StreamReader(filePath))
    //    {
    //        var tracks = new Collection<Track>();
    //        int trackNumber = 1;

    //        var line = sr.ReadLine();
    //        do
    //        {
    //            var filepath = line;
    //            var friendlyName = filepath.RemovePath().Remove(filepath.RemovePath().Length - 4);

    //            using (var reader = new AudioFileReader(filepath))
    //            {
    //                var duration = reader.TotalTime;
    //                tracks.Add(new Track(filepath, friendlyName, trackNumber, duration));
    //            }

    //            line = sr.ReadLine();

    //            //tracks.Add(new Track(line,line.RemovePath().Remove(line.RemovePath().Length - 4)));
    //            trackNumber++;
    //        } while (line != null);

    //        return tracks;
    //    }
    //}
    }
}
