using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

public class PJLinkHelper
{
    private static PJLinkHelper _instance = null;
    private static readonly object _padlock = new object();
    private const int DefaultTimeout = 3000; // 3000ms 타임아웃

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
        var result = await GetPowerStatusAsync(ipAddress, port, timeout);
        Debug.WriteLine(result);
        bool firstAttempt = await SendCommandAsync(ipAddress, port, "%1POWR 1", timeout) == "OK";
        if (firstAttempt) return true;

        bool secondAttempt = await SendCommandAsync(ipAddress, port, "%1POWR 1", timeout) == "OK";
        return firstAttempt || secondAttempt;
    }

    public async Task<bool> PowerOffAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        var result = await GetPowerStatusAsync(ipAddress, port, timeout);
        Debug.WriteLine(result);
        bool firstAttempt = await SendCommandAsync(ipAddress, port, "%1POWR 0", timeout) == "OK";
        if (firstAttempt) return true;

        bool secondAttempt = await SendCommandAsync(ipAddress, port, "%1POWR 0", timeout) == "OK";
        return firstAttempt || secondAttempt;
    }

    public async Task<PowerStatus> GetPowerStatusAsync(string ipAddress, int port = 4352, int timeout = DefaultTimeout)
    {
        string response = await SendCommandAsync(ipAddress, port, "%1POWR ?", timeout);
        if (response.Contains("="))
        {
            string status = response.Substring(response.IndexOf("=") + 1).Trim();
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

    private async Task<string> SendCommandAsync(string ipAddress, int port, string command, int timeout)
    {
        using (var client = new TcpClient())
        {
            try
            {
                await client.ConnectAsync(ipAddress, port);
                client.ReceiveTimeout = timeout;
                client.SendTimeout = timeout;

                using (var stream = client.GetStream())
                {
                    // 초기 PJLink 응답 읽기
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string initialResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    if (!initialResponse.StartsWith("PJLINK 0"))
                    {
                        throw new Exception($"Unexpected initial PJLink response: {initialResponse}");
                    }

                    // 명령 전송
                    byte[] data = Encoding.ASCII.GetBytes(command + "\r");
                    await stream.WriteAsync(data, 0, data.Length);

                    // 응답 읽기
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, bytesRead);
                        } while (stream.DataAvailable);

                        return Encoding.ASCII.GetString(ms.ToArray()).Trim();
                    }
                }
            }
            finally
            {
                client.Close();
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