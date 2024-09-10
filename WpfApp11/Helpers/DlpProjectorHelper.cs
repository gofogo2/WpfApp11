using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

public class DlpProjectorHelper : IDisposable
{
    private const int ProjectorPort = 4352;
    private const int TimeoutMilliseconds = 5000;
    private readonly string projectorIp;
    private TcpClient client;
    private bool isDisposed = false;

    private static class Commands
    {
        public const string PowerOn = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
        public const string PowerOff = "41542B53797374656D3D4F66660D";
        public const string PowerStatus = "41542B53797374656D3F41542B53797374656D3F0D";
    }

    public DlpProjectorHelper(string projectorIp)
    {
        this.projectorIp = projectorIp;
    }

    private async Task EnsureConnectedAsync()
    {
        if (client == null || !client.Connected)
        {
            if (client != null)
            {
                client.Dispose();
            }
            client = new TcpClient();
            await client.ConnectAsync(projectorIp, ProjectorPort);
        }
    }

    public async Task<bool> SendPowerOnCommandAsync()
    {
        return await SendPowerCommandAsync(Commands.PowerOn, "PowerOn");
    }

    public async Task<bool> SendPowerOffCommandAsync()
    {
        return await SendPowerCommandAsync(Commands.PowerOff, "PowerOff");
    }

    public async Task<string> GetPowerStatusAsync()
    {
        return await SendStatusCommandAsync(Commands.PowerStatus, "PowerStatus");
    }

    private async Task<bool> SendPowerCommandAsync(string command, string operationType)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(DlpProjectorHelper));
        }

        try
        {
            await EnsureConnectedAsync();
            string response = await SendCommandAsync(command);
            Debug.WriteLine($"{operationType} Response: {response}");
            return ParsePowerCommandResponse(response);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in {operationType}: {ex.Message}");
            return false;
        }
    }

    private async Task<string> SendStatusCommandAsync(string command, string operationType)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(DlpProjectorHelper));
        }

        try
        {
            await EnsureConnectedAsync();
            string response = await SendCommandAsync(command);
            Debug.WriteLine($"{operationType} Response: {response}");
            return ParsePowerStatus(response);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in {operationType}: {ex.Message}");
            return "Error";
        }
    }

    private async Task<string> SendCommandAsync(string hexCommand)
    {
        using (var stream = client.GetStream())
        {
            stream.WriteTimeout = TimeoutMilliseconds;
            byte[] commandBytes = StringToByteArray(hexCommand);
            await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

            byte[] buffer = new byte[1024];
            using (var cts = new System.Threading.CancellationTokenSource(TimeoutMilliseconds))
            {
                Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (await Task.WhenAny(readTask, Task.Delay(TimeoutMilliseconds, cts.Token)) != readTask)
                {
                    throw new TimeoutException("Read operation timed out.");
                }

                int bytesRead = await readTask;
                return Encoding.ASCII.GetString(buffer, 0, bytesRead);
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

    public void Dispose()
    {
        if (!isDisposed)
        {
            client?.Dispose();
            isDisposed = true;
        }
    }
}