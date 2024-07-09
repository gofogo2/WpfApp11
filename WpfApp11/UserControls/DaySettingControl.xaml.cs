using System;
using System.Windows.Controls;

namespace WpfApp9
{
    public partial class DaySettingControl : UserControl
    {
        public string Day { get; private set; }

        public DaySettingControl(string day)
        {
            InitializeComponent();
            Day = day;
            DayCheckBox.Content = day;
            PopulateComboBoxes();
        }

        private void PopulateComboBoxes()
        {
            for (int i = 0; i < 24; i++)
            {
                StartHourComboBox.Items.Add(i.ToString("D2"));
                EndHourComboBox.Items.Add(i.ToString("D2"));
            }

            for (int i = 0; i < 60; i += 30)
            {
                StartMinuteComboBox.Items.Add(i.ToString("D2"));
                EndMinuteComboBox.Items.Add(i.ToString("D2"));
            }
        }

        public void SetSchedule(DaySchedule schedule)
        {
            DayCheckBox.IsChecked = schedule.IsEnabled;
            StartHourComboBox.SelectedItem = schedule.StartTime.Hours.ToString("D2");
            StartMinuteComboBox.SelectedItem = schedule.StartTime.Minutes.ToString("D2");
            EndHourComboBox.SelectedItem = schedule.EndTime.Hours.ToString("D2");
            EndMinuteComboBox.SelectedItem = schedule.EndTime.Minutes.ToString("D2");
        }

        public DaySchedule GetSchedule()
        {
            return new DaySchedule
            {
                IsEnabled = DayCheckBox.IsChecked ?? false,
                StartTime = new TimeSpan(int.Parse(StartHourComboBox.SelectedItem as string), int.Parse(StartMinuteComboBox.SelectedItem as string), 0),
                EndTime = new TimeSpan(int.Parse(EndHourComboBox.SelectedItem as string), int.Parse(EndMinuteComboBox.SelectedItem as string), 0)
            };
        }
    }
}