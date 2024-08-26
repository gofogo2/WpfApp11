using Launcher_SE.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp11.Helpers;
using WpfApp11.Helpers.Launcher_SE.Helpers;

namespace WpfApp9
{
    public partial class DraggableItemControl : UserControl
    {
        private UdpReceiver _udpReceiver;

        private const int PingInterval = 5000; // 5초마다 핑 체크
        private bool isPinging = false;
        private CancellationTokenSource pingCancellationTokenSource;
        string vncViewerPath = @"C:\Program Files\TightVNC\tvnviewer.exe"; // TightVNC 뷰어 경로
        public ItemConfiguration Configuration { get; private set; }
        SolidColorBrush onColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
        SolidColorBrush offColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

        public bool ispow = false;

        public void UpdatePowerState(bool isOn)
        {
            ispow = isOn;
            Configuration.IsOn = isOn;
            //PowerToggle.IsChecked = isOn;
            PowerState.Fill = isOn ? onColor : offColor;


            if (isOn)
            {
                if (Configuration.DeviceType == "pc")
                {
                    StatusIndicator.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Configuration.DeviceType == "pc")
                {
                    StatusIndicator.Visibility = Visibility.Collapsed;
                }
            }
            
            
        }



        //public void UpdatePowerState(bool isOn)
        //{
        //    Configuration.IsOn = isOn;
        //    PowerToggle.IsChecked = isOn;
        //    StatusIndicator.Fill = isOn ? Brushes.Green : Brushes.Red;
        //}

        public void StartPingCheck()
        {
            if (Configuration.DeviceType == "pc")
            {
                StopPingCheck();
                pingCancellationTokenSource = new CancellationTokenSource();
                _ = PingCheckLoop(pingCancellationTokenSource.Token);
            }
           


        }

        public void StopPingCheck()
        {
            pingCancellationTokenSource?.Cancel();
            pingCancellationTokenSource?.Dispose();
            pingCancellationTokenSource = null;
        }



