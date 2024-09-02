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
        return await SendCommandWithRetryAsync(ipAddress, port, "%1POWR 1", timeout);
    }

    public async Task<bool> PowerOffAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        return await SendCommandWithRetryAsync(ipAddress, port, "%1POWR 0", timeout);
    }

    private async Task<bool> SendCommandWithRetryAsync(string ipAddress, int port, string command, int timeout)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                string response = await SendCommandAsync(ipAddress, port, command, timeout);
                return response.Contains("OK");
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    Logger.Log2($"PJLink command failed after {MaxRetries + 1} attempts: {ex.Message}");
                    return false;
                }
                await Task.Delay(500); // Wait before retrying
            }
        }
        return false;
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