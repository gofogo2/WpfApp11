using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp9;

namespace WpfApp11.UserControls
{
   


    public partial class AddDeviceControl : UserControl
    {

        public ItemConfiguration NewDeviceConfig { get; private set; }

        public AddDeviceControl()
        {
            InitializeComponent();
        }

        static bool IsValidIPv4(string ipAddress)
        {
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(ipAddress, pattern);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                DeviceTypeComboBox.SelectedItem == null ||
                
                string.IsNullOrWhiteSpace(MacAddressTextBox.Text) ||
                string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
            {
                MessageBox.Show("필수 필드를 모두 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsValidIPv4(IpAddressTextBox.Text))
            {
            }
            else
            {
                MessageBox.Show("유효하지 않은 IP 주소입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



         

            NewDeviceConfig = new ItemConfiguration
            {
                Name = NameTextBox.Text,
                DeviceType = ((ComboBoxItem)DeviceTypeComboBox.SelectedItem).Content.ToString(),
                
                MacAddress = MacAddressTextBox.Text,
                IpAddress = IpAddressTextBox.Text,
                port = DescriptionTextBox.Text,
                IsOn = InitialStateCheckBox.IsChecked ?? false,
                Row = 0,
                Column = 0,
                ZIndex = 1,
                VncPw = "1015"
            };


            //ItemConfiguration newConfig = addDeviceWindow.NewDeviceConfig;
            //CreateDraggableItem(newConfig);
            //SaveItemConfigurations();
            var main = Application.Current.MainWindow as MainWindow;
            main.createitem(NewDeviceConfig);

            main.add_device_ppanel.Visibility = Visibility.Collapsed;

            data_clear();
            //DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {



            var main = Application.Current.MainWindow as MainWindow;
            main.add_device_ppanel.Visibility = Visibility.Collapsed;


            data_clear();


            //DialogResult = false;
        }

        void data_clear()
        {
            NameTextBox.Text = "";
            DeviceTypeComboBox.SelectedItem = null;


            MacAddressTextBox.Text = "";
            IpAddressTextBox.Text = "";
            DescriptionTextBox.Text = "";
        }
    }
}
