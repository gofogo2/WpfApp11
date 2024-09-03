using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfApp11.Helpers.ByteConverter;

namespace WpfApp11.Helpers
{
   public class ProtocolHelper
    {
        public ProtocolHelper()
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

        private void Instance_PacketReceived(string code)
        {
            Logger.Log2(code);

            code = code.ToUpper().Trim();

            switch (code)
            {
                case "IDLE_ALL":
                    break;
                case "IDLE_VW":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "IDLE_AI":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "IDLE_HO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "IDLE_CL":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "IDLE_C":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                
                
                case "VW_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "VW_MSG_NORMAL":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "VW_MSG_CUSTOM":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "VW_CEO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "VW_REPORT":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "VW_BLEND":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;


                case "EN_OPEN":
                    break;
                case "EN_CLOSE":
                    break;
                case "EX_OPEN":
                    break;
                case "EX_CLOSE":
                    break;
                case "EX_ALL":
                    break;
                case "EN_ALL":
                    break;
                case "FACE_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "FACE_RECO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "EV_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "EV_VIDEO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "OF_ON":
                    break;
                case "OF_OFF":
                    break;
                case "CEO_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "CEO_SOLUTION":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "CEO_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "MB_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "MB_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "WS_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "WS_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;


                case "CL_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "CL_VIDEO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "SCP_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO0":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO1":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO2":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO3":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO4":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO5":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO6":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "SCP_VIDEO7":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "DC_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "DC_VIDEO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "MSP_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "MSP_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                default:
                    break;
            }
        }

    }
}
