using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfApp11.Helpers;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using WpfApp11.UserControls;
using WpfApp11.Helpers.Launcher_SE.Helpers;
using TcpHelperNamespace;
using System.Diagnostics;
using Launcher_SE.Helpers;

namespace WpfApp9
{
    public partial class MainWindow : Window
    {
        private List<DraggableItemControl> dragItems = new List<DraggableItemControl>();
        private const string SettingsFile = "settings.json";
        private Point offset;
        private DraggableItemControl draggedItem;
        private const string ConfigFile = "itemConfig.json";
        private const int GridCellHeight = 200;
        private const int GridCellWidth = 180;
        private const int GridRows = 8;
        private const int GridColumns = 10;
        private bool[,] occupiedCells;
        private int highestZIndex = 1;
        private DateTime lastClickTime = DateTime.MinValue;
        private const double DoubleClickTime = 300; // 밀리초
        private DispatcherTimer powerTimer;
        private double powerProgress = 0;

        private double Progress_duration = 3;
        public bool isvnc = true;
        public bool isftp = true;
        private static readonly Dictionary<string, int> deviceTypeSortOrder = new Dictionary<string, int>
{
    {"projector", 1},
    {"pc", 2},
    {"RELAY #1", 3},
    {"RELAY #2", 4},
    {"pdu", 5}
};

        DispatcherTimer pow_timer = new DispatcherTimer();

        public string local_pc_name = "pc";
        public string local_path = @"C:\test";
        bool first_init = false;

        public MainWindow()
        {
            
            InitializeComponent();
            occupiedCells = new bool[GridRows, GridColumns];
            CreateGrid();
            LoadSettings();
            LoadItemConfigurations();
            InitializeAutoPowerSettings();
            GlobalMessageService.MessageReceived += OnGlobalMessageReceived;
            FileExplorerControl.CloseRequested += FileExplorerControl_CloseRequested;


            pow_timer.Tick += Pow_timer_Tick;
            pow_timer.Interval = TimeSpan.FromMinutes(1);

            first_init = true;
            //TcpHelper.Instance.Send("wer", 234, "wer"); ;
        }

       

        private void LoadSettings()
        {
            //if (File.Exists(SettingsFile))
            //{
            //    string json = File.ReadAllText(SettingsFile);
            //    var settings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
            //    if (settings.TryGetValue("AutoPowerEnabled", out bool isEnabled))
            //    {
            //        AutoPowerToggle.IsChecked = isEnabled;
            //    }
            //}


            if (File.Exists(SettingsFile))
            {
                string json = File.ReadAllText(SettingsFile);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (settings.TryGetValue("AutoPowerEnabled", out var autoPowerValue) && autoPowerValue is bool isEnabled)
                {
                    AutoPowerToggle.IsChecked = isEnabled;
                }

                if (settings.TryGetValue("cms_pc_Name", out var nameValue) && nameValue is string name)
                {
                    local_pc_name = name;
                    p_title.Text = local_pc_name;
                }

                if (settings.TryGetValue("local_path", out var local_pathValue) && local_pathValue is string local_path_value)
                {
                    local_path = local_path_value;
                }

                if (settings.TryGetValue("Progress_duration", out var progressDurationValue) && progressDurationValue is double progressDuration)
                {
                    Progress_duration = progressDuration;
                }

                if (settings.TryGetValue("vnc", out var vnc_value) && vnc_value is bool vncvalue)
                {
                    isvnc = vncvalue;
                }

                if (settings.TryGetValue("ftp", out var ftp_value) && ftp_value is bool ftpvalue)
                {
                    isftp = ftpvalue;
                }
            }

        }

        private void SaveSettings()
        {
            //    var settings = new Dictionary<string, bool>
            //{
            //    { "AutoPowerEnabled", AutoPowerToggle.IsChecked ?? false }
            //};
            //    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            //    File.WriteAllText(SettingsFile, json);

            var settings = new Dictionary<string, object>
            {
                { "cms_pc_Name", local_pc_name },
                { "local_path", local_path },
                { "AutoPowerEnabled", AutoPowerToggle.IsChecked ?? false },
                { "Progress_duration", Progress_duration },
                { "vnc", isvnc },
                { "ftp", isftp}

            };

            // JSON 문자열로 직렬화
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            // JSON 파일에 기록
            File.WriteAllText(SettingsFile, json);


        }

