﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UECP
{
    public class UECPEncoder
    {
        Endpoint _endpoint;

        public UECPEncoder(Endpoint ep)
        {
            _endpoint = ep;
        }
        public static byte[] ToByteArray(string value)
        {
            byte[] bytes = new byte[value.Length / 2];
            int i = 0;
            while (true)
            {
                byte current = Convert.ToByte(value.Substring(0, 2), 16);
                //  Convert.ToByte()
                bytes[i] = current;
                i++;
                value = value.Substring(2);

                if (value == "")
                {
                    break;
                }
            }

            return bytes;
        }
        public void SetPI(string pi)
        {
            BuildAndSendMessage(MEC.RDS_PI, ToByteArray(pi));
        }
        //public void SetPI(string pi)
        //{
        //    //BuildAndSendMessage(MEC.RDS_PI, BitConverter.GetBytes(pi));
        //    if (pi.Length > 4)
        //        pi = pi.Substring(0, 4);

        //    List<byte> piBytes = new List<byte>();
        //    piBytes.AddRange(Encoding.ASCII.GetBytes(pi));
        //    FillBytes(piBytes, (byte)' ', 4);

        //    BuildAndSendMessage(MEC.RDS_PS, piBytes.ToArray());
        //}

        public void SetPS(string ps)
        {
            if (ps.Length > 8)
                ps = ps.Substring(0, 8);

            List<byte> psBytes = new List<byte>();
            psBytes.AddRange(Encoding.ASCII.GetBytes(ps));
            FillBytes(psBytes, (byte)' ', 8);

            BuildAndSendMessage(MEC.RDS_PS, psBytes.ToArray());
        }

        public void SetAF(string af)
        {
            if (af.Length > 8)
                af = af.Substring(0, 8);

            List<byte> afBytes = new List<byte>();
            afBytes.AddRange(Encoding.ASCII.GetBytes(af));
            FillBytes(afBytes, (byte)' ', 8);

            BuildAndSendMessage(MEC.RDS_AF, afBytes.ToArray());
        }

        public void SetRadioText(string radioText)
        {
            if(radioText.Length > 64)
                radioText = radioText.Substring(0, 64);

            // no A/B flag, infinite transmissions, buffer flushed when a new RT message arrives
            byte rtConfig = 0x00; 

            List<byte> rtData = new List<byte>();
            rtData.Add(rtConfig);
            rtData.AddRange(Encoding.ASCII.GetBytes(radioText));

            BuildAndSendMessage(MEC.RDS_RT, rtData.ToArray());
        }

        public void SetTraffic(bool TA, bool TP)
        {
            byte taFlag = 0x01;
            byte tpFlag = 0x02;

            int data = 0;

            if (TA)
                data = data | taFlag;

            if (TP)
                data = data | tpFlag;

            byte[] msgData = new byte[1];
            msgData[0] = (byte)data;

            BuildAndSendMessage(MEC.RDS_TA_TP, msgData);
        }

        public void SetPTY(PTY pty)
        {
            byte[] ptyData = new byte[1];
            ptyData[0] = (byte)pty;
            BuildAndSendMessage(MEC.RDS_PTY, ptyData);
        }

        public void SetPTYN(string ptyn)
        {
            if (ptyn.Length > 8)
                ptyn = ptyn.Substring(0, 8);

            List<byte> ptynBytes = new List<byte>();
            ptynBytes.AddRange(Encoding.ASCII.GetBytes(ptyn));
            FillBytes(ptynBytes, (byte)' ', 8);

            BuildAndSendMessage(MEC.RDS_PTYN, ptynBytes.ToArray());
        }

        public void Set3A(string data)
        {
            if (data.Length > 12)
                data = data.Substring(0,12);

            List<byte> dataBytes = new List<byte>();
            dataBytes.AddRange(Encoding.ASCII.GetBytes(data));
            FillBytes(dataBytes, (byte)' ', 8);

            BuildAndSendMessage(MEC.FREE_FORMAT, dataBytes.ToArray());
        }

        public void Set8A(string data)
        {
            if (data.Length > 12)
                data = data.Substring(0, 12);

            List<byte> dataBytes = new List<byte>();
            dataBytes.AddRange(Encoding.ASCII.GetBytes(data));
            FillBytes(dataBytes, (byte)' ', 8);

            BuildAndSendMessage(MEC.TMC, dataBytes.ToArray());
        }

        private void BuildAndSendMessage(MEC elementCode, byte[] messageElementData)
        {
            BuildAndSendMessage(new MessageElement(elementCode, messageElementData));
        }

        private void BuildAndSendMessage(MessageElement messageElement)
        {
            UECPFrame uecpFrame = new UECPFrame();
            uecpFrame.MessageElements.Add(messageElement);

            _endpoint.SendData(uecpFrame.GetBytes());
        }

        private void FillBytes(List<byte> data, byte fillWith, int desiredLength)
        {
            int fillBytes = desiredLength - data.Count;
            if (fillBytes > 0)
            {
                for (int i = 0; i < fillBytes; i++)
                {
                    data.Add(fillWith);
                }
            }
        }
    }
}
