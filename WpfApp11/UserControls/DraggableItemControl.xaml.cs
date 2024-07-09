﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp9
{
    public partial class DraggableItemControl : UserControl
    {
        public ItemConfiguration Configuration { get; private set; }

        public DraggableItemControl(ItemConfiguration config)
        {
            InitializeComponent();
            Configuration = config;
            UpdateUI();
            PowerToggle.Checked += (s, e) => { Configuration.IsOn = true; StatusIndicator.Fill = Brushes.Green; };
            PowerToggle.Unchecked += (s, e) => { Configuration.IsOn = false; StatusIndicator.Fill = Brushes.Red; };
        }

        private void UpdateUI()
        {
            TitleTextBlock.Text = Configuration.Name;
            IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{Configuration.DeviceType}.png"));
            StatusIndicator.Fill = Configuration.IsOn ? Brushes.Green : Brushes.Red;
            PowerToggle.IsChecked = Configuration.IsOn;
        }

        public void UpdatePowerState(bool isOn)
        {
            Configuration.IsOn = isOn;
            PowerToggle.IsChecked = isOn;
            StatusIndicator.Fill = isOn ? Brushes.Green : Brushes.Red;
        }

        private void StatusIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OverlayGrid.Visibility = OverlayGrid.Visibility == Visibility.Visible ?
                                     Visibility.Collapsed : Visibility.Visible;
        }

        private void VNC_Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle VNC button click
            MessageBox.Show("VNC button clicked");
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void FTP_Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle FTP button click
            MessageBox.Show("FTP button clicked");
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }
    }
}