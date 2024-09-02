using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp9;
namespace WpfApp9
{
    public partial class EditDeviceWindow : Window
    {
        public ItemConfiguration EditedDeviceConfig { get; private set; }

        public EditDeviceWindow(ItemConfiguration config)
        {
            InitializeComponent();
            EditedDeviceConfig = config;
            LoadConfigurationData();
        }

        private void LoadConfigurationData()
        {
            NameTextBox.Text = EditedDeviceConfig.Name;
            DeviceTypeComboBox.SelectedItem = DeviceTypeComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == EditedDeviceConfig.DeviceType);
            FtpAddressTextBox.Text = EditedDeviceConfig.FtpAddress;
            MacAddressTextBox.Text = EditedDeviceConfig.MacAddress;
            IpAddressTextBox.Text = EditedDeviceConfig.IpAddress;
            DescriptionTextBox.Text = EditedDeviceConfig.port;
            //InitialStateCheckBox.IsChecked = EditedDeviceConfig.IsOn;
            InitialStateCheckBox.IsChecked = EditedDeviceConfig.IsPower;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // AddDeviceWindow의 AddButton_Click과 유사한 로직 구현
            // 필드 검증 후 EditedDeviceConfig 업데이트
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}