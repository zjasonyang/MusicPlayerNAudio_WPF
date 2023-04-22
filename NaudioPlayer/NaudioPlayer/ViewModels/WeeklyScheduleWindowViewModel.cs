using NaudioPlayer;
using NaudioPlayer.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System;

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
        }
    }

    public ICommand AddScheduleCommand { get; set; }
    public ICommand SaveScheduleCommand { get; set; }
    public ICommand DeleteScheduleCommand { get; set; }

    public WeeklyScheduleWindowViewModel()
    {
        LoadCommands();
        // Initialize the WeeklySchedules collection with some sample data
        WeeklySchedules = new ObservableCollection<WeeklySchedule>
        {
            // Add your sample WeeklySchedule objects here
            new WeeklySchedule
            {
                Name = "Sample schedule 1",
                PlaylistPath = "Sample playlist path 1",
                StartTime = TimeSpan.Parse("18:00"),
                EndTime = TimeSpan.Parse("24:00"),
                DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday }
            }
        };
    }
    
        

    private void LoadCommands()
    {
        //AddScheduleCommand = new RelayCommand(AddSchedule, CanAddSchedule);
        SaveScheduleCommand = new RelayCommand(SaveSchedule, CanSaveSchedule);
        DeleteScheduleCommand = new RelayCommand(DeleteSchedule, CanDeleteSchedule);
    }

    private void AddSchedule(object p)
    {
        // Logic to add a new schedule
    }
    

    private void SaveSchedule(object p)
    {
        // Check if we have a SelectedWeeklySchedule
        if (SelectedWeeklySchedule == null)
        {
            // If not, create a new schedule and add it to the list
            var newSchedule = new WeeklySchedule
            {
                // Set the properties of the new schedule
            };
            WeeklySchedules.Add(newSchedule);
        }
        else
        {
            // If we have a SelectedWeeklySchedule, update its properties
            SelectedWeeklySchedule.Name = "Updated name";
            SelectedWeeklySchedule.PlaylistPath = "Updated playlist path";
            SelectedWeeklySchedule.StartTime = TimeSpan.Parse("12:00"); // Use TimeSpan.Parse to convert the string to TimeSpan
            SelectedWeeklySchedule.EndTime = TimeSpan.Parse("18:00"); // Use TimeSpan.Parse to convert the string to TimeSpan
            SelectedWeeklySchedule.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Tuesday, DayOfWeek.Thursday }; // Update the DaysOfWeek list with new DayOfWeek values
        }

    }
    private bool CanSaveSchedule(object p)
    {
        // Logic to determine if we can add a new schedule
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
        // Logic to determine if we can save the schedule
        return true;
    }

}