        private async Task PingCheckLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool isReachable = await PingHostAsync(Configuration.IpAddress);
                await Dispatcher.InvokeAsync(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        UpdatePowerState(isReachable);
                    }
                });
                await Task.Delay(PingInterval, cancellationToken);
            }
        }

        private async Task<bool> PingHostAsync(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipAddress, 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        public DraggableItemControl(ItemConfiguration config)
        {
            InitializeComponent();
            Configuration = config;
            UpdateUI();
            //PowerToggle.Checked += PowerToggle_Checked;
            //PowerToggle.Unchecked += PowerToggle_Unchecked;


            if (Configuration.DeviceType == "pc")
            {
                StartPingCheck();
            }
            else if (Configuration.DeviceType == "프로젝터")
            {
                StartPingCheck();
            }
            else if (Configuration.DeviceType == "PDU")
            {
                
            }
            else if (Configuration.DeviceType == "RELAY #1")
            {
                //var allowedIp = IPAddress.Parse(config.IpAddress); // 허용할 IP 주소를 설정합니다.
                //_udpReceiver = new UdpReceiver(int.Parse(config.port), allowedIp);
                //Task.Run(() => _udpReceiver.StartReceivingAsync(OnMessageReceived, OnInvalidIpReceived));
            }
            else if (Configuration.DeviceType == "RELAY #2")
            {
                //var allowedIp = IPAddress.Parse(config.IpAddress); // 허용할 IP 주소를 설정합니다.
                //_udpReceiver = new UdpReceiver(int.Parse(config.port), allowedIp);
                //Task.Run(() => _udpReceiver.StartReceivingAsync(OnMessageReceived, OnInvalidIpReceived));
            }
        }

        int relay_onoff = 0;

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // 유효한 IP에서 수신된 메시지를 표시합니다.
                if (message == "00")
                {
                    if (relay_onoff < 5)
                    {
                        relay_onoff++;
                    }
                    else
                    {
                        UpdatePowerState(false);
                    }
                    
                }
                else
                {
                    UpdatePowerState(true);
                    relay_onoff = 0;
                }
                
            });
        }

        private void OnInvalidIpReceived(string message)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    // 유효하지 않은 IP에서 수신된 메시지를 표시합니다.
                
            //});
        }

        public void StopReceiving()
        {
            _udpReceiver?.Stop();
        }

        private async void PowerToggle_Checked(object sender, RoutedEventArgs e)
        {
            //StopPingCheck();
            //UpdatePowerState(true);
            
            //await Task.Delay(5000);
            //StartPingCheck();
        }

        private async void PowerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            //StopPingCheck();
            //UpdatePowerState(false);
            //await Task.Delay(5000);
            //StartPingCheck();
        }






        private void UpdateUI()
        {

            TitleTextBlock.Text = Configuration.Name;

            if (Configuration.DeviceType == "프로젝터")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType == "PDU")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType == "RELAY #1")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType == "RELAY #2")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{Configuration.DeviceType}.png"));
            }
        }


       


        private void StatusIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (Configuration.DeviceType == "pc")
            {
                OverlayGrid.Visibility = OverlayGrid.Visibility == Visibility.Visible ?
                                     Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void FTP_Button_Click(object sender, RoutedEventArgs e)
        {
            // MainWindow의 인스턴스를 가져옵니다.
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                if (ispow == true)
                {
                    mainWindow.ShowFileExplorer(Configuration.IpAddress, Configuration.Name);
                }
                
            }
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void VNC_Button_Click(object sender, RoutedEventArgs e)
        {

            var main = Application.Current.MainWindow as MainWindow;

            if (main.isvnc)
            {
                if (ispow)
                {
                    try
                    {
                        Process.Start(vncViewerPath, $"{Configuration.IpAddress} -password={Configuration.VncPw}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"연결 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    Debug.WriteLine(Configuration.IpAddress);
                }
            }

            
            
            OverlayGrid.Visibility = Visibility.Collapsed;
        }
    

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow.add_device_ppanel.Visibility = Visibility.Visible;

            mainWindow.addDeviceWindow.set_edit_value(Configuration);
            //ㅈㄷㄱㅈㄷㄱㄷㅈㄱ
            //EditDeviceWindow editWindow = new EditDeviceWindow(Configuration);
            //editWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //if (editWindow.ShowDialog() == true)
            //{ 

            //    Configuration = editWindow.EditedDeviceConfig;
            //    UpdateUI();
            //}
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("이 기기를 삭제하시겠습니까?", "기기 삭제", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.RemoveDevice(this);
            }
        }

        private void InfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string info = $"이름: {Configuration.Name}\n" +
                          $"종류: {Configuration.DeviceType}\n" +
                          $"FTP 주소: {Configuration.FtpAddress}\n" +
                          $"MAC 주소: {Configuration.MacAddress}\n" +
                          $"IP 주소: {Configuration.IpAddress}\n" +
                          $"채널: {Configuration.Channel}\n" +
                          $"전원 상태: {(Configuration.IsOn ? "켜짐" : "꺼짐")}";

            MessageBox.Show(info, "기기 정보", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private async void pow_on(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Configuration);
            if (Configuration.DeviceType == "pc")
            {
                WakeOnLanHelper.Instance.TurnOnPC(Configuration.IpAddress, Configuration.MacAddress);
            }
            else if (Configuration.DeviceType == "프로젝터")
            {
                PJLinkHelper.Instance.PowerOn(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "DLP프로젝터")
            {
                await UdpHelper.Instance.SendPowerOnCommandToDLPProjector(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "PDU")
            {
                await WebApiHelper.Instance.OnAll(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "RELAY #1")
            {
                await UdpRelayHelper.Instance.SendPowerOn(Configuration.IpAddress, Configuration.port,"0");
            }

            MessageBox.Show("on");
        }

        private async void pow_off(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Configuration);
            if (Configuration.DeviceType == "pc")
            {
                await UdpHelper.Instance.SendWithIpAsync("power|0", Configuration.IpAddress, 8889);
            }
            else if (Configuration.DeviceType == "프로젝터")
            {
                PJLinkHelper.Instance.PowerOff(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "DLP프로젝터")
            {
                await UdpHelper.Instance.SendPowerOffCommandToDLPProjector(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "PDU")
            {
                await WebApiHelper.Instance.OffAll(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "RELAY #1")
            {
                await UdpRelayHelper.Instance.SendPowerOff(Configuration.IpAddress, Configuration.port, "0");
            }
            MessageBox.Show("off");
        }

        private async void pow_re(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Configuration);
            if (Configuration.DeviceType == "pc")
            {
                await UdpHelper.Instance.SendWithIpAsync("power|1", Configuration.IpAddress, 8889);
            }
            else if (Configuration.DeviceType == "프로젝터")
            {
                
            }
            else if (Configuration.DeviceType == "DLP프로젝터")
            {
                
            }
            else if (Configuration.DeviceType == "PDU")
            {
                await WebApiHelper.Instance.RebootAll(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "RELAY #1")
            {

            }
            MessageBox.Show("off");
        }
    }
}
