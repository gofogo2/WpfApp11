using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using WpfApp11.Helpers;

public class DlpProjectorHelper
{
    private const int ProjectorPort = 4352;
    private const int MaxRetries = 1;
    private static DlpProjectorHelper _instance;
    private static readonly object _lock = new object();

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

    public async Task<bool> SendPowerOnCommandToDLPProjector(string projectorIp)
    {
        string hexCommand = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
        return await SendCommandWithRetryToDLPProjector(projectorIp, hexCommand);
    }

    public async Task<bool> SendPowerOffCommandToDLPProjector(string projectorIp)
    {
        string hexCommand = "41542B53797374656D3D4F66660D";
        return await SendCommandWithRetryToDLPProjector(projectorIp, hexCommand);
    }

    private async Task<bool> SendCommandWithRetryToDLPProjector(string projectorIp, string hexCommand)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await SendCommandToDLPProjector(projectorIp, hexCommand);
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    Logger.Log2($"DLP Projector command failed after {MaxRetries + 1} attempts: {ex.Message}");
                    return false;
                }
                await Task.Delay(500); // Wait before retrying
            }
        }
        return false;
    }

    private async Task<bool> SendCommandToDLPProjector(string projectorIp, string hexCommand)
    {
        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync(projectorIp, ProjectorPort);
            using (NetworkStream stream = client.GetStream())
            {
                byte[] commandBytes = StringToByteArray(hexCommand);
                await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
            }
        }
        return true;
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