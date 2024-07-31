using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using FluentFTP;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using WpfApp11;
using System.Drawing;
using System.Windows.Interop;
using System.Net;
using System.Collections.Generic;

namespace WpfApp9
{
    public partial class FileExplorerControl : UserControl
    {
        public event EventHandler CloseRequested;

        public class FileSystemItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string FullPath { get; set; }
            public BitmapSource IconSource { get; set; }
        }

        private ObservableCollection<FileSystemItem> _leftItems;
        private ObservableCollection<FileSystemItem> _rightItems;
        private FtpClient _ftpClient;
        private string _currentLocalPath = @"C:\dw";
        private string _currentFtpPath = "/";
        string ftp_port = "21";
        //private string _currentFtpPath = "ftp://192.168.0.5";


        bool _isconnect = false;
        MainWindow main;
        public FileExplorerControl()
        {
            InitializeComponent();

            main = Application.Current.MainWindow as MainWindow;


            _leftItems = new ObservableCollection<FileSystemItem>();
            _rightItems = new ObservableCollection<FileSystemItem>();
            LeftFileListView.ItemsSource = _leftItems;
            RightFileListView.ItemsSource = _rightItems;
            LeftFileListView.MouseDoubleClick += LeftFileListView_MouseDoubleClick;
            RightFileListView.MouseDoubleClick += RightFileListView_MouseDoubleClick;
        }

        public void Initialize(string ftpAddress)
        {


            InitializeFtpConnection(ftpAddress);
            LoadLocalDirectory(_currentLocalPath);
            LoadFtpDirectory(_currentFtpPath);
        }

        private void InitializeFtpConnection(string ftpAddress)
        {

            var target_ftp = "ftp://" + ftpAddress +":" + ftp_port;
            try
            {
                //원래====================================================================
                Uri ftpUri = new Uri(target_ftp);
                string host = ftpUri.Host;
                //string username = ftpUri.UserInfo.Split(':')[0];
                //string password = ftpUri.UserInfo.Split(':')[1];
                string username = "ftp1234";
                string password = "1234";

                //string username = "engium";
                //string password = "1";
                //====================================================================





                //임시====================================================================
                //Uri ftpUri = new Uri("ftp://121.131.142.148:12923");
                //string host = ftpUri.Host;
                //string username = "engium";
                //string password = "1";

                //====================================================================





                _ftpClient = new FtpClient(host, username, password);
                

                _ftpClient.Connect();

                _isconnect = true;






















                //try
                //{
                //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                //    request.Method = WebRequestMethods.Ftp.ListDirectory;

                //    // 타임아웃 설정 (밀리초 단위)
                //    request.Timeout = 5000; // 10초
                //    request.ReadWriteTimeout = 5000; // 10초

                //    request.Credentials = new NetworkCredential(username, password);

                //    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                //    {
                //        Console.WriteLine($"Response status: {response.StatusDescription}");
                //    }
                //}
                //catch (WebException ex)
                //{
                //    Console.WriteLine($"Error: {ex.Message}");
                //}
























            }
            catch (Exception ex)
            {
                _isconnect = false;
                MessageBox.Show("FTP 연결 오류: " + ex.Message);

              
            }
        }

        private void LoadLocalDirectory(string path)
        {

            if (_isconnect)
            {
                try
                {
                    _leftItems.Clear();
                    DirectoryInfo di = new DirectoryInfo(path);

                    if (di.Parent != null)
                    {
                        _leftItems.Add(new FileSystemItem
                        {
                            Name = "..",
                            Type = "Parent Directory",
                            FullPath = di.Parent.FullName,
                            IconSource = GetIconForFileType("folder")
                        });
                    }

                    foreach (var directory in di.GetDirectories())
                    {
                        _leftItems.Add(new FileSystemItem
                        {
                            Name = directory.Name,
                            Type = "Folder",
                            FullPath = directory.FullName,
                            IconSource = GetIconForFileType("folder")
                        });
                    }

                    foreach (var file in di.GetFiles())
                    {
                        _leftItems.Add(new FileSystemItem
                        {
                            Name = file.Name,
                            Type = "File",
                            FullPath = file.FullName,
                            IconSource = GetIconForFileType(file.FullName)
                        });



                    }

                    _currentLocalPath = path;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading local directory: " + ex.Message);
                }
            }
           
        }

