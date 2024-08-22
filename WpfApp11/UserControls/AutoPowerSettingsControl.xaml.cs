using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;

namespace WpfApp9
{
    public partial class AutoPowerSettingsControl : UserControl
    {
        private const string ScheduleFile = "schedule.json";
        private List<DaySettingControl> daySettings = new List<DaySettingControl>();

        public event EventHandler CloseRequested;


        public Dictionary<string, DaySchedule> pow_schedule = new Dictionary<string, DaySchedule>();

        public AutoPowerSettingsControl()
        {
            InitializeComponent();
            PopulateComboBoxes();
            PopulateDaySettings();
            LoadSchedule();
        }



        private void PopulateComboBoxes()
        {
            for (int i = 1; i < 25; i++)
            {
                all_StartHourComboBox.Items.Add(i.ToString("D2"));
                all_EndHourComboBox.Items.Add(i.ToString("D2"));
            }

            for (int i = 0; i < 60; i += 30)
            {
                all_StartMinuteComboBox.Items.Add(i.ToString("D2"));
                all_EndMinuteComboBox.Items.Add(i.ToString("D2"));
            }
        }

        private void allapply_click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (all_StartHourComboBox.SelectedIndex != -1 && all_StartMinuteComboBox.SelectedIndex != -1 && all_EndHourComboBox.SelectedIndex != -1 && all_EndMinuteComboBox.SelectedIndex != -1)
            {
                for (int i = 0; i < daySettings.Count; i++)
                {
                    daySettings[i].StartHourComboBox.SelectedIndex = all_StartHourComboBox.SelectedIndex;
                    daySettings[i].StartMinuteComboBox.SelectedIndex = all_StartMinuteComboBox.SelectedIndex;
                    daySettings[i].EndHourComboBox.SelectedIndex = all_EndHourComboBox.SelectedIndex;
                    daySettings[i].EndMinuteComboBox.SelectedIndex = all_EndMinuteComboBox.SelectedIndex;

                    daySettings[i].DayCheckBox.IsChecked = true;
                }
            }
            else
            {
                MessageBox.Show("모든 시간을 입력하세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            

        }

        private void PopulateDaySettings()
        {
            //string[] days = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };
            //foreach (var day in days)
            //{
            //    var daySettingControl = new DaySettingControl(day);
            //    DaySettingsPanel.Children.Add(daySettingControl);
            //    daySettings.Add(daySettingControl);
            //}

            w1.settingControl("월요일");
            daySettings.Add(w1);

            w2.settingControl("화요일");
            daySettings.Add(w2);

            w3.settingControl("수요일");
            daySettings.Add(w3);

            w4.settingControl("목요일");
            daySettings.Add(w4);

            w5.settingControl("금요일");
            daySettings.Add(w5);

            w6.settingControl("토요일");
            daySettings.Add(w6);

            w7.settingControl("일요일");
            daySettings.Add(w7);

        }

        private void LoadSchedule()
        {
            if (File.Exists(ScheduleFile))
            {
                string json = File.ReadAllText(ScheduleFile);
                var schedule = JsonConvert.DeserializeObject<Dictionary<string, DaySchedule>>(json);
                pow_schedule = schedule;
                foreach (var daySetting in daySettings)
                {
                    if (schedule.TryGetValue(daySetting.Day, out var daySchedule))
                    {
                        daySetting.SetSchedule(daySchedule);
                    }
                }
            }
            else
            {

                var schedule = new Dictionary<string, DaySchedule>();


                DaySchedule d = new DaySchedule();
                d.StartTime = TimeSpan.Parse("01:00");
                d.EndTime = TimeSpan.Parse("03:00");

                schedule.Add("월요일", d);
                schedule.Add("화요일", d);
                schedule.Add("수요일", d);
                schedule.Add("목요일", d);
                schedule.Add("금요일", d);
                schedule.Add("토요일", d);
                schedule.Add("일요일", d);

                pow_schedule = schedule;
                string json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
                File.WriteAllText(ScheduleFile, json);



                 string json2 = File.ReadAllText(ScheduleFile);
                var schedule2 = JsonConvert.DeserializeObject<Dictionary<string, DaySchedule>>(json2);
                pow_schedule = schedule2;
                foreach (var daySetting in daySettings)
                {
                    if (schedule2.TryGetValue(daySetting.Day, out var daySchedule))
                    {
                        daySetting.SetSchedule(daySchedule);
                    }
                }






            }
        }

        public void init()
        {
            //if (!File.Exists(ScheduleFile))
            //{
            //    var schedule = new Dictionary<string, DaySchedule>();


            //    DaySchedule d = new DaySchedule();
            //    d.StartTime = TimeSpan.Parse("01:00");
            //    d.EndTime = TimeSpan.Parse("03:00");

            //    schedule.Add("월요일", d);
            //    schedule.Add("화요일", d);
            //    schedule.Add("수요일", d);
            //    schedule.Add("목요일", d);
            //    schedule.Add("금요일", d);
            //    schedule.Add("토요일", d);
            //    schedule.Add("일요일", d);

            //    pow_schedule = schedule;
            //    string json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            //    File.WriteAllText(ScheduleFile, json);
            //}
        }


        private void SaveSchedule()
        {
            var schedule = new Dictionary<string, DaySchedule>();
            
            foreach (var daySetting in daySettings)
            {
                schedule[daySetting.Day] = daySetting.GetSchedule();
            }
          

            //daySettings[0].Day = "월요일";
            //daySettings[0].
            //schedule.Add

            pow_schedule = schedule;
            string json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            File.WriteAllText(ScheduleFile, json);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSchedule();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public void cancle_ev()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DaySchedule
    {
        public bool IsEnabled { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}