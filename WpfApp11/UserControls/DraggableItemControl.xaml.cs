using Launcher_SE.Helpers;
using System;
using System.Collections.Generic;
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
using WpfApp11.UserControls;

namespace WpfApp9
{
    public partial class DraggableItemControl : UserControl
    {
        private UdpReceiver _udpReceiver;
        public WakeOnLan wol;
        private  int PingInterval = 5000; // 5초마다 핑 체크
        private CancellationTokenSource pduStatusCancellationTokenSource;
        private bool isPinging = false;
        private CancellationTokenSource pingCancellationTokenSource;
        string vncViewerPath = @"C:\Program Files\TightVNC\tvnviewer.exe"; // TightVNC 뷰어 경로
        public ItemConfiguration Configuration { get; private set; }
        SolidColorBrush onColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ACD7FE"));
        SolidColorBrush offColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

        public bool ispow = false;

        public DraggableItemControl(ItemConfiguration config)
        {
            InitializeComponent();
            Configuration = config;
            UpdateUI();
            wol = new WakeOnLan();


            var main = Application.Current.MainWindow as MainWindow;

            PingInterval = main.pingtime * 1000;

            if (PingInterval < 5000)
            {
                PingInterval = 5000;
            }


            if (Configuration.DeviceType.ToLower() == "pc")
            {
                StartPingCheck();
            }
            else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
            {
                StartPingCheck();
            }
            else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
            {
                StartPingCheck();
            }
            else if (Configuration.DeviceType == "PDU")
            {
                StartPDUStatus();

            }
            else if (Configuration.DeviceType == "RELAY")
            {
        
            }


            // ContextMenu 생성
            CreateContextMenu();
        }

        public async void StartPDUStatus()
        {
            StopPDUStatus(); // 기존 체크가 실행 중이면 중지
            pduStatusCancellationTokenSource = new CancellationTokenSource();
            await PDUStatusCheckLoop(pduStatusCancellationTokenSource.Token);
        }

        public void StopPDUStatus()
        {
            pduStatusCancellationTokenSource?.Cancel();
            pduStatusCancellationTokenSource?.Dispose();
            pduStatusCancellationTokenSource = null;
        }

        private async Task PDUStatusCheckLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Dictionary<string, string> p = await WebApiHelper.Instance.StatusPDU(Configuration.IpAddress, Configuration.Channel);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (p["Error"] == "Fail")
                            {
                                UpdatePowerState(false);
                            }
                            else
                            {
                                int channelNumber = Convert.ToInt32(Configuration.Channel);
                                string formattedChannel = channelNumber.ToString("00");
                                bool isOn = p[formattedChannel] == "ON";
                                UpdatePowerState(isOn);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    // 오류 처리 (로깅 등)
                    Logger.Log2($"PDU 상태 체크 중 오류 발생: {ex.Message}");
                    Debug.WriteLine($"PDU 상태 체크 중 오류 발생: {ex.Message}");
                }

                await Task.Delay(PingInterval, cancellationToken);
            }
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
            menu.Items.Add(CreateMenuItem("DELETE", DeleteMenuItem_Click));  //여기다

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

