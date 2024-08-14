using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;

public class PJLinkHelper
{
    private static PJLinkHelper _instance = null;
    private static readonly object _padlock = new object();
    private const int DefaultTimeout = 500; // 기본 타임아웃 5초

    private PJLinkHelper() { }

    public static PJLinkHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new PJLinkHelper();
                    }
                }
            }
            return _instance;
        }
    }

    public string SendCommand(string ipAddress, int port, string command, int timeout = DefaultTimeout)
    {
        using (TcpClient client = new TcpClient())
        {
            try
            {
                var connectTask = client.ConnectAsync(ipAddress, port);
                if (!connectTask.Wait(timeout))
                {
                    throw new TimeoutException("연결 시도 시간이 초과되었습니다.");
                }

                client.ReceiveTimeout = timeout;
                client.SendTimeout = timeout;

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes(command + "\r");
                    stream.Write(data, 0, data.Length);

                    byte[] responseBuffer = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int bytesRead;
                        do
                        {
                            bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                            ms.Write(responseBuffer, 0, bytesRead);
                        } while (stream.DataAvailable);

                        return Encoding.ASCII.GetString(ms.ToArray());
                    }
                }
            }
            catch (TimeoutException te)
            {
                return $"Timeout error: {te.Message}";
            }
            catch (SocketException se)
            {
                return $"Socket error: {se.Message}";
            }
            catch (IOException ioe)
            {
                return $"IO error: {ioe.Message}";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }
    }

    public async Task<string> SendCommandAsync(string ipAddress, int port, string command, int timeout = DefaultTimeout)
    {
        using (TcpClient client = new TcpClient())
        {
            try
            {
                var connectTask = client.ConnectAsync(ipAddress, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) != connectTask)
                {
                    throw new TimeoutException("연결 시도 시간이 초과되었습니다.");
                }

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes(command + "\r");
                    await stream.WriteAsync(data, 0, data.Length);

                    byte[] responseBuffer = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int bytesRead;
                        var readTask = stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                        if (await Task.WhenAny(readTask, Task.Delay(timeout)) != readTask)
                        {
                            throw new TimeoutException("응답 읽기 시간이 초과되었습니다.");
                        }
                        bytesRead = await readTask;
                        ms.Write(responseBuffer, 0, bytesRead);

                        while (stream.DataAvailable)
                        {
                            readTask = stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                            if (await Task.WhenAny(readTask, Task.Delay(timeout)) != readTask)
                            {
                                throw new TimeoutException("응답 읽기 시간이 초과되었습니다.");
                            }
                            bytesRead = await readTask;
                            ms.Write(responseBuffer, 0, bytesRead);
                        }

                        return Encoding.ASCII.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }

    public async Task<bool> PowerOnAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = await SendCommandAsync(ipAddress, port, "%1POWR 1", timeout);
        return response.Contains("OK");
    }

    public async Task<bool> PowerOffAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = await SendCommandAsync(ipAddress, port, "%1POWR 0", timeout);
        return response.Contains("OK");
    }

    public bool PowerOn(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = SendCommand(ipAddress, port, "%1POWR 1", timeout);
        return response.Contains("OK");
    }

    public bool PowerOff(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = SendCommand(ipAddress, port, "%1POWR 0", timeout);
        return response.Contains("OK");
    }

    // 추가적인 PJLink 명령어 메소드들을 여기에 구현할 수 있습니다.
}