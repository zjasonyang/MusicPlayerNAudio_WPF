using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NaudioPlayer.Models;

namespace NaudioPlayer.Services
{
    public class PlaylistSaver
    {
        public void Save(ICollection<Track> playlist, string destinationFilename)
        {
            var basePath = Path.GetDirectoryName(Path.GetFullPath(destinationFilename));
            using (var sw = new StreamWriter(destinationFilename))
            {
                foreach (var track in playlist)
                {
                    var relativePath = GetRelativePath(basePath, track.Filepath);
                    sw.WriteLine(relativePath);
                }
            }
        }

        private string GetRelativePath(string fromPath, string toPath)
        {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
