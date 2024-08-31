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
        private const int GridCellWidth = 200;
        private const int GridRows = 8;
        private const int GridColumns = 9;
        private bool[,] occupiedCells;
        private int highestZIndex = 1;
        private DateTime lastClickTime = DateTime.MinValue;
        private const double DoubleClickTime = 300; // 밀리초
        private double powerProgress = 0;

        private double Progress_duration = 50;
        public bool isvnc = false;
        public bool isftp = false;

        private int clickCount = 0;
        private DispatcherTimer clickTimer;
        public WakeOnLan wol;

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
        public string local_path = @"C:\GL-MEDIA";
        bool first_init = false;

        public MainWindow()
        {
            InitializeComponent();
            clickTimer = new DispatcherTimer();
            clickTimer.Interval = TimeSpan.FromMilliseconds(200); // 500 ms interval for double-click detection
            clickTimer.Tick += ClickTimer_Tick;
            AutoPowerSettingsControl.init();
            wol = new WakeOnLan();
            occupiedCells = new bool[GridRows, GridColumns];
            CreateGrid();
            if (!File.Exists(SettingsFile))
            {
                SaveSettings();
            }

            LoadSettings();

           
                LoadItemConfigurations();
            InitializeAutoPowerSettings();
            GlobalMessageService.MessageReceived += OnGlobalMessageReceived;
            FileExplorerControl.CloseRequested += FileExplorerControl_CloseRequested;

            pow_timer.Tick += Pow_timer_Tick;
            pow_timer.Interval = TimeSpan.FromSeconds(30);

            first_init = true;


            this.KeyDown += MainWindow_KeyDown;

          
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                addDeviceWindow.cancle_popup();
                AutoPowerSettingsControl.cancle_ev();

                editpanel.Visibility = Visibility.Collapsed;
                for (int i = 0; i < dragItems.Count; i++)
                {
                    dragItems[i].delete_select.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                string json = File.ReadAllText(SettingsFile);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (settings.TryGetValue("AutoPowerEnabled", out var autoPowerValue) && autoPowerValue is bool isEnabled)
                {
                    AutoPowerToggle.IsChecked = isEnabled;
                }

                if (settings.TryGetValue("CMSTitle", out var nameValue) && nameValue is string name)
                {
                    local_pc_name = name;
                    p_title.Text = local_pc_name;
                }

                if (settings.TryGetValue("ContentsPath", out var local_pathValue) && local_pathValue is string local_path_value)
                {
                    local_path = local_path_value;
                }

                if (settings.TryGetValue("Progress_duration", out var progressDurationValue) && progressDurationValue is double progressDuration)
                {
                    Progress_duration = progressDuration;
                }

                if (settings.TryGetValue("useVNC", out var vnc_value) && vnc_value is bool vncvalue)
                {
                    isvnc = vncvalue;
                }

                if (settings.TryGetValue("useFTP", out var ftp_value) && ftp_value is bool ftpvalue)
                {
                    isftp = ftpvalue;
                }
            }
        }

        private void SaveSettings()
        {
            var settings = new Dictionary<string, object>
            {
                { "CMSTitle", local_pc_name },
                { "ContentsPath", local_path },
                { "AutoPowerEnabled", AutoPowerToggle.IsChecked ?? false },
                { "Progress_duration", Progress_duration },
                { "useVNC", isvnc },
                { "useFTP", isftp}
            };

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }

        private void AutoPowerToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (first_init)
            {
                SaveSettings();
            }
            pow_timer.Start();
        }

        private void AutoPowerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (first_init)
            {
                SaveSettings();
            }
            pow_timer.Stop();
        }

        private async void Pow_timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string[] days = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };
            //string currentDay = days[(int)now.DayOfWeek - 1];
            string currentDay;
            if ((int)now.DayOfWeek == 0)
            {
                currentDay = days[6];
            }
            else
            {
                currentDay = days[(int)now.DayOfWeek - 1];
            }


            //string currentDay = days[(int)now.DayOfWeek];

            if (AutoPowerSettingsControl.pow_schedule[currentDay].IsEnabled)
            {
                string currentTime = now.ToString("HH:mm");
                if (currentTime == AutoPowerSettingsControl.pow_schedule[currentDay].StartTime.ToString().Substring(0, 5))
                {
                    await OnDevice();
                }
                else if (currentTime == AutoPowerSettingsControl.pow_schedule[currentDay].EndTime.ToString().Substring(0, 5))
                {
                    await OffDevice();
                }
            }
        }

        public async Task OnDevice()
        {
            PowerOverlay.Visibility = Visibility.Visible;
            PowerProgressBar.Value = 0;
            var items = dragItems.Select(a => a.Configuration).ToList();
            await SortAndProcessDragItems(items, true, 0, 25); // 장치 처리
            await AddDelay(25, 100); //10초 딜레이
            await FinalizeDeviceOperation(true);
        }

        public async Task OffDevice()
        {
            PowerOverlay.Visibility = Visibility.Visible;
            PowerProgressBar.Value = 0;
            var items = dragItems.Select(a => a.Configuration).ToList();
            await SortAndProcessDragItems(items, false, 0, 25); //장치 처리
            await AddDelay(25, 100); // 10초 딜레이
            await FinalizeDeviceOperation(false);
        }
        public async Task SortAndProcessDragItems(List<ItemConfiguration> drags, bool onOff, double startProgress, double endProgress)
        {
            var sortedDragItems = onOff
           ? drags.OrderBy(a => a.DeviceType.ToLower() == "relay" ? 1 :
                                a.DeviceType.ToLower() == "pdu" ? 2 :
                                a.DeviceType.ToLower() == "프로젝터" ? 3 :
                                a.DeviceType.ToLower() == "DLP프로젝터" ? 4 :
                                a.DeviceType.ToLower() == "pc" ? 5 : 6)
                  .ToList()
           : drags.OrderBy(a => a.DeviceType.ToLower() == "pc" ? 1 :
                                a.DeviceType.ToLower() == "DLP프로젝터" ? 2 :
                                a.DeviceType.ToLower() == "프로젝터" ? 3 :
                                a.DeviceType.ToLower() == "pdu" ? 4 :
                                a.DeviceType.ToLower() == "relay" ? 5 : 6)
                  .ToList();




            foreach (var i in dragItems)
            {
                i.StopPingCheck();
            }

            //isOn true 인것만
            //sortedDragItems = sortedDragItems.FindAll(a => a.IsOn == true).ToList();

            int totalItems = sortedDragItems.Count;
            for (int i = 0; i < totalItems; i++)
            {
                var item = sortedDragItems[i];
                Debug.WriteLine($"{item.DeviceType}: {item.IpAddress}: onOff:{onOff}");

                switch (item.DeviceType.ToLower())
                {
                    case "프로젝터":
                        await ProcessProjector(item, onOff);
                        await Task.Delay(1000);
                        break;
                    case "dlp프로젝터":
                        await ProcessDLPProjector(item, onOff);
                        await Task.Delay(1000);
                        break;
                    case "pc":
                        ProcessPC(item, onOff);
                        break;
                    case "relay":
                        ProcessRelay1(item, onOff);
                        break;
                    case "pdu":
                        ProcessPDU(item, onOff);
                        break;
                }

                double progress = startProgress + (endProgress - startProgress) * ((i + 1) / (double)totalItems);
                Dispatcher.Invoke(() => PowerProgressBar.Value = progress);
                
                await Task.Delay(200);
            }

            foreach (var i in dragItems)
            {
                i.StartPingCheck();
            }
        }

        private async Task AddDelay(double startProgress, double endProgress)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            double delayDuration = 10000; // 10초

            while (watch.ElapsedMilliseconds < delayDuration)
            {
                double progress = startProgress + (endProgress - startProgress) * (watch.ElapsedMilliseconds / delayDuration);
                Dispatcher.Invoke(() => PowerProgressBar.Value = progress);
                await Task.Delay(100);
            }

            watch.Stop();
        }

        private Task FinalizeDeviceOperation(bool isOn)
        {
            PowerOverlay.Visibility = Visibility.Collapsed;

            foreach (var item in dragItems)
            {
                item.Configuration.IsOn = isOn;
                UpdateItemPowerState(item, isOn);
            }
            SaveItemConfigurations();
            return Task.CompletedTask;
        }



        private async void TotalPowerBtnOn_Click(object sender, RoutedEventArgs e)
        {
            PowerStatusText.Text = "전원 ON";
            await OnDevice();
        }

        private async void TotalPowerBtnOff_Click(object sender, RoutedEventArgs e)
        {
            PowerStatusText.Text = "전원 OFF";
            await OffDevice();
        }

        private async Task ProcessProjector(ItemConfiguration item, bool onOff)
        {
            try
            {
                bool result = onOff
                    ? await PJLinkHelper.Instance.PowerOnAsync(item.IpAddress)
                    : await PJLinkHelper.Instance.PowerOffAsync(item.IpAddress);

                if (!result)
                {
                    Debug.WriteLine($"Failed to {(onOff ? "power on" : "power off")} projector at {item.IpAddress}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing projector: {ex.Message}");
            }
        }

        private async Task ProcessDLPProjector(ItemConfiguration item, bool onOff)
        {
            try
            {
                bool result = onOff
                    ? await DlpProjectorHelper.Instance.SendPowerOnCommandToDLPProjector(item.IpAddress)
                    : await DlpProjectorHelper.Instance.SendPowerOffCommandToDLPProjector(item.IpAddress);

                if (!result)
                {
                    Debug.WriteLine($"Failed to {(onOff ? "power on" : "power off")} projector at {item.IpAddress}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing projector: {ex.Message}");
            }
        }

        private void ProcessPC(ItemConfiguration item, bool onOff)
        {
            try
            {
                if (onOff)
                {
                    
                    wol.TurnOnPC(item.MacAddress);
                }
                else
                {
                    UdpHelper.Instance.SendWithIpAsync("power|0", item.IpAddress, 8889);
                }
            }catch(Exception e)
            {

            }
        }

        private void ProcessRelay1(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
                OnRelay(item);
            }
            else
            {
                OffRelay(item);
            }
        }


        private async void OnRelay(ItemConfiguration item )
        {
            string hexStr = Utils.Instance.IntToHex(item.Channel);
            Debug.WriteLine(hexStr);
            string hex = $"525920{hexStr}20310D";
            Logger.Log(item.IpAddress, item.port, "Power ON", hex);
            await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(item.port), item.IpAddress);
        }

        private async void OffRelay(ItemConfiguration item)
        {
            string hexStr = Utils.Instance.IntToHex(item.Channel);
            Debug.WriteLine(hexStr);


            string hex = $"525920{hexStr}20300D";
            Logger.Log(item.IpAddress, item.port, "Power OFF", hex);
            await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(item.port), item.IpAddress);
        }


        private void ProcessPDU(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
                _ = WebApiHelper.Instance.OnPDU(item.IpAddress, item.Channel);
            }
            else
            {
                _ = WebApiHelper.Instance.OffPDU(item.IpAddress,item.Channel);
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
                        //Width = GridCellWidth,
                        //Height = GridCellHeight,
                        Width = 200,
                        Height = 200,
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

        private const int ItemMargin = 10;

        private void CreateDraggableItem(ItemConfiguration config)
        {
            var itemControl = new DraggableItemControl(config)
            {
                Width = 180,
                Height = 180
                //Width = GridCellWidth - (ItemMargin * 2),
                //Height = GridCellHeight - (ItemMargin * 2)
            };

            itemControl.MouseLeftButtonDown += Item_MouseLeftButtonDown;
            itemControl.MouseLeftButtonUp += Item_MouseLeftButtonUp;
            itemControl.MouseMove += Item_MouseMove;

            //itemControl.StatusIndicator.Visibility = config.DeviceType == "pc" ? Visibility.Visible : Visibility.Collapsed;

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
                var item = sender as DraggableItemControl;
                if (item != null && item.Configuration.DeviceType == "pc" && item.ispow)
                {
                    ShowFileExplorer(item.Configuration.IpAddress, item.Configuration.Name);
                }
                e.Handled = true;
            }
            else
            {
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
            List<ItemConfiguration> currentConfigurations = new List<ItemConfiguration>();

            if (File.Exists(ConfigFile))
            {
                string jsonString = File.ReadAllText(ConfigFile);
                currentConfigurations = JsonConvert.DeserializeObject<List<ItemConfiguration>>(jsonString) ?? new List<ItemConfiguration>();
            }

            var existingConfig = currentConfigurations.FirstOrDefault(c => c.id == updatedConfiguration.id);
            if (existingConfig != null)
            {
                existingConfig.Name = updatedConfiguration.Name;
                existingConfig.port = updatedConfiguration.port;
                existingConfig.IpAddress = updatedConfiguration.IpAddress;
                existingConfig.DeviceType = updatedConfiguration.DeviceType;
                existingConfig.MacAddress = updatedConfiguration.MacAddress;
                existingConfig.Channel = updatedConfiguration.Channel;
            }

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

     

        private async Task AnimateProgressBar(bool isOn)
        {
            double animationDuration = Progress_duration * 0.7;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (watch.Elapsed.TotalSeconds < animationDuration)
            {
                await Task.Delay(16); // 약 60fps
                double progress = Math.Min(watch.Elapsed.TotalSeconds / animationDuration, 1);
                PowerProgressBar.Value = progress * 100;
            }

            watch.Stop();
            PowerProgressBar.Value = 100;

            PowerOverlay.Visibility = Visibility.Collapsed;

            bool newState = isOn;
            foreach (var item in dragItems)
            {
                item.Configuration.IsOn = newState;
                UpdateItemPowerState(item, newState);
            }
            SaveItemConfigurations();
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

        private void tt(object sender, EventArgs e)
        {
            var canvas = sender as Canvas;
            if (canvas != null)
            {
                // Get the ContextMenu associated with the Canvas
                var contextMenu = canvas.ContextMenu;

                // Show the ContextMenu at the position of the mouse click
                if (contextMenu != null)
                {
                    contextMenu.IsOpen = true;
                }
            }
        }

        private void add_devi(object sender, RoutedEventArgs e)
        {
            add_Device_init();
            add_device_ppanel.Visibility = Visibility.Visible;
            addDeviceWindow.addbtn.Visibility = Visibility.Visible;
            addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
            addDeviceWindow.title.Content = "장비 등록";
            addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;
        }

        private void del_devi(object sender, RoutedEventArgs e)
        {
            editpanel.Visibility = Visibility.Visible;
            for (int i = 0; i < dragItems.Count; i++)
            {
                dragItems[i].delete_select.Visibility = Visibility.Visible;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (clickCount == 2)
            {
                // Double-click detected
                add_Device_init();

                add_device_ppanel.Visibility = Visibility.Visible;
                addDeviceWindow.addbtn.Visibility = Visibility.Visible;
                addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
                addDeviceWindow.title.Content = "장비 등록";
                addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;


                clickCount = 0; // Reset click count
                clickTimer.Stop(); // Stop the timer
            }
            else
            {
                clickTimer.Start(); // Start or restart the timer
            }
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            clickTimer.Stop(); // Stop the timer when interval expires
            clickCount = 0; // Reset click count
        }



        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            add_Device_init();
            add_device_ppanel.Visibility = Visibility.Visible;
            addDeviceWindow.addbtn.Visibility = Visibility.Visible;
            addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
            addDeviceWindow.title.Content = "장비 등록";
            addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;
        }


        void add_Device_init()
        {
            addDeviceWindow.NameTextBox.Text = "";
            addDeviceWindow.DeviceTypeComboBox.SelectedIndex = -1;
            addDeviceWindow.IpAddressTextBox.Text = "";
            addDeviceWindow.DescriptionTextBox.Text = "";
            addDeviceWindow.MacAddressTextBox.Text = "";
            addDeviceWindow.ChannelTextBox.Text = "";

            addDeviceWindow.InitialStateCheckBox.IsChecked = false;
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
               "현재 목록을 저장 하시겠습니까?",
               "확인",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question
           );

            if (result == MessageBoxResult.Yes)
            {
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
            AutoPowerSettingsControl.CloseRequested += (sender, e) =>
            {
                AutoPowerSettingsControl.Visibility = Visibility.Collapsed;
            };
            //auto_wol_btn.Click += Auto_wol_btn_Click;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

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
        public string Channel { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int ZIndex { get; set; }
        public string VncPw { get; set; }
    }
}