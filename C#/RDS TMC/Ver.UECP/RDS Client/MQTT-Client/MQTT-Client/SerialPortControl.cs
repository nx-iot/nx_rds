using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;


namespace RDS.Serial
{
    class SerialPortControl
    {
        private SerialPort port;
        public bool connected = false;

        public static string[] getCompots()
        {
            string[] ports = SerialPort.GetPortNames();
            return ports;
        }

        public SerialPortControl(String comname,int datarate)
        {
            port = new SerialPort(comname,datarate, Parity.None, 8, StopBits.One);
           // Console.WriteLine("Incoming Data:");
            //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();
            //connected = true;
            //EmoControl.logger.Add("SerialPort is Opened ");
        
        }

        public void disconnection() {
            port.Close();
            connected = false;
            //EmoControl.logger.Add("SerialPort is Closed ");
        }
        public void writeData(ref byte[] data, int length)
        {
            Console.Write("\n");
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine("DATA serial::" + data[i]);
            }
            port.Write(data, 0, length);
            //Console.WriteLine("DATA serial::" + ByteArrayToString(data));
            
        }

        public string readData()
        {
            string data = "";
            if (port.IsOpen == false)
                port.Open();
            //port.WriteLine(textBox1.Text);
            int bytes = port.BytesToRead;
            byte[] byte_buffer = new byte[bytes];
            port.Read(byte_buffer, 0, bytes);
            data = ByteArrayToString(byte_buffer);
            return data;
        }
                
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //string data = port.ReadLine();
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

     }
}
