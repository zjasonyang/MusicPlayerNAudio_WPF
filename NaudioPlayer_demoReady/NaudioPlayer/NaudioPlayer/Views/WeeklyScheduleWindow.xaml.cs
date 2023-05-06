using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NaudioPlayer.Views
{
    public partial class WeeklyScheduleWindow : Window
    {
        public WeeklyScheduleWindow()
        {
            InitializeComponent();
            var viewModel = new WeeklyScheduleWindowViewModel();           
            DataContext = viewModel;
        }

        private void Windows_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}
