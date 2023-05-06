using NaudioPlayer;
using NaudioPlayer.Models;
using NaudioPlayer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

public class AddEditScheduleWindowViewModel : ObservableObject
{
    public string WindowTitle { get; set; }
    public WeeklySchedule WeeklySchedule { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; }
    public List<DayOfWeek> SelectedDaysOfWeek { get; set; }


    public ICommand SaveCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    public AddEditScheduleWindowViewModel(WeeklySchedule schedule)
    {
        WeeklySchedule = schedule;
        SelectedDaysOfWeek = new List<DayOfWeek>(WeeklySchedule.DaysOfWeek);
        DaysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void AddSchedule(object p)
    {
        var addEditScheduleWindow = new AddEditScheduleWindow();
        var newSchedule = new WeeklySchedule();
        addEditScheduleWindow.DataContext = new AddEditScheduleWindowViewModel(newSchedule);

        if (addEditScheduleWindow.ShowDialog() == true)
        {
            WeeklySchedules.Add(newSchedule);
        }
    }

    private void EditSchedule(object obj)
    {
        if (SelectedWeeklySchedule == null)
        {
            return;
        }

        var addEditScheduleWindow = new AddEditScheduleWindow();
        var scheduleCopy = SelectedWeeklySchedule.DeepClone(); // You'll need to implement a DeepClone method for WeeklySchedule
        addEditScheduleWindow.DataContext = new AddEditScheduleWindowViewModel(scheduleCopy);

        if (addEditScheduleWindow.ShowDialog() == true)
        {
            int index = WeeklySchedules.IndexOf(SelectedWeeklySchedule);
            WeeklySchedules[index] = scheduleCopy;
        }
    }

    private void DeleteSchedule(object obj)
    {
        if (SelectedWeeklySchedule == null)
        {
            return;
        }

        WeeklySchedules.Remove(SelectedWeeklySchedule);
    }

    private void Save(object p)
    {
        // Add logic to save the changes
        // Make sure to update WeeklySchedule.DaysOfWeek with SelectedDaysOfWeek before saving
    }

    private void Cancel(object p)
    {
        // Add logic to cancel the operation
    }
}
