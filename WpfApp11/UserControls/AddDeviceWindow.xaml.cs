using System.Windows;
using System.Windows.Controls;

namespace WpfApp9
{
    public partial class AddDeviceWindow : Window
    {
        //public ItemConfiguration NewDeviceConfig { get; private set; }

        public AddDeviceWindow()
        {
            InitializeComponent();
        }

        //private void AddButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
        //        DeviceTypeComboBox.SelectedItem == null ||
        //        string.IsNullOrWhiteSpace(FtpAddressTextBox.Text) ||
        //        string.IsNullOrWhiteSpace(MacAddressTextBox.Text) ||
        //        string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
        //    {
        //        MessageBox.Show("필수 필드를 모두 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }


        //    NewDeviceConfig = new ItemConfiguration
        //    {
        //        Name = NameTextBox.Text,
        //        DeviceType = ((ComboBoxItem)DeviceTypeComboBox.SelectedItem).Content.ToString(),
        //        FtpAddress = FtpAddressTextBox.Text,
        //        MacAddress = MacAddressTextBox.Text,
        //        IpAddress = IpAddressTextBox.Text,
        //        Description = DescriptionTextBox.Text,
        //        IsOn = InitialStateCheckBox.IsChecked ?? false,
        //        Row = 0,
        //        Column = 0,
        //        ZIndex = 1,
        //        VncPw = "1015"
        //    };

        //    DialogResult = true;
        //}

        //private void CancelButton_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}
    }
}