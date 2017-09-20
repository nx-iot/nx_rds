/*
MIT License

Copyright (c) 2017 Stéphane Lepin <stephane.lepin@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UECP
{
    public interface Endpoint
    {
        void SendData(byte[] data);
    }

    public class UDPSimplexEndpoint : Endpoint
    {
      //  private UdpClient _udpClient;
        private SerialPort _sr;

        public UDPSimplexEndpoint(int br, string encoderPort)
        {
            //  _udpClient = new UdpClient();
            //  _udpClient.Connect(IPAddress.Parse(encoderAddress), encoderPort);
            _sr = new SerialPort(encoderPort, br, Parity.None, 8, StopBits.One);
            // Console.WriteLine("Incoming Data:");
            //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            _sr.Open();
            _sr.DataReceived += new SerialDataReceivedEventHandler(port1_DataReceived);
        }

        public void SendData(byte[] data)
        {
            _sr.Write(data,0, data.Length);
        }

        public void disconnection()
        {
            _sr.Close();
        }

        private void port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(1);
            byte DATA = Convert.ToByte(_sr.ReadByte());
            String data = Convert.ToString(DATA);

            Console.WriteLine(DATA);
        }
    }
}
