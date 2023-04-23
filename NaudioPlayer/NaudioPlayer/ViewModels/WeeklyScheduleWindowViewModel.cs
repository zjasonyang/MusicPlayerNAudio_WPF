using NaudioPlayer;
using NaudioPlayer.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

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
                StartTime = "Sample start time 1",
                EndTime = "Sample end time 1",
                DaysOfWeek = "Sample days of week 1"
            }
        };
    }

    private void LoadCommands()
    {
        AddScheduleCommand = new RelayCommand(AddSchedule);
        SaveScheduleCommand = new RelayCommand(SaveSchedule);
        DeleteScheduleCommand = new RelayCommand(DeleteSchedule);
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
            SelectedWeeklySchedule.StartTime = "Updated start time";
            SelectedWeeklySchedule.EndTime = "Updated end time";
            SelectedWeeklySchedule.DaysOfWeek = "Updated days of week";
        }
    }

    private void DeleteSchedule(object p)
    {
        // Logic to delete the selected schedule
        if (SelectedWeeklySchedule != null)
        {
            WeeklySchedules.Remove(SelectedWeeklySchedule);
        }
    }
}
