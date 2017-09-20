using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQTT_Client
{
    class RDSDecode
    {
        public void sub_data(string data)
        {
            data = hex2binary(data);
            string Block1 = data.Substring(0, 16);
            string Block2 = data.Substring(26, 16);
            string Block3 = data.Substring(52, 16);
            string Block4 = data.Substring(78, 16);
            decode_rds(Block1, Block2, Block3, Block4);
        }

        public void decode_rds(string s1, string s2, string s3, string s4)
        {
            string chk = s2.Substring(0, 4);
            if (chk=="0000")
            { 
            string PI = HexConverted(s1);
            string GT = s2.Substring(0, 4);
            string VS = s2.Substring(4, 1);
            string TP = s2.Substring(5, 1);
            string PTY = s2.Substring(6, 5);
            string PTY_hex = HexConverted(PTY);
            string T = s2.Substring(11, 1);
            string F = s2.Substring(12, 1);
            string DP = s2.Substring(13, 3);
            string AF1 = s3.Substring(0, 8);
            string AF2 = s3.Substring(8, 8);
            string PSN = HexConverted(s4);
            string PSN_ASCII = ConvertHex(PSN);
           

            Form1.logger.Add("*** RDS ***");
            Form1.logger.Add("Decoder : ");
            Form1.logger.Add("Program Identification : " + s1 + " (" + PI + ")");
            Form1.logger.Add("Group type code/version : " + GT + " (" + VS + ")");
            Form1.logger.Add("Traffic Program : " + TP);
            Form1.logger.Add("Programe Type : " + PTY + " (" + PTY_hex + ")");
            Form1.logger.Add("Decoded 0A group : ");
            Form1.logger.Add("        Traffic Announcement : " + T);
            Form1.logger.Add("        Music Speech switch : " + F);
            Form1.logger.Add("        Decoder Identification control : " + DP);
            Form1.logger.Add("        Alternative Frequencies : " + AF1 + " , " + AF2);
            Form1.logger.Add("        Programme Service name : " + s4 + "(" + PSN_ASCII + ")");
            Form1.logger.Add("        Collected PSN : " + PSN_ASCII);
            Form1.logger.Add(" ");
            Form1.logger.Add(" *************************************************************************************** ");
           }
            else if (chk == "0011")
            {
                string PI = HexConverted(s1);
                string GT = s2.Substring(0, 4);
                string VS = s2.Substring(4, 1);
                string TP = s2.Substring(5, 1);
                string PTY = s2.Substring(6, 5);
                string PTY_hex = HexConverted(PTY);
                string T = s2.Substring(11, 1);
                string F = s2.Substring(12, 1);
                string DP = s2.Substring(13, 3);
                string LTN = s3.Substring(0, 10);
                string LTN_Dec = DecConverted(LTN);
                string AL = s3.Substring(10, 1);
                string Md = s3.Substring(11, 1);
                string IS = s3.Substring(12, 1);
                string NS = s3.Substring(13, 1);
                string RS = s3.Substring(14, 1);
                string US = s3.Substring(15, 1);
                string AID = s4;
                string AID_Dec = DecConverted(s4);

                Form1.logger.Add("*** RDS ***");
                Form1.logger.Add("Decoder : ");
                Form1.logger.Add("Program Identification : " + s1 + " (" + PI + ")");
                Form1.logger.Add("Group type code/version : " + GT + " (" + VS + ")");
                Form1.logger.Add("Traffic Program : " + TP);
                Form1.logger.Add("Programe Type : " + PTY + " (" + PTY_hex + ")");
                Form1.logger.Add("Decoded TMC Sys Info group (3A) : ");
                Form1.logger.Add("        Location Table Number : " + LTN + "(" + LTN_Dec+")");
                Form1.logger.Add("        Alternationtive Frequency bit : " + AL);
                Form1.logger.Add("        Mode of Transmission : " + Md);
                Form1.logger.Add("        International Scope : " + IS);
                Form1.logger.Add("        National Scope : " + NS);
                Form1.logger.Add("        Regional Scope : " + RS);
                Form1.logger.Add("        Urban Scope : " + US);
                Form1.logger.Add("        AID : " + AID + " (" + AID_Dec + ")");
                Form1.logger.Add(" ");
                Form1.logger.Add(" *************************************************************************************** ");
            }
            else if (chk == "1000")
            {
                string PI = HexConverted(s1);
                string GT = s2.Substring(0, 4);
                string VS = s2.Substring(4, 1);
                string TP = s2.Substring(5, 1);
                string PTY = s2.Substring(6, 5);
                string PTY_hex = HexConverted(PTY);
                string T = s2.Substring(11, 1);
                string F = s2.Substring(12, 1);
                string DP = s2.Substring(13, 3);
                string D = s3.Substring(0, 1);
                string PN = s3.Substring(1, 1);
                string Ext = s3.Substring(2, 3);
                string Ext_hex = HexConverted(Ext);
                string EV = s3.Substring(5, 11);
                string EV_dec = DecConverted(EV);
                string Locat = DecConverted(s4);

                Form1.logger.Add("*** RDS ***");
                Form1.logger.Add("Decoder : ");
                Form1.logger.Add("Program Identification : " + s1 + " (" + PI + ")");
                Form1.logger.Add("Group type code/version : " + GT + " (" + VS + ")");
                Form1.logger.Add("Traffic Program : " + TP);
                Form1.logger.Add("Programe Type : " + PTY + " (" + PTY_hex + ")");
                Form1.logger.Add("Decoded 8A group : ");
                Form1.logger.Add("        Bit X4 : " + T);
                Form1.logger.Add("        Bit X3 : " + F);
                Form1.logger.Add("        Duration and Persistence : " + DP);
                Form1.logger.Add("        Diversion advice : " + D);
                Form1.logger.Add("        Direction : " + PN);
                Form1.logger.Add("        Extent : " + Ext + " (" + Ext_hex + ")");
                Form1.logger.Add("        Event : " + EV + " (" + EV_dec + ")");
                Form1.logger.Add("        Location : " + s4 + " (" + Locat + ")");
                Form1.logger.Add(" ");
                Form1.logger.Add(" *************************************************************************************** ");
            }
        }

        public static string ConvertHex(String hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }

        public string HexConverted(string strBinary)
        {
            string strHex = Convert.ToInt32(strBinary, 2).ToString("X");
            return strHex;
        }
        public string DecConverted(string strBinary)
        {
            string strDex = Convert.ToInt32(strBinary, 2).ToString();
            return strDex;
        }
        public static string hex2binary(string hexvalue)
        {
            return String.Join(String.Empty, hexvalue.Select(c => Convert.ToString(Convert.ToUInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }
    }
}
