using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class PJLinkHelper
{
    private static PJLinkHelper _instance = null;
    private static readonly object _padlock = new object();

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

    public string SendCommand(string ipAddress, int port, string command)
    {
        TcpClient client = null;
        NetworkStream stream = null;

        try
        {
            client = new TcpClient();
            client.ReceiveTimeout = 1000;
            client.SendTimeout = 1000;
            client.Connect(ipAddress, port);

            stream = client.GetStream();

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
        finally
        {
            stream?.Close();
            client?.Close();
        }
    }

    public bool PowerOn(string ipAddress, int port = 4352)
    {
        string response = SendCommand(ipAddress, port, "%1POWR 1");
        return response.Contains("OK");
    }

    public bool PowerOff(string ipAddress, int port = 4352)
    {
        string response = SendCommand(ipAddress, port, "%1POWR 0");
        return response.Contains("OK");
    }

    // 추가적인 PJLink 명령어 메소드들을 여기에 구현할 수 있습니다.
}