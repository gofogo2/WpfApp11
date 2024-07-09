using System.Windows;
using System.Windows.Controls;

namespace WpfApp9
{
    public partial class AddDeviceWindow : Window
    {
        public ItemConfiguration NewDeviceConfig { get; private set; }

        public AddDeviceWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || DeviceTypeComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(FtpAddressTextBox.Text))
            {
                MessageBox.Show("모든 필드를 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewDeviceConfig = new ItemConfiguration
            {
                Name = NameTextBox.Text,
                DeviceType = ((ComboBoxItem)DeviceTypeComboBox.SelectedItem).Content.ToString(),
                FtpAddress = FtpAddressTextBox.Text,
                IsOn = InitialStateCheckBox.IsChecked ?? false,
                Row = 0,
                Column = 0,
                ZIndex = 1
            };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}