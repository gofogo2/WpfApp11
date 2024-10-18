using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfApp11.Helpers.ByteConverter;

namespace WpfApp11.Helpers
{
    class KIA360ProtocolHelper
    {
       
        public KIA360ProtocolHelper()
        {
       
        }

        public void Start()
        {
            ProtocolUdpHelper.Instance.PacketReceived -= Instance_PacketReceived; ;
            ProtocolUdpHelper.Instance.PacketReceived += Instance_PacketReceived; ;
        }
        public void Stop()
        {
            ProtocolUdpHelper.Instance.PacketReceived -= Instance_PacketReceived; ;
        }

        public void Instance_PacketReceived(string code)
        {
            Logger.Log2(code);

            code = code.ToUpper().Trim();

            switch (code)
            {

                case "JOURNEY_PLAY":
                case "JOURNEY_PAUSE":
                case "JOURNEY_RESTART":
                    Logger.Log2($"SEND : IP 192.168.1.210, code {code}, port 8020");
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.1.210", 8020);
                    break;

                case "MANI_PLAY1":
                case "MANI_PAUSE1":
                case "MANI_RESTART1":
                case "MANI_PLAY2":
                case "MANI_PAUSE2":
                case "MANI_RESTART2":
                    Logger.Log2($"SEND : IP 192.168.1.130, code {code}, port 8020");
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.1.130", 8020);
                    break;
                case "MOMENT_PLAY":
                case "MOMENT_PAUSE":
                case "MOMENT_RESTART":
                    Logger.Log2($"SEND : IP 192.168.1.216, code {code}, port 8020");
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.1.216", 8020);
                    break;
                case "HIGHT_PLAY1":
                case "HIGHT_PAUSE1":
                case "HIGHT_RESTART1":
                case "HIGHT_PLAY2":
                case "HIGHT_PAUSE2":
                case "HIGHT_RESTART2":
                    Logger.Log2($"SEND : IP 192.168.1.132, code {code}, port 8020");
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.1.132", 8020);
                    break;
                case "PRO_PLAY":
                case "PRO_PAUSE":
                case "PRO_RESTART":
                    Logger.Log2($"SEND : IP 192.168.1.133, code {code}, port 8020");
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.1.133", 8020);
                    break;
                default:
                    break;
            }
        }

    }
}
