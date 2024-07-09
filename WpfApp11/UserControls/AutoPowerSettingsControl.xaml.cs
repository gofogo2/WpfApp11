﻿using System;
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

        public AutoPowerSettingsControl()
        {
            InitializeComponent();
            PopulateDaySettings();
            LoadSchedule();
        }

        private void PopulateDaySettings()
        {
            string[] days = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };
            foreach (var day in days)
            {
                var daySettingControl = new DaySettingControl(day);
                DaySettingsPanel.Children.Add(daySettingControl);
                daySettings.Add(daySettingControl);
            }
        }

        private void LoadSchedule()
        {
            if (File.Exists(ScheduleFile))
            {
                string json = File.ReadAllText(ScheduleFile);
                var schedule = JsonConvert.DeserializeObject<Dictionary<string, DaySchedule>>(json);
                foreach (var daySetting in daySettings)
                {
                    if (schedule.TryGetValue(daySetting.Day, out var daySchedule))
                    {
                        daySetting.SetSchedule(daySchedule);
                    }
                }
            }
        }

        private void SaveSchedule()
        {
            var schedule = new Dictionary<string, DaySchedule>();
            foreach (var daySetting in daySettings)
            {
                schedule[daySetting.Day] = daySetting.GetSchedule();
            }
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
    }

    public class DaySchedule
    {
        public bool IsEnabled { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}