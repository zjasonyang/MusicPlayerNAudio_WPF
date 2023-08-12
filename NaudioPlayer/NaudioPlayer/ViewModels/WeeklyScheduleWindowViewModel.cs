using NaudioPlayer;
using NaudioPlayer.Models;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

public class WeeklyScheduleWindowViewModel : ObservableObject
{
     
    private string _saveStatusMessage;
    public string SaveStatusMessage
    {
        get { return _saveStatusMessage; }
        set
        {
            _saveStatusMessage = value;
            OnPropertyChanged();
        }
    }
    private bool _isPopupOpen;
    public bool IsPopupOpen
    {
        get { return _isPopupOpen; }
        set
        {
            _isPopupOpen = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<WeeklySchedule> _weeklySchedules;
    public ObservableCollection<WeeklySchedule> WeeklySchedules
    {
        get { return _weeklySchedules; }
        set
        {
            _weeklySchedules = value;
            OnPropertyChanged();
        }
    }

    private WeeklySchedule _selectedWeeklySchedule;
    public WeeklySchedule SelectedWeeklySchedule
    {
        get { return _selectedWeeklySchedule; }
        set
        {
            _selectedWeeklySchedule = value;
            OnPropertyChanged();
            UpdateSelectedDays();
            Console.WriteLine($"SelectedWeeklySchedule: {SelectedWeeklySchedule}");
        }
    }

    private ObservableCollection<SelectableDay> _selectedDays;
    public ObservableCollection<SelectableDay> SelectedDays
    {
        get { return _selectedDays; }
        set
        {
            _selectedDays = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> AvailablePlaylists { get; set; }
    public ObservableCollection<string> AvailableTimes { get; set; }
    
    //用來顯示 days of week 
    public ObservableCollection<DayOfWeek> AllDaysOfWeek { get; set; }
    public IList<DayOfWeek> ListBoxSelectedDays { get; set; }

    public ICommand AddScheduleCommand { get; set; }
    public ICommand SaveScheduleCommand { get; set; }
    public ICommand DeleteScheduleCommand { get; set; }

    public ICommand SaveScheduleToJsonCommand { get; set; }

    public ICommand CloseWindowCommand { get; private set; }

    public WeeklyScheduleWindowViewModel()
    {
        LoadAvailablePlaylists();

        WeeklySchedules = LoadScheduleFromJson();

        if (WeeklySchedules == null || WeeklySchedules.Count == 0)
        {
            WeeklySchedule defaultSchedule = new WeeklySchedule();

            WeeklySchedules = new ObservableCollection<WeeklySchedule> { defaultSchedule };
            SaveScheduleToJson(WeeklySchedules); 
        }

        
        LoadCommands();
    }

    private void LoadCommands()
    {
        AddScheduleCommand = new RelayCommand(AddSchedule, CanAddSchedule);
        SaveScheduleCommand = new RelayCommand(SaveSchedule, CanSaveSchedule);
        DeleteScheduleCommand = new RelayCommand(DeleteSchedule, CanDeleteSchedule);
        CloseWindowCommand = new RelayCommand(CloseWindow, _ => true);
    }
    private void UpdateSelectedDays()
    {
        if (SelectedWeeklySchedule != null)
        {
            SelectedDays = new ObservableCollection<SelectableDay>
            {
                new SelectableDay { Day = DayOfWeek.Monday, IsSelected = SelectedWeeklySchedule.MondayIsSelected },
                new SelectableDay { Day = DayOfWeek.Tuesday, IsSelected = SelectedWeeklySchedule.TuesdayIsSelected },
                new SelectableDay { Day = DayOfWeek.Wednesday, IsSelected = SelectedWeeklySchedule.WednesdayIsSelected },
                new SelectableDay { Day = DayOfWeek.Thursday, IsSelected = SelectedWeeklySchedule.ThursdayIsSelected },
                new SelectableDay { Day = DayOfWeek.Friday, IsSelected = SelectedWeeklySchedule.FridayIsSelected },
                new SelectableDay { Day = DayOfWeek.Saturday, IsSelected = SelectedWeeklySchedule.SaturdayIsSelected },
                new SelectableDay { Day = DayOfWeek.Sunday, IsSelected = SelectedWeeklySchedule.SundayIsSelected }
            };
        }
    }


    private void LoadAvailablePlaylists()
    {
        AvailablePlaylists = new ObservableCollection<string>();

        // #手動設定 playlist 資料夾路徑
        string playlistFolderPath = "./_playlist"; 
        if (Directory.Exists(playlistFolderPath))
        {
            var playlistFiles = Directory.EnumerateFiles(playlistFolderPath, "*.playlist");

            foreach (var playlistFile in playlistFiles)
            {
                string playlistName = Path.GetFileNameWithoutExtension(playlistFile);
                AvailablePlaylists.Add(playlistName);
            }
        }
    }

    private async void SaveScheduleToJson(ObservableCollection<WeeklySchedule> schedules)
    {
        if (schedules != null)
        {
            string json = JsonConvert.SerializeObject(schedules, Formatting.Indented);
            File.WriteAllText("weeklySchedules.json", json);
            SaveStatusMessage = "Save successful";
            IsPopupOpen = true;
            await Task.Delay(3000);
            IsPopupOpen = false;
        }
        else
        {
            SaveStatusMessage = "Save failed";
            IsPopupOpen = true;
            await Task.Delay(3000);
            IsPopupOpen = false;
        }
    }
    private bool CanSaveScheduleToJson()
    {
        return true;
    }

    private void CloseWindow(object obj)
    {
        if (obj is Window window)
        {
            window.Close();
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

    private void AddSchedule(object p)
    {
        var newSchedule = new WeeklySchedule();
        WeeklySchedules.Add(newSchedule);
        SelectedWeeklySchedule = newSchedule;
    }
    private bool CanAddSchedule(object p)
    {
        return true;
    }

    private void SaveSchedule(object p)
    {
        SaveScheduleToJson(WeeklySchedules);
    }
    private bool CanSaveSchedule(object p)
    {
        return true;
    }

    private void DeleteSchedule(object p)
    {
        if (SelectedWeeklySchedule != null)
        {
            WeeklySchedules.Remove(SelectedWeeklySchedule);
            SelectedWeeklySchedule = null;
            SaveScheduleToJson(WeeklySchedules);
        }
    }
    private bool CanDeleteSchedule(object p)
    {
        return SelectedWeeklySchedule != null;
    }
}
