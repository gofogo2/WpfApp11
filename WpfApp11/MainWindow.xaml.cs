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

namespace WpfApp9
{
    public partial class MainWindow : Window
    {
        private List<DraggableItemControl> dragItems = new List<DraggableItemControl>();
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

        public MainWindow()
        {
            InitializeComponent();
            occupiedCells = new bool[GridRows, GridColumns];
            CreateGrid();
            LoadItemConfigurations();
            InitializeAutoPowerSettings();
            FileExplorerControl.CloseRequested += FileExplorerControl_CloseRequested;
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
                    GridCanvas.Children.Add(cell);
                }
            }
        }

        private const int ItemMargin = 10; // 그리드 셀과 아이템 사이의 여백

        private void CreateDraggableItem(ItemConfiguration config)
        {
            var itemControl = new DraggableItemControl(config)
            {
                Width = GridCellWidth - (ItemMargin * 2),
                Height = GridCellHeight - (ItemMargin * 2)
            };

            itemControl.MouseLeftButtonDown += Item_MouseLeftButtonDown;
            itemControl.MouseLeftButtonUp += Item_MouseLeftButtonUp;
            itemControl.MouseMove += Item_MouseMove;

            double left = (config.Column * GridCellWidth) + ItemMargin;
            double top = (config.Row * GridCellHeight) + ItemMargin;

            Canvas.SetLeft(itemControl, left);
            Canvas.SetTop(itemControl, top);
            Canvas.SetZIndex(itemControl, config.ZIndex);

            highestZIndex = Math.Max(highestZIndex, config.ZIndex);

            ItemCanvas.Children.Add(itemControl);
            dragItems.Add(itemControl);
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
                    ShowFileExplorer(item.Configuration.FtpAddress);
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

        private void LoadItemConfigurations()
        {
            if (File.Exists(ConfigFile))
            {
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

        private void TotalPowerBtn_Click(object sender, RoutedEventArgs e)
        {
            bool newState = TotalPowerBtn.Content.ToString() == "전체 전원 ON";
            foreach (var item in dragItems)
            {
                item.Configuration.IsOn = newState;
                UpdateItemPowerState(item, newState);
            }
            TotalPowerBtn.Content = newState ? "전체 전원 OFF" : "전체 전원 ON";
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

        private void ShowFileExplorer(string ftpAddress)
        {
            FileExplorerControl.Initialize(ftpAddress);
            OverlayGrid.Visibility = Visibility.Visible;
        }

        private void FileExplorerControl_CloseRequested(object sender, EventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceWindow addDeviceWindow = new AddDeviceWindow();
            if (addDeviceWindow.ShowDialog() == true)
            {
                ItemConfiguration newConfig = addDeviceWindow.NewDeviceConfig;
                CreateDraggableItem(newConfig);
                SaveItemConfigurations();
            }
        }

        private void RemoveDevice_Click(object sender, RoutedEventArgs e)
        {
            if (dragItems.Count == 0)
            {
                MessageBox.Show("삭제할 기기가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            RemoveDeviceWindow removeDeviceWindow = new RemoveDeviceWindow(dragItems.Select(i => i.Configuration).ToList());
            if (removeDeviceWindow.ShowDialog() == true)
            {
                var configToRemove = removeDeviceWindow.SelectedConfig;
                var itemToRemove = dragItems.FirstOrDefault(i => i.Configuration == configToRemove);
                if (itemToRemove != null)
                {
                    ItemCanvas.Children.Remove(itemToRemove);
                    dragItems.Remove(itemToRemove);
                    SaveItemConfigurations();
                }
            }
        }
        private void InitializeAutoPowerSettings()
        {
            sp_auto.MouseLeftButtonDown += AutoPowerSettings_MouseLeftButtonDown;
            //var autoPowerSettingsPanel = FindName("StackPanel") as StackPanel;
            //if (autoPowerSettingsPanel != null)
            //{
            //    autoPowerSettingsPanel.MouseLeftButtonDown += AutoPowerSettings_MouseLeftButtonDown;
            //}
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
            AutoPowerSettingsOverlay.Visibility = Visibility.Visible;
            PopulateDaySettings();
        }

        private void PopulateDaySettings()
        {
            string[] days = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };
            DaySettingsPanel.Children.Clear();

            foreach (var day in days)
            {
                DaySettingsPanel.Children.Add(CreateDaySettingControl(day));
            }
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

        private void ApplyAutoPowerSettings_Click(object sender, RoutedEventArgs e)
        {
            // 여기에 설정을 저장하는 로직을 구현하세요
            AutoPowerSettingsOverlay.Visibility = Visibility.Collapsed;
        }

        private void CancelAutoPowerSettings_Click(object sender, RoutedEventArgs e)
        {
            AutoPowerSettingsOverlay.Visibility = Visibility.Collapsed;
        }





    }



    public class DraggableItem
    {
        public UIElement UIElement { get; set; }
        public ItemConfiguration Configuration { get; set; }
    }

    public class ItemConfiguration
    {
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public bool IsOn { get; set; }
        public string FtpAddress { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int ZIndex { get; set; }
    }
}