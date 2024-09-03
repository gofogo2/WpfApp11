using System;
using System.IO.Ports;

namespace WpfApp11.Helpers
{
    public class SerialHelper
    {
        private SerialPort _serialPort;

        public SerialHelper(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits
            };
        }

        public bool OpenConnection()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log2($"Error : {ex.Message}");
                Console.WriteLine($"Error opening serial port: {ex.Message}");
                return false;
            }
        }

        public void CloseConnection()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public bool SendData(string data)
        {
            if (!_serialPort.IsOpen)
            {
                Logger.Log2($"Serial port is not open.");
                Console.WriteLine("Serial port is not open.");
                return false;
            }

            try
            {
                Logger.Log2($"Serial: {data}");
                _serialPort.Write(data);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log2($"Error : {ex.Message}");
                Console.WriteLine($"Error sending data: {ex.Message}");
                return false;
            }
        }

        public bool SendData(byte[] data)
        {
            if (!_serialPort.IsOpen)
            {
                Logger.Log2($"Error : Serial port is not open.");
                Console.WriteLine("Serial port is not open.");
                return false;
            }

            try
            {
                _serialPort.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log2($"Error sending data : {ex.Message}");
                Console.WriteLine($"Error sending data: {ex.Message}");
                return false;
            }
        }
    }
}