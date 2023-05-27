using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace NaudioPlayer.ViewModels
{
    
    public class Genre
    {
        public string Name { get; set; }
        public ImageSource Image { get; set; }
    }

    public class MusicGenreSelectViewModel
    {
        public ObservableCollection<Genre> Genres { get; set; }



        public MusicGenreSelectViewModel()
        {
            Genres = new ObservableCollection<Genre>
            {
                new Genre { Name = "Jazz", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Jazz.png", UriKind.Absolute)) },
                new Genre { Name = "Punk", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Punk.png", UriKind.Absolute)) },
                new Genre { Name = "Rock", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Rock.png", UriKind.Absolute)) },
                new Genre { Name = "Electric", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Electric.png", UriKind.Absolute)) },
                // Other genres...
            };
        }           



        //public MusicGenreSelectViewModel()
        //{
        //    Genres = new ObservableCollection<Genre>
        //    {
        //        new Genre { Name = "Jazz", Image = new BitmapImage(new Uri("pack:application:,,,/Images/genre/Jazz.png")) },
        //        new Genre { Name = "Punk", Image = new BitmapImage(new Uri("pack:application:,,,/Images/genre/Punk.png")) },
        //        new Genre { Name = "Rock", Image = new BitmapImage(new Uri("pack:application:,,,/Images/genre/Rock.png")) },
        //        new Genre { Name = "Electric", Image = new BitmapImage(new Uri("pack:application:,,,/Images/genre/Electric.png")) }
        //        // Other genres...
        //    };
        //}
    }
}