        private void LoadFtpDirectory(string path)
        {

            if (_isconnect)
            {
                try
                {

                    _rightItems.Clear();
                    if (path != "/")
                    {
                        _rightItems.Add(new FileSystemItem
                        {
                            Name = "..",
                            Type = "Parent Directory",
                            FullPath = Path.GetDirectoryName(path).Replace('\\', '/'),
                            IconSource = GetIconForFileType("folder")
                        });
                    }

                    var listing = _ftpClient.GetListing(path);
                    var sortedListing = listing
                        .OrderByDescending(item => item.Type == FtpObjectType.Directory)
                        .ThenBy(item => item.Name);

                    foreach (var item in sortedListing)
                    {

                        FileSystemItem fs = new FileSystemItem();
                        fs.Name = item.Name;
                        fs.Type = item.Type == FtpObjectType.Directory ? "Folder" : "File";
                        fs.FullPath = item.FullName;



                        string extension = Path.GetExtension(fs.FullPath).ToLower();


                        if (extension == "")
                        {
                            fs.IconSource = GetIconForFileType(item.Type == FtpObjectType.Directory ? "folder" : item.Name);
                        }
                        else if (extension == ".exe")
                        {
                            System.Drawing.Icon errorIcon = SystemIcons.Application;
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                errorIcon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                            fs.IconSource = bitmapSource;
                        }
                        else if (extension == ".txt")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_txt.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;



                            //================================================================================================================

                            //Icon icon = GetIconFromDll(@"C:\Windows\System32\shell32.dll", 97); // 예시로 4번째 아이콘 가져오기

                            //// Icon을 BitmapSource로 변환
                            //BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            //    icon.Handle,
                            //    Int32Rect.Empty,
                            //    BitmapSizeOptions.FromEmptyOptions());

                            //// 이미지를 표시하는 Image 컨트롤에 설정
                            //fs.IconSource = bitmapSource;

                            //// Icon 핸들 해제
                            //icon.Dispose();


                            //================================================================================================================


                            //Icon shellIcon = GetShellIcon(5); // 예시로 Shell32.dll의 5번째 아이콘 가져오기

                            //// Icon을 BitmapSource로 변환
                            //BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            //    shellIcon.Handle,
                            //    Int32Rect.Empty,
                            //    BitmapSizeOptions.FromEmptyOptions());

                            //// 이미지를 표시하는 Image 컨트롤에 설정
                            //fs.IconSource = bitmapSource;

                            //// Icon 핸들 해제
                            //shellIcon.Dispose();

                            //================================================================================================================


                        }
                        else if (extension == ".bat")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_bat.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }
                        else
                        {
                            System.Drawing.Icon errorIcon = SystemIcons.Application;
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                errorIcon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                            fs.IconSource = bitmapSource;

                            //BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/time.png", UriKind.Relative));
                        }



                        _rightItems.Add(fs);
                    }

                    _currentFtpPath = path;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading FTP directory: " + ex.Message);
                }
            }
            else
            {
                main.OverlayGrid.Visibility = Visibility.Collapsed;
            }
        }





        //=======================================

        private Icon GetShellIcon(int index)
        {
            IntPtr hIcon;
            hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, @"C:\Windows\System32\shell32.dll", index);
            if (hIcon != IntPtr.Zero)
            {
                return Icon.FromHandle(hIcon);
            }
            return null;
        }

        // 네이티브 메서드 선언
        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public extern static IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);
        }





        private Icon GetIconFromDll(string dllPath, int iconIndex)
        {
            IntPtr hIcon = IntPtr.Zero;
            try
            {
                hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, dllPath, iconIndex);
                if (hIcon != IntPtr.Zero)
                {
                    return Icon.FromHandle(hIcon);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting icon: {ex.Message}");
            }
            return null;
        }

        //===================================================












        private void LeftFileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListView)sender).SelectedItem as FileSystemItem;
            if (item != null && (item.Type == "Folder" || item.Type == "Parent Directory"))
            {
                LoadLocalDirectory(item.FullPath);
                e.Handled = true;
            }
        }

        private void RightFileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListView)sender).SelectedItem as FileSystemItem;
            if (item != null && (item.Type == "Folder" || item.Type == "Parent Directory"))
            {
                
                    LoadFtpDirectory(item.FullPath);
                    e.Handled = true;
                
            }
        }
        private long CalculateTotalSize(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            else if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                .Sum(filePath => new FileInfo(filePath).Length);
            }
            return 0;
        }

        private class TransferProgress
        {
            public long TransferredSize { get; set; }
        }
        private void LocalCreateFolder_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new InputDialog("새 폴더 이름을 입력하세요.", "New Folder");
            if (inputDialog.ShowDialog() == true)
            {
                string folderName = inputDialog.Answer;
                if (!string.IsNullOrEmpty(folderName))
                {
                    string newFolderPath = Path.Combine(_currentLocalPath, folderName);
                    Directory.CreateDirectory(newFolderPath);
                    LoadLocalDirectory(_currentLocalPath);
                }
            }
        }

        private void LocalEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = LeftFileListView.SelectedItem as FileSystemItem;
            if (selectedItem != null)
            {
                InputDialog inputDialog = new InputDialog("새 이름을 입력하세요.", selectedItem.Name);
                if (inputDialog.ShowDialog() == true)
                {
                    string newName = inputDialog.Answer;
                    if (!string.IsNullOrEmpty(newName))
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(selectedItem.FullPath), newName);
                        if (selectedItem.Type == "Folder")
                            Directory.Move(selectedItem.FullPath, newPath);
                        else
                            File.Move(selectedItem.FullPath, newPath);
                        LoadLocalDirectory(_currentLocalPath);
                    }
                }
            }
        }

        private void LocalDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"선택한 {selectedItems.Count}개의 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var item in selectedItems)
                    {
                        if (item.Type == "Folder")
                        {
                            Directory.Delete(item.FullPath, true);
                        }
                        else
                        {
                            File.Delete(item.FullPath);
                        }
                    }
                    MessageBox.Show("선택한 항목이 삭제되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"삭제 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadLocalDirectory(_currentLocalPath);
                }
            }
        }

        private void FtpCreateFolder_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new InputDialog("새 폴더 이름을 입력하세요.", "New Folder");
            if (inputDialog.ShowDialog() == true)
            {
                string folderName = inputDialog.Answer;
                if (!string.IsNullOrEmpty(folderName))
                {
                    string newFolderPath = _currentFtpPath + "/" + folderName;
                    _ftpClient.CreateDirectory(newFolderPath);
                    LoadFtpDirectory(_currentFtpPath);
                }
            }
        }

        private void FtpEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = RightFileListView.SelectedItem as FileSystemItem;
            if (selectedItem != null)
            {
                InputDialog inputDialog = new InputDialog("새 이름을 입력하세요.", selectedItem.Name);
                if (inputDialog.ShowDialog() == true)
                {
                    string newName = inputDialog.Answer;
                    if (!string.IsNullOrEmpty(newName))
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(selectedItem.FullPath), newName).Replace('\\', '/');
                        _ftpClient.Rename(selectedItem.FullPath, newPath);
                        LoadFtpDirectory(_currentFtpPath);
                    }
                }
            }
        }

        private void FtpDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"선택한 {selectedItems.Count}개의 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var item in selectedItems)
                    {
                        if (item.Type == "Folder")
                        {
                            _ftpClient.DeleteDirectory(item.FullPath, FtpListOption.Recursive);
                        }
                        else
                        {
                            _ftpClient.DeleteFile(item.FullPath);
                        }
                    }
                    MessageBox.Show("선택한 항목이 삭제되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"삭제 중");
                }
                finally
                {
                    LoadFtpDirectory(_currentFtpPath);
                }
            }
        }

       
        private async void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftFileListView.SelectedItems.Cast<FileSystemItem>().ToList();

            if (!selectedItems.Any())
            {
                MessageBox.Show("전송할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<string> filename = new List<string>();
            var listing = _ftpClient.GetListing(_currentFtpPath);
            var sortedListing = listing
                .OrderByDescending(item => item.Type == FtpObjectType.Directory)
                .ThenBy(item => item.Name);

            foreach (var item in sortedListing)
            {
                filename.Add(item.Name);
            }

            bool file_exit = false;

            for (int i = 0; i < filename.Count; i++)
            {
                for (int j = 0; j < selectedItems.Count; j++)
                {
                    if (filename[i] == selectedItems[j].Name)
                    {
                        file_exit = true;
                    }
                }
            }


            if (file_exit)
            {
                MessageBoxResult result = MessageBox.Show(
               "같은 이름의 파일이 있습니다. 덮어씌우시겠습니까?",   // 메시지
               "확인",                      // 제목
               MessageBoxButton.YesNo,      // 버튼 종류
               MessageBoxImage.Question     // 아이콘 종류
           );
             
                if (result == MessageBoxResult.Yes)
                {
                    // 사용자가 'Yes'를 클릭했을 때의 처리
                    await file_send_to_ftp(selectedItems);
                }
            }
            else
            {
                await file_send_to_ftp(selectedItems);
            }
        }

        private async Task file_send_to_ftp(List<FileSystemItem> selectedItems)
        {
            Transferpopup.Visibility = Visibility.Visible;
            TransferProgressBar.Visibility = Visibility.Visible;

            TransferProgressBar.Value = 0;
            TransferProgressText.Text = "0%";

            long totalSize = selectedItems.Sum(item => CalculateTotalSize(item.FullPath));
            var progress = new TransferProgress();

            try
            {
                foreach (var item in selectedItems)
                {
                    await TransferItemAsync(item, _currentFtpPath, totalSize, progress);
                }

                if (check_finish == true)
                {
                    MessageBox.Show("전송이 완료되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Transferpopup.Visibility = Visibility.Collapsed;
                TransferProgressBar.Visibility = Visibility.Collapsed;
                TransferProgressText.Text = "";
                LoadFtpDirectory(_currentFtpPath);
            }
        }

        private async void FtpToLocalTransferButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (!selectedItems.Any())
            {
                MessageBox.Show("전송할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Transferpopup.Visibility = Visibility.Visible;
            TransferProgressBar.Visibility = Visibility.Visible;
            TransferProgressBar.Value = 0;
            TransferProgressText.Text = "0%";

            long totalSize = await Task.Run(() => selectedItems.Sum(item => _ftpClient.GetFileSize(item.FullPath)));
            var progress = new TransferProgress();

            try
            {
                foreach (var item in selectedItems)
                {
                    await FtpToLocalTransferItemAsync(item, _currentLocalPath, totalSize, progress);
                }


                if (check_finish == true)
                {
                    MessageBox.Show("전송이 완료되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Transferpopup.Visibility = Visibility.Collapsed;
                TransferProgressBar.Visibility = Visibility.Collapsed;
                TransferProgressText.Text = "";
                LoadLocalDirectory(_currentLocalPath);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }


        bool check_finish = false;
        private async Task TransferItemAsync(FileSystemItem item, string destPath, long totalSize, TransferProgress progress)
        {
            try
            {

                if (item.Type == "File")
                {
                    //var ftpPath = Path.Combine(destPath, item.Name).Replace("\\", "/");
                    //var result = await Task.Run(() => _ftpClient.UploadFile(item.FullPath, ftpPath, FtpRemoteExists.Overwrite, true, FtpVerify.None, ftpProgress =>
                    //{
                    //    progress.TransferredSize = ftpProgress.TransferredBytes;
                    //    UpdateProgress(progress.TransferredSize, totalSize);

                    //}));


                    //if (result != FtpStatus.Success)
                    //{
                    //    throw new Exception($"파일 '{item.Name}' 전송 실패");
                    //}



                    try
                    {
                        var ftpPath = Path.Combine(destPath, item.Name).Replace("\\", "/");
                        await Task.Run(() =>
                        {
                            try
                            {
                                _ftpClient.UploadFile(item.FullPath, ftpPath, FtpRemoteExists.Overwrite, true, FtpVerify.None, ftpProgress =>
                                {
                                    check_finish = true;
                                    progress.TransferredSize = ftpProgress.TransferredBytes;
                                    UpdateProgress(progress.TransferredSize, totalSize);
                                });
                            }
                            catch (System.Net.Sockets.SocketException ex)
                            {
                                Console.WriteLine($"SocketException 발생: {ex.Message}");
                                check_finish = false;
                                throw; // 예외를 호출자에게 전달
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"업로드 중 오류 발생: {ex.Message}");
                                //MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

                               
                                check_finish = false;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"작업 실행 중 오류 발생: {ex.Message}");
                        check_finish = false;
                    }












                    //try
                    //{
                    //    await Task.Run(() =>
                    //    {
                    //        try
                    //        {
                    //            // FTP 업로드 작업 수행
                    //            _ftpClient.UploadFile(item.FullPath, ftpPath, FtpRemoteExists.Overwrite, true, FtpVerify.None, ftpProgress =>
                    //            {
                    //                progress.TransferredSize = ftpProgress.TransferredBytes;
                    //                UpdateProgress(progress.TransferredSize, totalSize);
                    //            });



                    //            //if (result != FtpStatus.Success)
                    //            //{
                    //            //    throw new Exception($"파일 '{item.Name}' 전송 실패");
                    //            //}
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            // FTP 업로드 중 발생한 예외 처리
                    //            Console.WriteLine($"FTP 업로드 중 오류 발생: {ex.Message}");
                    //            // 예외를 다시 던져서 호출자에게 전달할 수 있습니다.
                    //            //throw;
                    //        }
                    //    });
                    //}
                    //catch (Exception ex)
                    //{
                    //    // Task.Run 내에서 발생한 예외 처리
                    //    Console.WriteLine($"작업 실행 중 오류 발생: {ex.Message}");
                    //}

















                }
                else if (item.Type == "Folder")
                {
                    var ftpFolderPath = Path.Combine(destPath, item.Name).Replace("\\", "/");
                    await Task.Run(() => _ftpClient.CreateDirectory(ftpFolderPath));

                    foreach (var file in Directory.GetFiles(item.FullPath))
                    {
                        var fileItem = new FileSystemItem
                        {
                            Name = Path.GetFileName(file),
                            Type = "File",
                            FullPath = file
                        };
                        await TransferItemAsync(fileItem, ftpFolderPath, totalSize, progress);
                    }

                    foreach (var dir in Directory.GetDirectories(item.FullPath))
                    {
                        var dirItem = new FileSystemItem
                        {
                            Name = Path.GetFileName(dir),
                            Type = "Folder",
                            FullPath = dir
                        };
                        await TransferItemAsync(dirItem, ftpFolderPath, totalSize, progress);
                    }
                }
            }
            catch(Exception e)
            {

            }
        }






        private async Task FtpToLocalTransferItemAsync(FileSystemItem item, string destPath, long totalSize, TransferProgress progress)
        {
            if (item.Type == "File")
            {
               //  var localPath = Path.Combine(destPath, item.Name);
                //var result = await Task.Run(() => _ftpClient.DownloadFile(localPath, item.FullPath, FtpLocalExists.Overwrite, FtpVerify.None, ftpProgress =>
                //{
                //    progress.TransferredSize += ftpProgress.TransferredBytes;
                //    UpdateProgress(progress.TransferredSize, totalSize);
                //}));

                //if (result != FtpStatus.Success)
                //{
                //    throw new Exception($"파일 '{item.Name}' 전송 실패");
                //}






                try
                {
                    var localPath = Path.Combine(destPath, item.Name);
                    await Task.Run(() =>
                    {
                        try
                        {
                            _ftpClient.DownloadFile(localPath, item.FullPath, FtpLocalExists.Overwrite,  FtpVerify.None, ftpProgress =>
                            {
                                check_finish = true;
                                progress.TransferredSize = ftpProgress.TransferredBytes;
                                UpdateProgress(progress.TransferredSize, totalSize);
                            });
                        }
                        catch (System.Net.Sockets.SocketException ex)
                        {
                            Console.WriteLine($"SocketException 발생: {ex.Message}");
                            check_finish = false;
                            throw; // 예외를 호출자에게 전달
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"업로드 중 오류 발생: {ex.Message}");
                            //MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);


                            check_finish = false;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"작업 실행 중 오류 발생: {ex.Message}");
                    check_finish = false;
                }







            }
            else if (item.Type == "Folder")
            {
                var localFolderPath = Path.Combine(destPath, item.Name);
                Directory.CreateDirectory(localFolderPath);

                var ftpListing = await Task.Run(() => _ftpClient.GetListing(item.FullPath));
                foreach (var ftpItem in ftpListing)
                {
                    var subItem = new FileSystemItem
                    {
                        Name = ftpItem.Name,
                        Type = ftpItem.Type == FtpObjectType.File ? "File" : "Folder",
                        FullPath = ftpItem.FullName
                    };
                    await FtpToLocalTransferItemAsync(subItem, localFolderPath, totalSize, progress);
                }
            }
        }

        private void UpdateProgress(long transferredSize, long totalSize)
        {
            double percentage = (double)transferredSize / totalSize * 100;
            int roundedPercentage = (int)Math.Round(percentage);

            Dispatcher.Invoke(() =>
            {
                TransferProgressBar.Value = roundedPercentage;
                TransferProgressText.Text = $"{roundedPercentage}%";
            });
        }

        //private void CloseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    CloseRequested?.Invoke(this, EventArgs.Empty);
        //}

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;

        private BitmapSource GetIconForFileType(string fileType)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_SMALLICON;

            if (fileType == "folder")
            {
                SHGetFileInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            }
            else
            {
                SHGetFileInfo(fileType, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            }

            BitmapSource bitmapSource = null;
            if (shfi.hIcon != IntPtr.Zero)
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    shfi.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(shfi.hIcon);  // 리소스 해제
            }

            return bitmapSource;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}