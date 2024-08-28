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

        public DraggableItemControl(ItemConfiguration config)
        {
            InitializeComponent();
            Configuration = config;
            UpdateUI();

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

            // ContextMenu 생성
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            menu.Style = FindResource("CustomContextMenuStyle") as Style;
            var main = Application.Current.MainWindow as MainWindow;

            if (Configuration.DeviceType.ToLower() == "pc")
            {
                        menu.Items.Add(CreateMenuItem("VNC", VNC_Button_Click));
                        menu.Items.Add(CreateMenuItem("FTP", FTP_Button_Click));
                 
            }
            menu.Items.Add(CreateMenuItem("EDIT", EditMenuItem_Click));
            menu.Items.Add(CreateMenuItem("DELETE", DeleteMenuItem_Click));

            this.ContextMenu = menu;
        }


    

        private MenuItem CreateMenuItem(string header, RoutedEventHandler clickHandler)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = header;
            menuItem.Click += clickHandler;
            menuItem.Style = FindResource("CustomMenuItemStyle") as Style;
            return menuItem;
        }


        private Separator CreateSeparator()
        {
            Separator separator = new Separator();
            separator.Style = FindResource("CustomSeparatorStyle") as Style;
            return separator;
        }

        public void UpdatePowerState(bool isOn)
        {
            ispow = isOn;
            Configuration.IsOn = isOn;
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
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                if (ispow == true)
                {
                    mainWindow.ShowFileExplorer(Configuration.IpAddress, Configuration.Name);
                }
                else
                {
                    MessageBox.Show("전원이 꺼져있습니다.");
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
                else
                {
                    MessageBox.Show("전원이 꺼져있습니다.");
                }
            }
            else
            {
                MessageBox.Show("전원이 꺼져있습니다.");
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
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("이 기기를 삭제하시겠습니까?", "기기 삭제", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.RemoveDevice(this);
            }
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
                await UdpRelayHelper.Instance.SendPowerOn(Configuration.IpAddress, Configuration.port, "0");
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
                // 프로젝터 재시작 로직
            }
            else if (Configuration.DeviceType == "DLP프로젝터")
            {
                // DLP 프로젝터 재시작 로직
            }
            else if (Configuration.DeviceType == "PDU")
            {
                await WebApiHelper.Instance.RebootAll(Configuration.IpAddress);
            }
            else if (Configuration.DeviceType == "RELAY #1")
            {
                // RELAY #1 재시작 로직
            }
            MessageBox.Show("재시작");
        }

        public void StopReceiving()
        {
            _udpReceiver?.Stop();
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // 유효한 IP에서 수신된 메시지를 처리합니다.
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
            // 유효하지 않은 IP에서 수신된 메시지 처리 (필요한 경우)
        }

        private int relay_onoff = 0;
    }
}