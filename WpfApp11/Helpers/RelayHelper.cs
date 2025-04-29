using Launcher_SE.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp9;

namespace WpfApp11.Helpers
{
    class RelayHelper
    {

        private static RelayHelper _instance = null;
        private static readonly object _padlock = new object();

        private RelayHelper() { }

        public static RelayHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RelayHelper();
                        }
                    }
                }
                return _instance;
            }
        }

       public void ProcessRelay1(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
                OnRelay(item);
            }
            else
            {
                OffRelay(item);
            }
        }


        public async void OnRelay(ItemConfiguration item)
        {
            try
            {
                string hexStr = Utils.Instance.IntToHex(item.Channel);
                Debug.WriteLine(hexStr);
                string hex = $"525920{hexStr}20310D";
                Logger.Log(item.IpAddress, item.port, "Power ON", hex);
                await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(item.port), item.IpAddress);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
            }
        }

        public async void OffRelay(ItemConfiguration item)
        {
            try
            {
                string hexStr = Utils.Instance.IntToHex(item.Channel);
                Debug.WriteLine(hexStr);


                string hex = $"525920{hexStr}20300D";
                Logger.Log(item.IpAddress, item.port, "Power OFF", hex);
                await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(item.port), item.IpAddress);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
            }
        }


    }
}
