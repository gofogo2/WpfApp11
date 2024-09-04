using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using WpfApp11.Helpers;

public class PJLinkHelper
{
    private static PJLinkHelper _instance = null;
    private static readonly object _padlock = new object();
    private const int DefaultTimeout = 500;
    private const int MaxRetries = 1;

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

    public async Task<bool> PowerOnAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = await SendCommandWithRetryAsync(ipAddress, port, "%1POWR 1", timeout);
        return response.Contains("OK");
    }

    public async Task<bool> PowerOffAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = await SendCommandWithRetryAsync(ipAddress, port, "%1POWR 0", timeout);
        return response.Contains("OK");
    }

    public async Task<PowerStatus> GetPowerStatusAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = await SendCommandWithRetryAsync(ipAddress, port, "%1POWR ?", timeout);
        if (response.Contains("OK"))
        {
            string status = response.Substring(response.Length - 1);
            switch (status)
            {
                case "0": return PowerStatus.StandBy;
                case "1": return PowerStatus.PoweredOn;
                case "2": return PowerStatus.Cooling;
                case "3": return PowerStatus.WarmUp;
                default: return PowerStatus.Unknown;
            }
        }
        return PowerStatus.Unknown;
    }

    private async Task<string> SendCommandWithRetryAsync(string ipAddress, int port, string command, int timeout)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await SendCommandAsync(ipAddress, port, command, timeout);
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    //Logger.Log2($"PJLink command failed after {MaxRetries + 1} attempts: {ex.Message}");
                    return string.Empty;
                }
                await Task.Delay(500); // Wait before retrying
            }
        }
        return string.Empty;
    }

    private async Task<string> SendCommandAsync(string ipAddress, int port, string command, int timeout)
    {
        using (var client = new TcpClient())
        {
            await client.ConnectAsync(ipAddress, port);
            client.ReceiveTimeout = timeout;
            client.SendTimeout = timeout;
            using (var stream = client.GetStream())
            {
                byte[] data = Encoding.ASCII.GetBytes(command + "\r");
                await stream.WriteAsync(data, 0, data.Length);
                using (var ms = new MemoryStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, bytesRead);
                    } while (stream.DataAvailable);
                    return Encoding.ASCII.GetString(ms.ToArray());
                }
            }
        }
    }
}

public enum PowerStatus
{
    Unknown,
    StandBy,
    PoweredOn,
    Cooling,
    WarmUp
}