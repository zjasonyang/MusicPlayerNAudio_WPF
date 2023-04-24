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

public class WeeklyScheduleWindowViewModel : ObservableObject
{
    private ObservableCollection<WeeklySchedule> _weeklySchedules;
    private WeeklySchedule _selectedWeeklySchedule;

    public ObservableCollection<WeeklySchedule> WeeklySchedules
    {
        get { return _weeklySchedules; }
        set
        {
            _weeklySchedules = value;
            OnPropertyChanged();
        }
    }
    public WeeklySchedule SelectedWeeklySchedule
    {
        get { return _selectedWeeklySchedule; }
        set
        {
            _selectedWeeklySchedule = value;
            OnPropertyChanged();
            Console.WriteLine($"SelectedWeeklySchedule: {SelectedWeeklySchedule}");
        }
    }

    public ObservableCollection<string> AvailablePlaylists { get; set; }
    public ObservableCollection<string> AvailableTimes { get; set; }


    public ICommand AddScheduleCommand { get; set; }
    public ICommand SaveScheduleCommand { get; set; }
    public ICommand DeleteScheduleCommand { get; set; }

    public ICommand SaveScheduleToJsonCommand { get; set; }

    public ObservableCollection<SelectableDay> SelectedDaysOfWeek
    {
        get
        {
            return new ObservableCollection<SelectableDay>(SelectedWeeklySchedule.DaysOfWeek.Select(d => new SelectableDay { Day = d, IsSelected = true }));
        }
        set
        {
            SelectedWeeklySchedule.DaysOfWeek = value.Where(x => x.IsSelected).Select(x => x.Day).ToList();
            OnPropertyChanged(nameof(SelectedDaysOfWeek));
        }
    }

    public WeeklyScheduleWindowViewModel()
    {
        LoadCommands();
        // Initialize the WeeklySchedules collection with some sample data

        //WeeklySchedule defaultSchedule = LoadScheduleFromJson();
        //if(defaultSchedule == null) 
        //{
        //    defaultSchedule = new WeeklySchedule
        //    {
        //        Name = "Default",
        //        PlaylistPath = "defaultPath",
        //        StartTime = TimeSpan.Parse("08:00"),
        //        EndTime = TimeSpan.Parse("16:00"),
        //        DaysOfWeek = { DayOfWeek.Monday, DayOfWeek.Wednesday }
        //    };
        //}       
    }

    private void LoadCommands()
    {
        AddScheduleCommand = new RelayCommand(AddSchedule, CanAddSchedule);
        SaveScheduleCommand = new RelayCommand(SaveSchedule, CanSaveSchedule);
        DeleteScheduleCommand = new RelayCommand(DeleteSchedule, CanDeleteSchedule);

        SaveScheduleToJsonCommand = new RelayCommand(SaveScheduleToJson, CanSaveScheduleToJson);
        
    }


    private void SaveScheduleToJson(object p)
    {
        if (WeeklySchedules != null)
        {
            string json = JsonConvert.SerializeObject(WeeklySchedules, Formatting.Indented);
            File.WriteAllText("weeklySchedules.json", json);
        }
    }
    private bool CanSaveScheduleToJson(object p)
    {
        return true;
    }

    private WeeklySchedule LoadScheduleFromJson()
    {
        if (File.Exists("schedule.json"))
        {
            string json = File.ReadAllText("schedule.json");
            return JsonConvert.DeserializeObject<WeeklySchedule>(json);
        }
        return null;
    }

    private void AddSchedule(object p)
    {
        // Logic to add a new schedule
    }
    private bool CanAddSchedule(object p)
    {
        return true;
    }

    private void SaveSchedule(object p)
    {
        WeeklySchedules.Add(SelectedWeeklySchedule);
        
    }
    private bool CanSaveSchedule(object p)
    {
        return true;
    }

    private void DeleteSchedule(object p)
    {
        // Logic to delete the selected schedule
        if (SelectedWeeklySchedule != null)
        {
            WeeklySchedules.Remove(SelectedWeeklySchedule);
        }
    }
    private bool CanDeleteSchedule(object p)
    {
        return SelectedWeeklySchedule != null;
    }


}
