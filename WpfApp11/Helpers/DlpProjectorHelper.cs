using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using WpfApp11.Helpers;

public class DlpProjectorHelper
{
    private const int ProjectorPort = 4352;
    private const int MaxRetries = 1;
    private const int TimeoutMilliseconds = 5000; // 5 seconds timeout
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
        string response = await SendCommandWithRetryAsync(projectorIp, Commands.PowerOn);
        return ParsePowerCommandResponse(response);
    }

    public async Task<bool> SendPowerOffCommandAsync(string projectorIp)
    {
        string response = await SendCommandWithRetryAsync(projectorIp, Commands.PowerOff);
        return ParsePowerCommandResponse(response);
    }

    public async Task<string> GetPowerStatusAsync(string projectorIp)
    {
        string response = await SendCommandWithRetryAsync(projectorIp, Commands.PowerStatus);
        return ParsePowerStatus(response);
    }

    private async Task<string> SendCommandWithRetryAsync(string projectorIp, string hexCommand)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await SendCommandAsync(projectorIp, hexCommand);
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    //Logger.Log2($"DLP Projector command failed after {MaxRetries + 1} attempts: {ex.Message}");
                    throw; // Re-throw the exception after all retries have failed
                }
                await Task.Delay(500); // Wait before retrying
            }
        }
        return string.Empty; // This line should never be reached due to the throw above
    }

    private async Task<string> SendCommandAsync(string projectorIp, string hexCommand)
    {
        using (var client = new TcpClient())
        {
            using (var cts = new System.Threading.CancellationTokenSource(TimeoutMilliseconds))
            {
                try
                {
                    var connectTask = client.ConnectAsync(projectorIp, ProjectorPort);
                    var timeoutTask = Task.Delay(TimeoutMilliseconds, cts.Token);

                    if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                    {
                        throw new TimeoutException("Connection attempt timed out");
                    }

                    using (var stream = client.GetStream())
                    {
                        byte[] commandBytes = StringToByteArray(hexCommand);
                        await stream.WriteAsync(commandBytes, 0, commandBytes.Length, cts.Token);

                        // Read the response
                        byte[] buffer = new byte[1024];
                        var readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                        timeoutTask = Task.Delay(TimeoutMilliseconds, cts.Token);

                        if (await Task.WhenAny(readTask, timeoutTask) == timeoutTask)
                        {
                            throw new TimeoutException("Reading response timed out");
                        }

                        int bytesRead = await readTask;
                        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException("Operation timed out");
                }
            }
        }
    }

    private bool ParsePowerCommandResponse(string response)
    {
        // 프로젝터의 실제 응답 형식에 따라 이 부분을 구현해야 합니다.
        // 예를 들어, 성공적인 응답이 "OK"를 포함한다고 가정합니다.
        return response.Contains("OK");
    }

    private string ParsePowerStatus(string response)
    {
        // 프로젝터의 실제 응답 형식에 따라 이 부분을 구현해야 합니다.
        if (response.Contains("ON"))
        {
            return "Powered On";
        }
        else if (response.Contains("OFF"))
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