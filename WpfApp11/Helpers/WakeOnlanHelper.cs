﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    using System;
    using System.Net.Sockets;

    namespace Launcher_SE.Helpers
    {
        public class WakeOnLanHelper : UdpClient
        {
            private static WakeOnLanHelper _instance;
            private static readonly object _lock = new object();

            private WakeOnLanHelper() : base() { }

            public static WakeOnLanHelper Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock (_lock)
                        {
                            if (_instance == null)
                            {
                                _instance = new WakeOnLanHelper();
                            }
                        }
                    }
                    return _instance;
                }
            }

            public void TurnOnPC(string macAddress)
            {
                this.Connect(new System.Net.IPAddress(0xffffffff), 0x2fff); //255.255.255.255 : 12287
                if (this.Active)
                {
                    this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                }
                byte[] bytes = GetMagicPacketToByteArray(macAddress);
                // 컴퓨터를 부팅 할 매직패킷을 보낸다.
                int reterned_value = this.Send(bytes, 1024);
            }

            public void TurnOnPC(string ip, string macAddress)
            {
                this.Connect(ip, 12287); //255.255.255.255 : 12287
                if (this.Active)
                {
                    this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                }
                byte[] bytes = GetMagicPacketToByteArray(macAddress);
                // 컴퓨터를 부팅 할 매직패킷을 보낸다.
                int reterned_value = this.Send(bytes, 1024);
            }

            private byte[] GetMagicPacketToByteArray(string macAddress)
            {
                // 보낼 바이트 초기화
                int counter = 0;
                // 보낼 버퍼 초기화
                byte[] bytes = new byte[1024];
                // 처음 6개 바이트는 "0xFF"  
                for (int y = 0; y < 6; y++)
                    bytes[counter++] = 0xFF;
                // 부팅 할 컴퓨터의 맥어드레스를 16번 반복한다.
                for (int y = 0; y < 16; y++)
                {
                    int i = 0;
                    for (int z = 0; z < 6; z++)
                    {
                        bytes[counter++] =
                            byte.Parse(macAddress.Substring(i, 2),
                            System.Globalization.NumberStyles.HexNumber);
                        i += 2;
                    }
                }
                return bytes;
            }
        }
    }
}