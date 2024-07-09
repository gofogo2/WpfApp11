using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp11.Helpers;

namespace WpfApp9
{
    public partial class DraggableItemControl : UserControl
    {
        string vncViewerPath = @"C:\Program Files\TightVNC\tvnviewer.exe"; // TightVNC 뷰어 경로
        public ItemConfiguration Configuration { get; private set; }

        public DraggableItemControl(ItemConfiguration config)
        {
            InitializeComponent();
            Configuration = config;
            UpdateUI();
            PowerToggle.Checked += PowerToggle_Checked;
            PowerToggle.Unchecked += PowerToggle_Unchecked; ;
        }

        private void PowerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            Configuration.IsOn = true; StatusIndicator.Fill = Brushes.Red;
            Debug.WriteLine("전원이 꺼졌습니다.");
            GlobalMessageService.ShowMessage("전원이 꺼졌습니다.");
        }

        private void PowerToggle_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.IsOn = true; StatusIndicator.Fill = Brushes.Green;
            Debug.WriteLine("전원이 켜졌습니다.");
            GlobalMessageService.ShowMessage("전원이 켜졌습니다.");
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
            try
            {
                Process.Start(vncViewerPath, $"{Configuration.IpAddress} -password=1015");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연결 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Debug.WriteLine(Configuration.IpAddress);
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void FTP_Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle FTP button click
            Debug.WriteLine(Configuration.FtpAddress);
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }
    }
}