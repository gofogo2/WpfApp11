using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using WpfApp11.Helpers;
using System.Diagnostics;

public class DlpProjectorHelper
{
    private const int ProjectorPort = 4352;
    private const int TimeoutMilliseconds = 500;
    private static DlpProjectorHelper _instance;
    private static readonly object _lock = new object();

    private static class Commands
    {
        public const string PowerOn = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
        public const string PowerOff = "41542B53797374656D3D4F66660D";
        public const string PowerStatus = "41542B53797374656D3F41542B53797374656D3F0D";
    }

    private DlpProjectorHelper() { }

    public static DlpProjectorHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DlpProjectorHelper();
                    }
                }
            }
            return _instance;
        }
    }

    public async Task<bool> SendPowerOnCommandAsync(string projectorIp)
    {
        string response = await SendCommandAsync(projectorIp, Commands.PowerOn);
        Debug.WriteLine("PowerOn Response: " + response);
        return ParsePowerCommandResponse(response);
    }

    public async Task<bool> SendPowerOffCommandAsync(string projectorIp)
    {
        string response = await SendCommandAsyncOff(projectorIp, Commands.PowerOff);
        Debug.WriteLine("PowerOff Response: " + response);
        return ParsePowerCommandResponse(response);
    }

    public async Task<string> GetPowerStatusAsync(string projectorIp)
    {
        string response = await SendCommandAsync(projectorIp, Commands.PowerStatus);
        Debug.WriteLine("PowerStatus Response: " + response);
        return ParsePowerStatus(response);
    }

    private async Task<string> SendCommandAsync(string projectorIp, string hexCommand)
    {
        using (var client = new TcpClient())
        {
            try
            {
                await client.ConnectAsync(projectorIp, ProjectorPort);
                using (var stream = client.GetStream())
                {
                    stream.WriteTimeout = TimeoutMilliseconds;

                    byte[] commandBytes = StringToByteArray(hexCommand);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

                    byte[] buffer = new byte[1024];
                    using (var cts = new System.Threading.CancellationTokenSource(TimeoutMilliseconds))
                    {
                        Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                        Task timeoutTask = Task.Delay(TimeoutMilliseconds, cts.Token);

                        Task completedTask = await Task.WhenAny(readTask, timeoutTask);
                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException("Read operation timed out.");
                        }

                        int bytesRead = await readTask;
                        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SendCommandAsync: {ex.Message}");
                throw;
            }
            finally
            {
                client.Close();
            }
        }
    }


    private async Task<string> SendCommandAsyncOff(string projectorIp, string hexCommand)
    {
        using (var client = new TcpClient())
        {
            try
            {
                await client.ConnectAsync(projectorIp, ProjectorPort);
                using (var stream = client.GetStream())
                {
                    stream.WriteTimeout = TimeoutMilliseconds;

                    byte[] commandBytes = StringToByteArray(hexCommand);
                    await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

                    byte[] buffer = new byte[1024];
                    using (var cts = new System.Threading.CancellationTokenSource(TimeoutMilliseconds))
                    {
                        Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                        Task timeoutTask = Task.Delay(TimeoutMilliseconds, cts.Token);

                        Task completedTask = await Task.WhenAny(readTask, timeoutTask);
                        if (completedTask == timeoutTask)
                        {
                            //throw new TimeoutException("Read operation timed out.");
                            return "OK";
                        }

                        int bytesRead = await readTask;
                        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SendCommandAsync: {ex.Message}");
                throw;
            }
            finally
            {
                client.Close();
            }
        }
    }

    private bool ParsePowerCommandResponse(string response)
    {
        return response.ToUpper().Contains("OK");
    }

    private string ParsePowerStatus(string response)
    {
        if (response.ToUpper().Contains("ON"))
        {
            return "Powered On";
        }
        else if (response.ToUpper().Contains("OFF"))
        {
            return "Powered Off";
        }
        else
        {
            return "Unknown Status";
        }
    }

    private byte[] StringToByteArray(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}