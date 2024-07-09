using System.Collections.Generic;
using System.Windows;

namespace WpfApp9
{
    public partial class RemoveDeviceWindow : Window
    {
        public ItemConfiguration SelectedConfig { get; private set; }

        public RemoveDeviceWindow(List<ItemConfiguration> devices)
        {
            InitializeComponent();
            DevicesListBox.ItemsSource = devices;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DevicesListBox.SelectedItem == null)
            {
                MessageBox.Show("삭제할 기기를 선택해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedConfig = (ItemConfiguration)DevicesListBox.SelectedItem;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}