            //if (isOn)
            //{
            //    if (Configuration.DeviceType == "pc")
            //    {
            //        StatusIndicator.Visibility = Visibility.Visible;
            //    }
            //}
            //else
            //{
            //    if (Configuration.DeviceType == "pc")
            //    {
            //        StatusIndicator.Visibility = Visibility.Collapsed;
            //    }
            //}
        }

        public void StartPingCheck()
        {
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc" || Configuration.DeviceType.ToLower() == "프로젝터(pjlink)" || Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
                {
                    StopPingCheck();
                    pingCancellationTokenSource = new CancellationTokenSource();
                    _ = PingCheckLoop(pingCancellationTokenSource.Token);
                }
            }catch(Exception e)
            {

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

            if (Configuration.Name.Length > 15)
            {
                TitleTextBlock.Text = Configuration.Name.Substring(0, 13) + "..";
            }
            else
            {
                TitleTextBlock.Text = Configuration.Name;
            }
            


            if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType == "PDU")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/PDU.png"));
                IconImage.Width = 67;
                IconImage.Height = 46;

            }
            else if (Configuration.DeviceType == "RELAY")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/RELAY.png"));
                IconImage.Width = 95;
                IconImage.Height = 46;
            }
            else
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/pc.png"));
            }
        }

        private void StatusIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Configuration.DeviceType.ToLower() == "pc")
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
                if (Configuration.IsOn == true)
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
                if (Configuration.IsOn)
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
            //if (MessageBox.Show("이 기기를 삭제하시겠습니까?", "기기 삭제", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    var mainWindow = Window.GetWindow(this) as MainWindow;
            //    mainWindow?.RemoveDevice(this);
            //}


            delete_dialog dialog = new delete_dialog
            {
                Owner = Application.Current.MainWindow,
                
            };

            dialog.popup_msg.Text = "해당 장비를 삭제하시겠습니까?\n삭제 후 되돌릴 수 없습니다.";


            bool? result = dialog.ShowDialog(); // 모달 다이얼로그로 표시됨

            if (dialog.DialogResult == true)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                    mainWindow?.RemoveDevice(this);
            }
          
        }

        private async void pow_on(object sender, RoutedEventArgs e)
        {
            string result = "Success";
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc")
                {
                    wol.TurnOnPC(Configuration.MacAddress);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
                {
                  await  PJLinkHelper.Instance.PowerOnAsync(Configuration.IpAddress);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
                {
                    await DlpProjectorHelper.Instance.SendPowerOffCommandToDLPProjector(Configuration.IpAddress);
                }
                else if (Configuration.DeviceType == "PDU")
                {
                    await WebApiHelper.Instance.OnPDU(Configuration.IpAddress, Configuration.Channel);
                }
                else if (Configuration.DeviceType == "RELAY")
                {


                    string hexStr = Utils.Instance.IntToHex(Configuration.Channel);
                    Debug.WriteLine(hexStr);
                    //16진수로 제어
                    string hex = $"525920{hexStr}20310D";
                    Logger.Log(Configuration.IpAddress, Configuration.port, "Power ON", hex);
                    await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(Configuration.port), Configuration.IpAddress);
                }
            }
            catch (Exception ex)
            {
                result = $"Error: {ex.Message}";
                Logger.Log2($"Error: {ex.Message}");
            }

            Logger.Log(Configuration.Name, Configuration.DeviceType, "Power On", result);
            MessageBox.Show("전원이 켜졌습니다");
        }



       

        private async void pow_off(object sender, RoutedEventArgs e)
        {
            string result = "Success";
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc")
                {
                    await UdpHelper.Instance.SendWithIpAsync("power|0", Configuration.IpAddress, 8889);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
                {
                    await PJLinkHelper.Instance.PowerOffAsync(Configuration.IpAddress);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
                {
                    await DlpProjectorHelper.Instance.SendPowerOffCommandToDLPProjector(Configuration.IpAddress);
                }
                else if (Configuration.DeviceType == "PDU")
                {
                    await WebApiHelper.Instance.OffPDU(Configuration.IpAddress,Configuration.Channel);
                }
                else if (Configuration.DeviceType == "RELAY")
                {

                    string hexStr = Utils.Instance.IntToHex(Configuration.Channel);
                    Debug.WriteLine(hexStr);


                    string hex = $"525920{hexStr}20300D";
                    Logger.Log(Configuration.IpAddress, Configuration.port, "Power OFF", hex);
                    await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(Configuration.port), Configuration.IpAddress);
                }
            }
            catch (Exception ex)
            {
                result = $"Error: {ex.Message}";
                Logger.Log2($"Error: {ex.Message}");
            }

            Logger.Log(Configuration.Name, Configuration.DeviceType, "Power Off", result);
            MessageBox.Show("전원이 꺼졌습니다.");
        }

        public static string ConvertStringToHex(string input)
        {
            try
            {
                // 문자열을 정수로 변환
                if (!int.TryParse(input, out int number))
                {
                    throw new ArgumentException("입력 문자열을 정수로 변환할 수 없습니다.");
                }

                // 정수를 16진수 문자열로 변환
                string hexString = number.ToString("X");

                // 16진수 문자열의 길이가 홀수인 경우 앞에 0 추가
                if (hexString.Length % 2 != 0)
                {
                    hexString = "0" + hexString;
                }

                return hexString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"변환 중 오류 발생: {ex.Message}");
                return string.Empty;
            }
        }

        private async void pow_re(object sender, RoutedEventArgs e)
        {
            string result = "Success";
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc")
                {
                    await UdpHelper.Instance.SendWithIpAsync("power|1", Configuration.IpAddress, 8889);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
                {
                    //PJLinkHelper.Instance.PowerOff(Configuration.IpAddress);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
                {
                    //await UdpHelper.Instance.SendPowerOffCommandToDLPProjector(Configuration.IpAddress);
                }
                else if (Configuration.DeviceType == "PDU")
                {
                    await WebApiHelper.Instance.RebootPDU(Configuration.IpAddress, Configuration.Channel);
                }
            }
            catch (Exception ex)
            {
                result = $"Error: {ex.Message}";
                Logger.Log2($"Error: {ex.Message}");
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