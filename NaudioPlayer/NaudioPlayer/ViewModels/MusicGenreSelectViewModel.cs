using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NaudioPlayer.Annotations;
using NaudioPlayer.Extensions;
using NaudioPlayer.Models;
using NaudioPlayer.Services;
using NaudioPlayer.Views;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using NaudioPlayer.Services;

namespace NaudioPlayer.ViewModels
{
    
    public class Genre
    {
        public string Name { get; set; }
        public ImageSource Image { get; set; }
        public string PlayListPath { get; set; }
    }

    public class MusicGenreSelectViewModel: INotifyPropertyChanged
    {
        private ObservableCollection<Genre> _genres;
        private int _currentPage = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        //public ICommand LoadPlaylistCommand => MainWindowViewModel.Instance.LoadPlaylistCommand;
        public ICommand LoadPlaylistCommand => ((MainWindowViewModel)Application.Current.MainWindow.DataContext).LoadPlaylistCommand;
        //public ObservableCollection<Track> Playlist => MainWindowViewModel.Instance.Playlist;

        public ObservableCollection<Genre> Genres
        {
            get => new ObservableCollection<Genre>(_genres.Skip(_currentPage * 4).Take(4));
            set
            {
                _genres = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsScrollButtonVisible));
            }
        }

        public Visibility IsScrollButtonVisible =>
            _genres != null && _genres.Count > (_currentPage + 1) * 4 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsBackScrollButtonVisible =>
            _currentPage > 0 ? Visibility.Visible : Visibility.Collapsed;


        public ICommand ScrollToNextPageCommand => new RelayCommand(
            execute: _ =>
            {
                if (_genres.Count > (_currentPage + 1) * 4)
                {
                    _currentPage++;
                    OnPropertyChanged(nameof(Genres));
                    OnPropertyChanged(nameof(IsScrollButtonVisible));
                    OnPropertyChanged(nameof(IsBackScrollButtonVisible));
                }
            },
            canExecute: _ => _genres.Count > (_currentPage + 1) * 4
        );

        public ICommand ScrollToPreviousPageCommand => new RelayCommand(
            execute: _ =>
            {
                if (_currentPage > 0)
                {
                    _currentPage--;
                    OnPropertyChanged(nameof(Genres));
                    OnPropertyChanged(nameof(IsScrollButtonVisible));
                    OnPropertyChanged(nameof(IsBackScrollButtonVisible));
                }
            },
            canExecute: _ => _currentPage > 0
        );

       

        public MusicGenreSelectViewModel()
        {
            Genres = new ObservableCollection<Genre>
            {
                new Genre { Name = "Jazz", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Jazz.png", UriKind.Absolute)), 
                            PlayListPath = @"C:\test3.playlist" },
                            
                new Genre { Name = "Punk", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Punk.png", UriKind.Absolute)) },
                new Genre { Name = "Rock", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Rock.png", UriKind.Absolute)) },
                new Genre { Name = "Electric", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Electric.png", UriKind.Absolute)) },
                new Genre { Name = "Pop", Image = new BitmapImage(new Uri(@"C:\Users\Jason Yang\source\repos\NaudioPlayer\NaudioPlayer\NaudioPlayer\Images\genre\Pop.png", UriKind.Absolute)) },
                // Other genres...
            };
        }

        //private ObservableCollection<T> ToObservableCollection<T>(ICollection<T> collection)
        //{
        //    return new ObservableCollection<T>(collection);
        //}

        // Play
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
