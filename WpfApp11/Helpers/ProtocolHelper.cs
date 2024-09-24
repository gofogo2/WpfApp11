using OSC_Test.Helpers;
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
        enum Serial
        {
            VW=1,
            CL=2,
            EL=3
        }

        enum  VW{
            LIGHT_ON=1,
            LIGHT_OFF=2
        }

        enum CL
        {
            LIGHT_ON=1,
            LIGHT_OFF=2,
            BL_UP=3,
            BL_DOWN=4,
            AIR_ON=5,
            AIR_OFF=6
        }

        enum EL
        {
            OPEN1=1,
            CLOSE1=2,
            OPEN2=3,
            CLOSE2=4,
            OPENALL=5,
            CLOSEALL=6
        }



        //비전월 조명
        SerialHelper serialHelper01;

        //라운지 조명 / 블라인드 / 공청기
        SerialHelper serialHelper02;

        //엘레베이터
        SerialHelper serialHelper03;
        public ProtocolHelper()
        {
            serialHelper01 = new SerialHelper("COM1");
            serialHelper02 = new SerialHelper("COM2");
            serialHelper03 = new SerialHelper("COM3");
            
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

        public void SendSerial(string index, string msg)
        {
            switch (index)
            {
                case "VW":
                    serialHelper01.OpenConnection();
                    serialHelper01.SendData(msg);
                    serialHelper01.CloseConnection();
                    break;
                case "CL":
                    serialHelper02.OpenConnection();
                    serialHelper02.SendData(msg);
                    serialHelper02.CloseConnection();
                    break;
                case "EL":
                    serialHelper03.OpenConnection();
                    serialHelper03.SendData(msg);
                    serialHelper03.CloseConnection();
                    break;
                default:
                    break;
            }
        }

        public void Instance_PacketReceived(string code)
        {
            Logger.Log2(code);

            code = code.ToUpper().Trim();

            switch (code)
            {
                case "IDLE_ALL":
                    break;
                case "IDLE_VW":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    SendSerial(Serial.VW.ToString(), VW.LIGHT_ON.ToString());
                    break;
                case "IDLE_AI":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    SendSerial(Serial.EL.ToString(), EL.CLOSEALL.ToString());
                    break;
                case "IDLE_HO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    SendSerial(Serial.CL.ToString(), CL.LIGHT_OFF.ToString());
                    SendSerial(Serial.CL.ToString(), CL.BL_DOWN.ToString());
                    SendSerial(Serial.CL.ToString(), CL.AIR_OFF.ToString());

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

                case "VW_CEO_MUTEOFF":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "VW_CEO_MUTEON":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "VW_CEO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    SendSerial(Serial.VW.ToString(), VW.LIGHT_OFF.ToString());
                    break;
                case "VW_REPORT":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "VW_BLEND":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    SendSerial(Serial.VW.ToString(), VW.LIGHT_OFF.ToString());
                    break;


                case "EN_OPEN":
                    SendSerial(Serial.EL.ToString(), EL.OPEN1.ToString());
                    break;
                case "EN_CLOSE":
                    SendSerial(Serial.EL.ToString(), EL.CLOSE1.ToString());
                    break;
                case "EX_OPEN":
                    SendSerial(Serial.EL.ToString(), EL.OPEN2.ToString());
                    break;
                case "EX_CLOSE":
                    SendSerial(Serial.EL.ToString(), EL.CLOSE2.ToString());
                    break;
                case "EX_ALL":
                    SendSerial(Serial.EL.ToString(), EL.OPENALL.ToString());
                    break;
                case "EN_ALL":
                    SendSerial(Serial.EL.ToString(), EL.CLOSEALL.ToString());
                    break;
                case "FACE_IDLE":
                  
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "FACE_RECO":
                    SendSerial(Serial.EL.ToString(), EL.OPEN1.ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "EV_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.48", 8020);
                    break;

                case "EV_VIDEO_MUTEOFF":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.48", 8020);
                    break;

                case "EV_VIDEO_MUTEON":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.48", 8020);
                    break;

                case "EV_VIDEO":
                    SendSerial(Serial.EL.ToString(), EL.OPEN1.ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.48", 8020);
                    break;

                case "EV_VIDEO_END":
                    SendSerial(Serial.EL.ToString(), EL.CLOSE1.ToString());
                    break;

                case "OF_ON":
                    SendSerial(Serial.CL.ToString(), CL.LIGHT_ON.ToString());
                    SendSerial(Serial.CL.ToString(), CL.AIR_ON.ToString());
                    SendSerial(Serial.CL.ToString(), CL.BL_UP.ToString());
                    break;
                case "OF_OFF":
                    SendSerial(Serial.CL.ToString(), CL.LIGHT_OFF.ToString());
                    SendSerial(Serial.CL.ToString(), CL.AIR_OFF.ToString());
                    SendSerial(Serial.CL.ToString(), CL.BL_DOWN.ToString());
                    break;
                case "CEO_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "CEO_SOLUTION_MUTEOFF":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "CEO_SOLUTION_MUTEON":
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
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.134", 8020);
                    break;
                case "WS_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.134", 8020);
                    break;


                case "CL_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;
                case "CL_VIDEO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.91", 8020);
                    break;

                case "SCP_IDLE":
                case "SCP_VIDEO0":
                case "SCP_VIDEO1":
                case "SCP_VIDEO2":
                case "SCP_VIDEO3":
                case "SCP_VIDEO4":
                case "SCP_VIDEO5":
                case "SCP_VIDEO6":
                case "SCP_VIDEO7":
                    OSCSenderHelper.Instance.Send("192.168.0.91", code);
                    break;
                case "DC_IDLE":
                case "DC_VIDEO_1":
                case "DC_VIDEO_2":
                case "DC_VIDEO_3":
                case "DC_VIDEO_4":
                case "DC_VIDEO_5":
                case "DC_VIDEO_6":
                case "DC_VIDEO_7":
                case "DC_VIDEO_8":
                case "DC_VIDEO_FULL":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.152", 8020);
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