        private void AutoPowerToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (first_init)
            {
                SaveSettings();
            }
            

            // 자동 전원 기능 활성화 로직 추가

            //pow_timer = new DispatcherTimer();
            pow_timer.Start();
        }

        private void AutoPowerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (first_init)
            {
                SaveSettings();
            }

            // 자동 전원 기능 비활성화 로직 추가

            pow_timer.Stop();
        }

        private void Pow_timer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine(AutoPowerSettingsControl.pow_schedule.Values);

            DateTime now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Monday)
            {
                if (AutoPowerSettingsControl.pow_schedule["월요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["월요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["월요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {
                     
                    }
                }
            }
            else if (now.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (AutoPowerSettingsControl.pow_schedule["화요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["화요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["화요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {
                       
                    }
                }
            }
            else if (now.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (AutoPowerSettingsControl.pow_schedule["수요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["수요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["수요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {

                    }
                }
            }
            else if (now.DayOfWeek == DayOfWeek.Thursday)
            {
                if (AutoPowerSettingsControl.pow_schedule["목요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["목요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["목요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {

                    }
                }
            }
            else if (now.DayOfWeek == DayOfWeek.Friday)
            {
                if (AutoPowerSettingsControl.pow_schedule["금요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["금요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["금요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {

                    }
                }
            }
            else if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                if (AutoPowerSettingsControl.pow_schedule["토요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["토요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["토요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {
                      
                    }
                }
            }
            else if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                if (AutoPowerSettingsControl.pow_schedule["일요일"].IsEnabled)
                {
                    if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["일요일"].StartTime.ToString().Substring(0, 5))
                    {
                        //켜지게
                        OnDevice();
                    }
                    else if (now.ToString("HH:mm") == AutoPowerSettingsControl.pow_schedule["일요일"].EndTime.ToString().Substring(0, 5))
                    {
                        //꺼지게
                        OffDevice();
                    }
                    else
                    {

                    }
                }
            }
        }





        public void OnDevice()
        {

            var items = dragItems.Select(a => a.Configuration).ToList();

            _ = SortAndProcessDragItems(items, true);
        }

        public void OffDevice()
        {
            var items = dragItems.Select(a => a.Configuration).ToList();

            _ = SortAndProcessDragItems(items, false);
        }

        public async Task SortAndProcessDragItems(List<ItemConfiguration> drags, bool onOff)
        {
            //var sortedDragItems = drags.OrderBy(item => deviceTypeSortOrder.TryGetValue(item.DeviceType, out int order) ? order : int.MaxValue).ToList();
            var sortedDragItems = drags.FindAll(a => a.DeviceType.ToLower() == "프로젝터");
            var listb = drags.FindAll(a => a.DeviceType.ToLower() == "pc");
            var listc = drags.FindAll(a => a.DeviceType.ToLower() == "relay #1");
            var listd = drags.FindAll(a => a.DeviceType.ToLower() == "relay #2");
            var liste = drags.FindAll(a => a.DeviceType.ToLower() == "pdu");

           sortedDragItems.AddRange(listb);
           sortedDragItems.AddRange(listc);
           sortedDragItems.AddRange(listd);
            sortedDragItems.AddRange(liste);

            foreach(var i in dragItems)
            {
                i.StopPingCheck();
            }

            



            foreach (var item in sortedDragItems)
            {
                Debug.WriteLine($"{item.DeviceType}: {item.IpAddress}: onOff:{onOff}");
                switch (item.DeviceType.ToLower())
                { 
                    case "프로젝터":
                        ProcessProjector(item, onOff);
                        await Task.Delay(1000); //딜레이
                        break;
                    case "pc":
                        ProcessPC(item, onOff);
                        break;
                    case "relay #1":
                        ProcessRelay1(item, onOff);
                        break;
                    case "relay #2":
                        ProcessRelay2(item, onOff);
                        break;
                    case "pdu":
                        ProcessPDU(item, onOff);
                        break;
                    default:
                        break;
                }
            }


            foreach (var i in dragItems)
            {
                i.StartPingCheck();
            }

        }

        // 프로젝터 전원
        private void ProcessProjector(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
                PJLinkHelper.Instance.PowerOn(item.IpAddress);
                
            }
            else
            {
                PJLinkHelper.Instance.PowerOff(item.IpAddress);
            }
        }

        // PC 전원
        private void ProcessPC(ItemConfiguration item,bool onOff)
        {
            if (onOff)
            {
                WakeOnLanHelper.Instance.TurnOnPC(item.IpAddress, item.MacAddress);
            }
            else
            {
                UdpHelper.Instance.SendWithIpAsync("off", item.IpAddress, 11116);
            }
            
        }

        // RELAY #1 전원
        private void ProcessRelay1(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
            }
            else
            {
            }

        }

        // RELAY #2 전원
        private void ProcessRelay2(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
            }
            else
            {
            }
                
        }

        // PDU 전원
        private void ProcessPDU(ItemConfiguration item,bool onOff)
        {
            if (onOff)
            {
                _ = WebApiHelper.Instance.OnAll();
            }
            else
            {
                _ = WebApiHelper.Instance.OffAll();
            }
        }

        private void OnGlobalMessageReceived(object sender, string message)
        {
            GlobalMessage.SetMessage(message);
            AnimateMessage(true);
        }

        private void AnimateMessage(bool show)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = show ? 30 : 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            animation.Completed += (s, e) =>
            {
                if (show)
                {
                    Task.Delay(1000).ContinueWith(_ => Dispatcher.Invoke(() => AnimateMessage(false)));
                }
            };

            GlobalMessage.BeginAnimation(HeightProperty, animation);
        }

        private void CreateGrid()
        {
            GridContainer.Width = GridColumns * GridCellWidth;
            GridContainer.Height = GridRows * GridCellHeight;

            for (int i = 0; i < GridRows; i++)
            {
                for (int j = 0; j < GridColumns; j++)
                {
                    Rectangle cell = new Rectangle
                    {
                        Width = GridCellWidth,
                        Height = GridCellHeight,
                        Stroke = Brushes.White,
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(cell, j * GridCellWidth);
                    Canvas.SetTop(cell, i * GridCellHeight);

                    var gs = new grid_background();

                    var vb = new VisualBrush();
                    
                    vb.Visual = gs;
                    
                    cell.Fill = vb;

                    GridCanvas.Children.Add(cell);
                }
            }
        }

        private const int ItemMargin = 10; // 그리드 셀과 아이템 사이의 여백

        private async void CreateDraggableItem(ItemConfiguration config)
        {

            var itemControl = new DraggableItemControl(config)
            {
                Width = GridCellWidth - (ItemMargin * 2),
                Height = GridCellHeight - (ItemMargin * 2)
            };

            itemControl.MouseLeftButtonDown += Item_MouseLeftButtonDown;
            itemControl.MouseLeftButtonUp += Item_MouseLeftButtonUp;
            itemControl.MouseMove += Item_MouseMove;


            if (itemControl.Configuration.DeviceType == "pc")
            {
                itemControl.StatusIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                itemControl.StatusIndicator.Visibility = Visibility.Collapsed;
            }
            

            double left = (config.Column * GridCellWidth) + ItemMargin;
            double top = (config.Row * GridCellHeight) + ItemMargin;

            Canvas.SetLeft(itemControl, left);
            Canvas.SetTop(itemControl, top);
            Canvas.SetZIndex(itemControl, config.ZIndex);

            highestZIndex = Math.Max(highestZIndex, config.ZIndex);

            ItemCanvas.Children.Add(itemControl);
            dragItems.Add(itemControl);

            SnapToGrid(itemControl);
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            foreach (var item in dragItems)
            {
                item.StopPingCheck();
            }
        }
        public void ShowFileExplorer(string ftpAddress, string name)
        {
            if (isftp)
            {
                OverlayGrid.Visibility = Visibility.Visible;
                FileExplorerControl.Initialize(ftpAddress);
                FileExplorerControl.targetname.Text = name;
            }

            
           
        }


        private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            if ((now - lastClickTime).TotalMilliseconds <= DoubleClickTime)
            {
                // 더블 클릭 처리
                var item = sender as DraggableItemControl;
                if (item != null)
                {
                    if (item.Configuration.DeviceType == "pc")
                    {
                        if (item.ispow)
                        {
                            ShowFileExplorer(item.Configuration.IpAddress, item.Configuration.Name); //더블클릭
                        }
                    }
                    
                }
                e.Handled = true;
            }
            else
            {
                // 단일 클릭 처리 (드래그 시작)
                draggedItem = sender as DraggableItemControl;
                offset = e.GetPosition(draggedItem);
                draggedItem.CaptureMouse();

                int rowIndex = (int)Math.Round(Canvas.GetTop(draggedItem) / GridCellHeight);
                int columnIndex = (int)Math.Round(Canvas.GetLeft(draggedItem) / GridCellWidth);
                if (rowIndex >= 0 && rowIndex < GridRows && columnIndex >= 0 && columnIndex < GridColumns)
                {
                    occupiedCells[rowIndex, columnIndex] = false;
                }

                Canvas.SetZIndex(draggedItem, ++highestZIndex);
            }

            lastClickTime = now;
        }

        private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedItem != null)
            {
                draggedItem.ReleaseMouseCapture();
                SnapToGrid(draggedItem);
                draggedItem = null;
                SaveItemConfigurations();
            }
        }
        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedItem != null)
            {
                Point currentPos = e.GetPosition(ItemCanvas);
                Canvas.SetLeft(draggedItem, currentPos.X - offset.X);
                Canvas.SetTop(draggedItem, currentPos.Y - offset.Y);
            }
        }

        private void SnapToGrid(DraggableItemControl element)
        {
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);

            int columnIndex = (int)Math.Round((left - ItemMargin) / GridCellWidth);
            int rowIndex = (int)Math.Round((top - ItemMargin) / GridCellHeight);

            int[] nearestEmptyCell = FindNearestEmptyCell(rowIndex, columnIndex);
            rowIndex = nearestEmptyCell[0];
            columnIndex = nearestEmptyCell[1];

            Canvas.SetLeft(element, (columnIndex * GridCellWidth) + ItemMargin);
            Canvas.SetTop(element, (rowIndex * GridCellHeight) + ItemMargin);

            occupiedCells[rowIndex, columnIndex] = true;

            // Update the configuration
            element.Configuration.Row = rowIndex;
            element.Configuration.Column = columnIndex;
            element.Configuration.ZIndex = Canvas.GetZIndex(element);
        }

        private int[] FindNearestEmptyCell(int startRow, int startCol)
        {
            int maxDistance = Math.Max(GridRows, GridColumns);

            for (int distance = 0; distance < maxDistance; distance++)
            {
                for (int i = -distance; i <= distance; i++)
                {
                    for (int j = -distance; j <= distance; j++)
                    {
                        if (Math.Abs(i) + Math.Abs(j) == distance)
                        {
                            int newRow = startRow + i;
                            int newCol = startCol + j;

                            if (newRow >= 0 && newRow < GridRows && newCol >= 0 && newCol < GridColumns)
                            {
                                if (!occupiedCells[newRow, newCol])
                                {
                                    return new int[] { newRow, newCol };
                                }
                            }
                        }
                    }
                }
            }

            return new int[] { startRow, startCol };
        }

        private void SaveItemConfigurations()
        {
            List<ItemConfiguration> configurations = dragItems.Select(item => item.Configuration).ToList();
            string jsonString = JsonConvert.SerializeObject(configurations, Formatting.Indented);
            File.WriteAllText(ConfigFile, jsonString);
        }



        public void EditItemConfiguration(ItemConfiguration updatedConfiguration)
        {
            // 현재 구성을 파일에서 읽어옵니다.
            List<ItemConfiguration> currentConfigurations = new List<ItemConfiguration>();

            if (File.Exists(ConfigFile))
            {
                string jsonString = File.ReadAllText(ConfigFile);
                currentConfigurations = JsonConvert.DeserializeObject<List<ItemConfiguration>>(jsonString) ?? new List<ItemConfiguration>();
            }

            // 기존 구성에서 업데이트할 아이템을 찾아 업데이트합니다.
            var existingConfig = currentConfigurations.FirstOrDefault(c => c.id == updatedConfiguration.id);
            if (existingConfig != null)
            {
                existingConfig.Name = updatedConfiguration.Name;
                existingConfig.port = updatedConfiguration.port;
                existingConfig.IpAddress = updatedConfiguration.IpAddress;
                existingConfig.DeviceType = updatedConfiguration.DeviceType;
                existingConfig.MacAddress= updatedConfiguration.MacAddress;
            }
            else
            {
                // 해당 아이템이 없으면 새로 추가합니다.
                //currentConfigurations.Add(updatedConfiguration);
            }

            // 업데이트된 구성을 JSON으로 직렬화하고 파일에 저장합니다.
            string updatedJsonString = JsonConvert.SerializeObject(currentConfigurations, Formatting.Indented);
            File.WriteAllText(ConfigFile, updatedJsonString);


            add_device_ppanel.Visibility = Visibility.Collapsed;


            LoadItemConfigurations();


        }











        private void LoadItemConfigurations()
        {



            if (File.Exists(ConfigFile))
            {


                ItemCanvas.Children.Clear();
                dragItems.Clear();



                string jsonString = File.ReadAllText(ConfigFile);
                List<ItemConfiguration> configurations = JsonConvert.DeserializeObject<List<ItemConfiguration>>(jsonString);

                occupiedCells = new bool[GridRows, GridColumns];

                foreach (var config in configurations)
                {
                    CreateDraggableItem(config);

                    if (config.Row >= 0 && config.Row < GridRows && config.Column >= 0 && config.Column < GridColumns)
                    {
                        occupiedCells[config.Row, config.Column] = true;
                    }

                    highestZIndex = Math.Max(highestZIndex, config.ZIndex);
                }
            }
        }

        private void TotalPowerBtnOn_Click(object sender, RoutedEventArgs e)
        {
            PowerOverlay.Visibility = Visibility.Visible;
            powerProgress = 0;
            PowerProgressBar.Value = 0;
            OnDevice();
            powerTimer = new DispatcherTimer();
            powerTimer.Interval = TimeSpan.FromMilliseconds(10);
            powerTimer.Tick += PowerTimerON_Tick;
            powerTimer.Start();
        }

        private void TotalPowerBtnOff_Click(object sender, RoutedEventArgs e)
        {
            PowerOverlay.Visibility = Visibility.Visible;
            powerProgress = 0;
            PowerProgressBar.Value = 0;

            OffDevice();
            powerTimer = new DispatcherTimer();
            powerTimer.Interval = TimeSpan.FromMilliseconds(10);

            //powerTimer.Interval = TimeSpan.FromSeconds(1);   //방법 1
            powerTimer.Tick += PowerTimerOFF_Tick;
            powerTimer.Start();
        }

        private void PowerTimerOFF_Tick(object sender, EventArgs e)
        {


            //=================================  방법 1=====================================
            //PowerProgressBar.Maximum = Progress_duration;

            //powerProgress += 1;
            //PowerProgressBar.Value = powerProgress;


            //if (powerProgress >= Progress_duration)
            //{
            //    powerTimer.Stop();
            //    PowerOverlay.Visibility = Visibility.Collapsed;

            //    bool newState = PowerStatusText.Text == "전원 OFF";
            //    foreach (var item in dragItems)
            //    {
            //        item.Configuration.IsOn = newState;
            //        UpdateItemPowerState(item, newState);
            //    }
            //    SaveItemConfigurations();
            //}
            //=================================  방법 1=====================================





            PowerProgressBar.Maximum = (Progress_duration * 0.7) * 100;

            powerProgress += 1;
            PowerProgressBar.Value = powerProgress;


            if (powerProgress >= (Progress_duration * 0.7) * 100)
            {
                powerTimer.Stop();
                PowerOverlay.Visibility = Visibility.Collapsed;

                bool newState = PowerStatusText.Text == "전원 OFF";
                foreach (var item in dragItems)
                {
                    item.Configuration.IsOn = newState;
                    UpdateItemPowerState(item, newState);
                }
                SaveItemConfigurations();
            }
        }


        private void PowerTimerON_Tick(object sender, EventArgs e)
        {
            //powerProgress += 1;
            //PowerProgressBar.Value = powerProgress;
            PowerProgressBar.Maximum = (Progress_duration * 0.7) * 100;

            powerProgress += 1;
            PowerProgressBar.Value = powerProgress;

            //if (powerProgress >= 100)
            if (powerProgress >= (Progress_duration * 0.7) * 100)
            {
                powerTimer.Stop();
                PowerOverlay.Visibility = Visibility.Collapsed;

                bool newState = PowerStatusText.Text == "전원 ON";
                foreach (var item in dragItems)
                {
                    item.Configuration.IsOn = newState;
                    UpdateItemPowerState(item, newState);
                }
                SaveItemConfigurations();
            }
        }

       

        private void UpdateItemPowerState(DraggableItemControl item, bool isOn)
        {
            item.UpdatePowerState(isOn);
        }


        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void FileExplorerControl_CloseRequested(object sender, EventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            //AddDeviceControl addDeviceWindow = new AddDeviceControl();

            add_device_ppanel.Visibility = Visibility.Visible;
            addDeviceWindow.addbtn.Visibility = Visibility.Visible;
            addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
            addDeviceWindow.title.Content = "기기 등록";
            addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;
        }


        public void createitem(ItemConfiguration newconfig)
        {

            CreateDraggableItem(newconfig);

           



            SaveItemConfigurations();
        }

        private void RemoveDevice_Click(object sender, RoutedEventArgs e)
        {
            editpanel.Visibility = Visibility.Visible;
            for (int i = 0; i < dragItems.Count; i++)
            {
                dragItems[i].delete_select.Visibility = Visibility.Visible;
            }
        }



        private void item_delete(object sender, RoutedEventArgs e)
        {



            MessageBoxResult result = MessageBox.Show(
               "현재 목록을 저장 하시겠습니까?",   // 메시지
               "확인",                      // 제목
               MessageBoxButton.YesNo,      // 버튼 종류
               MessageBoxImage.Question     // 아이콘 종류
           );

            if (result == MessageBoxResult.Yes)
            {
                // 사용자가 'Yes'를 클릭했을 때의 처리
                for (int i = dragItems.Count - 1; i >= 0; i--)
                {
                    if (dragItems[i].d_select.IsChecked == true)
                    {
                        dragItems[i].StopReceiving();
                        ItemCanvas.Children.Remove(dragItems[i]);
                        dragItems.RemoveAt(i);
                    }
                }

                SaveItemConfigurations();



                editpanel.Visibility = Visibility.Collapsed;
                for (int i = 0; i < dragItems.Count; i++)
                {
                    dragItems[i].delete_select.Visibility = Visibility.Collapsed;
                    dragItems[i].d_select.IsChecked = false;
                }
            }
            else
            {
              
            }          
        }
        private void editpanel_close(object sender, RoutedEventArgs e)
        {
            editpanel.Visibility = Visibility.Collapsed;
            for (int i = 0; i < dragItems.Count; i++)
            {
                dragItems[i].delete_select.Visibility = Visibility.Collapsed;
            }
        }
        private void AutoPowerSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowAutoPowerSettingsOverlay();
            }
        }
        private void ShowAutoPowerSettingsOverlay()
        {
            AutoPowerSettingsControl.Visibility = Visibility.Visible;
        }

        private void InitializeAutoPowerSettings()
        {
            //sp_auto.MouseLeftButtonDown += AutoPowerSettings_MouseLeftButtonDown;
            AutoPowerSettingsControl.CloseRequested += (sender, e) =>
            {
                AutoPowerSettingsControl.Visibility = Visibility.Collapsed;
            };
            auto_wol_btn.Click += Auto_wol_btn_Click;
        }

        private void Auto_wol_btn_Click(object sender, RoutedEventArgs e)
        {
            ShowAutoPowerSettingsOverlay();
        }

        private UIElement CreateDaySettingControl(string day)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

            panel.Children.Add(new CheckBox { Content = day, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBlock { Text = "시작 시간", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "시", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "분", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBlock { Text = "/ 종료 시간", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "시", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "분", VerticalAlignment = VerticalAlignment.Center });

            return panel;
        }
        public void RemoveDevice(DraggableItemControl deviceControl)
        {
            ItemCanvas.Children.Remove(deviceControl);
            dragItems.Remove(deviceControl);
            SaveItemConfigurations();
        }
    }





    public class DraggableItem
    {
        public UIElement UIElement { get; set; }
        public ItemConfiguration Configuration { get; set; }
    }

    public class ItemConfiguration
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public bool IsOn { get; set; }
        public string FtpAddress { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public string port { get; set; }

        public string Description { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int ZIndex { get; set; }
        public string VncPw { get; set; }

    }
}