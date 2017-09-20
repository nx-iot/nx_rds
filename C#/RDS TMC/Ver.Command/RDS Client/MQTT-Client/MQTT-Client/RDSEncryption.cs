    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    namespace MQTT_Client
    {
        class RDSEncryption
        {
            public int[] Rotate = new int[] {0x00,0x0,0x8,0x4,0xC,0x2,0x7,0xA,0x5,0x3,0xB,0x1,0x9,0x8,0x3,0xD,0x4,0x2,0x6,0x7,0x3,0xE,0x5,0x9,0x2,0xA,0x6,0x8,0x1,0x3,0x2,0x4,0xE};
            public int[] StartBit = new int[] {0,1,3,6,7,2,9,4,6,5,0,8,7,3,0,8,3,1,5,2,9,4,2,6,7,1,8,6,3,2,7,4 };
           // public String[] ENCIDString = new int[32];
            public int[] XOR = new int[] {   0x00,0x19,0x9B,0x7E,0x39,0x54,0xA2,0x44,0x81,0x6C,0xB3,0x2E,0xD7,0x3A,0x9F,0x68,0x55,0xC4,0x39,0x90,0xE5,0x1D,0x26,0xA9,0xBC,0x76,0x82,0xA3,0xC1,0xEA,0x61,0xDA
};

            public RDSEncryption(String PI) {
                this.GenerateServiceKey(PI);
            }
            public void GenerateServiceKey(String PI)
            {
                if (PI != "") {
           
                try
                {
                    string binaryval = Convert.ToString(Convert.ToInt32(PI, 16), 2);
                    //Console.WriteLine(binaryval);
                    string binarystr = binaryval;
                    for (int i = 0; i < 16 - binaryval.Length; i++)
                    {
                        binarystr = "0" + binarystr;
                    }
                    //Console.WriteLine(binarystr);

                    // Generate LFSR with polinomial : x16 + x14 + x13 + x11 + 1
                    int totalbit = 512;
                    int x14, x13, x11, x0, nextbit;
                    binarystr = ReverseString(binarystr);
                    for (int i = 0; i < binarystr.Length; i++)
                    {
                        if (i >= totalbit)
                        {
                            break;
                        }
                        x14 = Convert.ToInt32(binarystr[i + 13].ToString());
                        x13 = Convert.ToInt32(binarystr[i + 12].ToString());
                        x11 = Convert.ToInt32(binarystr[i + 10].ToString());
                        x0 = Convert.ToInt32(binarystr[i].ToString());

                        nextbit = (x14 + x13 + x11 + x0) % 2;
                        binarystr = binarystr + nextbit.ToString();
                        //Console.WriteLine(i);

                    }
                    binarystr = binarystr.Substring(16);
                    //  Console.WriteLine(binarystr);
                    //listBox1.Items.Add(binarystr);


                    // Stream myStream;
                    // SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                    // saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
                    // saveFileDialog1.FileName = "ServiceKeyTable.txt";
                    // saveFileDialog1.RestoreDirectory = true;

                   // StringBuilder sb = new StringBuilder();

                    // if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    // {
                    //if ((myStream = saveFileDialog1.OpenFile()) != null)
                    // {
                    // StreamWriter wText = new StreamWriter(myStream);
                   // string Rotate, Start, XOR;
                    int ENCID = 0;
                    for (int i = 0; i < binarystr.Length; i += 16)
                    {
                        Rotate[ENCID] =Convert.ToInt32(binarystr.Substring(i, 4), 2);
                        StartBit[ENCID] =Convert.ToInt32(binarystr.Substring(i + 4, 4), 2);
                        XOR[ENCID] = Convert.ToInt32(binarystr.Substring(i + 8, 8), 2);
                       // XOR[ENCID] = (XOR[ENCID].Length == 1) ? "0" + XOR[ENCID] : XOR[ENCID];
                       // ENCIDString[ENCID] = ((ENCID.ToString()).Length == 1) ? "0" + ENCID.ToString() : ENCID.ToString();
                        // wText.WriteLine(ENCIDStr + " " + Rotate + " " + Start + " " + XOR);
                        // Console.WriteLine(ENCIDStr + " " + Rotate + " " + Start + " " + XOR);
                       // sb.AppendLine(ENCIDString[ENCID] + " " + Rotate[ENCID] + " " + StartBit[ENCID] + " " + XOR[ENCID]);
                       // listBox1.Items.Add(ENCIDStr + " " + Rotate + " " + Start + " " + XOR);
                        ENCID++;
                    }
                    //  wText.Flush();
                    // myStream.Close();
                    // }
                    // }
                    /// Console.WriteLine(sb.ToString());
                    /// 
                    Console.WriteLine("test");
                }
                catch (Exception ex)
                {
                  //  MessageBox.Show(ex.Message);
                }
                }
            }
            private string ReverseString(string s)
            {
                char[] arr = s.ToCharArray();
                Array.Reverse(arr);
                return new string(arr);
            }

            private int RotateLeft(int value, int nRotate, int nbit)
            {
                uint val = (uint)value;
                return (int)(((val << nRotate) & 0xFFFF) | ((val >> (nbit - nRotate)) & 0xFFFF));
            }

            private int RotateRight(int value, int nRotate, int nbit)
            {
                uint val = (uint)value;
                return (int)((value >> nRotate) | (value << (nbit - nRotate) & 0xFFFF ));
            }

            private int XorValue(int data, int xValue, int nbit)
            {
                uint val = (uint)data;
                return data ^ (xValue << nbit);
            }

            public int Encryption(int data, int ENCID) { 
                 //CalEncrypt := IntToHex(XORVal(RotateRight(nData, Rotate, 16), XORValue, StartBit),4);
                //Console.WriteLine("data::" + data); 
                var rData = RotateRight(data, Rotate[ENCID], 16);
                //Console.WriteLine("RotateRight::" + rData);
                var xData = XorValue(rData,XOR[ENCID], StartBit[ENCID]);
                //Console.WriteLine("XorValue::" + xData);
                return xData;
            }

            public int Decryption(int rdata, int ENCID) {
                //  CalDecrypt := IntToHex(RotateLeft(XORVal(nData, XORValue, StartBit), Rotate, 16),4);
                //return RotateRight(XorValue(data, XOR[ENCID], StartBit[ENCID]), Rotate[ENCID], 16);
                var xData = XorValue(rdata,XOR[ENCID], StartBit[ENCID]);
                //Console.WriteLine("XorValue::" + xData);
                var data = RotateLeft(xData, Rotate[ENCID], 16);
                //Console.WriteLine("RotateLeft::" + data);
                return data;
           
            }

        }
    }
