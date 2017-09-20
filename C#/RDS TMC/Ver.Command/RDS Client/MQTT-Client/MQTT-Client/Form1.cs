using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using RDS.Serial;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace MQTT_Client
{
    public partial class Form1 : Form
    {
        private delegate void myUICallBack(string myStr, TextBox ctl);
        static MqttClient client;
        string host = "m10.cloudmqtt.com";
        int port = 10277;
        string username = "ovwnmwny";
        string password = "zERBTx4cWMeG";
        int chk_encryp = 0;
        int flag_send = 0;
        SerialPortControl s_port;
        RDSCheckword cw = new RDSCheckword();
        RDSEncryption en = new RDSEncryption("");
        RDSDecode decode = new RDSDecode();
        string AF = "";
        string format = "";
        string PI = "";
        string Radio = "";
        string city = "";
        int ENCID = 0;
        string c_id = "";
        public static List<string> logger { get; set; }

        public string[] DE_block = new string[4]; //Decryp
        public string[] call_back = new string[4]; //Callbak form serial port
        List<string> SEND_DATA = new List<string>();
        public string[] SEND_CONFIG = new string[10]; //write Data to serial port
                                                       // public string[] GET_MQTT = new string[1000]; // get data form mqtt
   
        int af = 204; // AF Decimal
        int af_fre = 1079; //AF Frequency
        int len_total = 0;

        List<string> mqtt_data = new List<string>();
        List<string> mqtt_data_raw = new List<string>();

        private void myUI(string myStr, TextBox ctl)
        {
            if (this.InvokeRequired)
            {
                myUICallBack myUpdate = new myUICallBack(myUI);
                this.Invoke(myUpdate, myStr, ctl);
                //GET_MQTT[c_mqtt] = myStr;
                if (myStr != null)
                {
                    mqtt_data_raw.Add(myStr);
                }
            }
            else
            {
                ctl.AppendText(myStr + Environment.NewLine);
            }

        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            myUI(System.Text.Encoding.UTF8.GetString(e.Message), MessageTextBox);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logger = new List<string>();
            string[] ports = SerialPortControl.getCompots();
            foreach (string p in ports)
            {
                comboBox1.Items.Add(p);
            }
            if (ports.Count() > 0)
            {
                comboBox1.Text = ports[0];
            }
            start();
        }
        
        public void start()
        {
            try
            {

                client = new MqttClient(host, port, false, null);
                client.Connect(Guid.NewGuid().ToString(), username, password);
                client.MqttMsgPublishReceived += new MqttClient.MqttMsgPublishEventHandler(client_MqttMsgPublishReceived);

            }
            catch
            {
                label4.Text = "Can't connect to server";
            }
            if (client != null && client.IsConnected)
            {
                label4.Text = "";
                MessageTextBox.Clear();
            }
            button2.Enabled = false;
            timer3.Enabled = true;
            c_id = System.IO.File.ReadAllText("C_ID.text");

            if (c_id == "")
            {
                c_id = "rds" + generateID();
                System.IO.StreamWriter file = new System.IO.StreamWriter("C_ID.text");
                file.Write(c_id);
                file.Close();
            }

            Subscribe(c_id);
            Run_Config();
            timer1.Enabled = true; // Read Config

        }
        private void config_0A()
        {
            int get_af = Convert.ToInt32(Convert.ToDouble(AF) * 10);
            string set_af1 = Convert.ToString(af - (af_fre - get_af), 2).PadLeft(8, '0');
            string set_af2 = Convert.ToString((af - (af_fre - get_af)) + 5, 2).PadLeft(8, '0');

            SEND_CONFIG[0] = StringToHex("PI=") + StringToHex(PI) + "0d0a"; //PI
            SEND_CONFIG[1] = StringToHex("AF=") + StringToHex(AF) + "0d0a";//AF
            SEND_CONFIG[2] = StringToHex("PS=") + StringToHex(Radio) + "0d0a"; //PS
            SEND_CONFIG[3] = StringToHex("TP=1") + "0d0a"; //TP=1
            SEND_CONFIG[4] = StringToHex("MS=0") + "0d0a"; //MS=0
            SEND_CONFIG[5] = StringToHex("UDG1=353044C0CD46") + "0d0a"; //3A
            logger.Add("********* Sent config *************");
            for (int i = 0; i < 6; i++)
            {
                //logger.Add("SEND_CONFIG : " + SEND_CONFIG[i]);
                byte[] s_data = ToByteArray(SEND_CONFIG[i]);
                s_port.writeData(ref s_data, s_data.Length);
                //Thread.Sleep(10);
            }

        }
        public void Run_Config()
        {
            string[] lines = System.IO.File.ReadAllLines("Config.text");
            foreach (string line in lines)
            {
                string[] a = line.Split(',');
                if (a[0] != null)
                {
                    if (c_id == a[1])
                    {
                        AF = a[2];
                        Radio = a[3];
                        PI = a[4];
                        format = a[5];
                        chk_encryp = Int32.Parse(a[6]);
                        ENCID = Int32.Parse(a[7]);
                        city = a[8];
                        Subscribe(city); // city
                    }
                }
            }
        }
        private void Subscribe(string topic)
        {
            client.Subscribe(new string[] { topic }, new byte[] { 0 });
            logger.Add("********* Subscribe *************");
        }

        //******************************** RDS *************************************

        bool checkLoopData = false;

        public void Read_Raw()
        {
            if (mqtt_data_raw.Count > 0)
            {
                string[] tag = mqtt_data_raw[0].Split(',');
                if (tag[0] == "config")
                {
                    Read_Config(mqtt_data_raw[0]);
                    mqtt_data_raw.RemoveAt(0);
                }
                else if (tag[0] == "data")
                {
                    
                    if (checkLoopData == false)
                    {
                        mqtt_data.Clear();
                        checkLoopData = true;
                    }
                    mqtt_data.Add(mqtt_data_raw[0]);
                    mqtt_data_raw.RemoveAt(0);
                }

            }
            else {
                if (checkLoopData) {
                    Read_File(mqtt_data);
                    checkLoopData = false;

                }
            }

        }
        public void Read_Config(string configx)
        {
            string[] tag = configx.Split(',');
            if (tag[0] == "config")
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter("Config.text");
                file.WriteLine(configx);
                file.Close();

                AF = tag[2];
                Radio = tag[3];
                PI = tag[4];
                format = tag[5];
                chk_encryp = Int32.Parse(tag[6]);
                ENCID = Int32.Parse(tag[7]);
                city = tag[8];
                Subscribe(city); // city
                config_0A();
            }
        }

        public void Read_File(List<string> datas)
        {
            string[] Loc = new string[1000];
            string[] Dir = new string[1000];
            string[] Quity = new string[1000];
            string[] QuType = new string[1000];
            string[] traffic = new string[1000];
            string[] lines2 = null;
            if (datas != null)
            {
                lines2 = datas.ToArray();
            }
            else {
                lines2 = System.IO.File.ReadAllLines("Read_RDS.text");
                mqtt_data.AddRange(lines2);
            }
            int j = 0;
            foreach (string line2 in lines2)
            {
                string[] a = line2.Split(',');

                Loc[j] = a[1];
                if (a[2] == "positive")
                {
                    Dir[j] = "0";
                }
                else
                {
                    Dir[j] = "1";
                }
                Quity[j] = a[3];
                QuType[j] = a[4];
                j++;
            }
            for (int i = 0; i < j; i++)
            {
                string b4 = Convert.ToString(Convert.ToInt32(Loc[i]), 2).PadLeft(16, '0');
                string b3 = "0" + Dir[i] + Convert.ToString(Convert.ToUInt32(Quity[i]), 2).PadLeft(3, '0') +
                Convert.ToString(Convert.ToUInt32(QuType[i]), 2).PadLeft(11, '0');//Event
                traffic[i] = "8529" + BinaryStringToHexString(b3) + BinaryStringToHexString(b4);
            }
            Set_Data(traffic, j); // Set Data for Sent

        }
        public void Set_Data(string[] T_data, int len_t)
        {
            int chk_sent = -1;
            //int get_af = Convert.ToInt32(Convert.ToDouble(AF) * 10);
            //string set_af1 = Convert.ToString(af - (af_fre - get_af), 2).PadLeft(8, '0');
            //string set_af2 = Convert.ToString((af - (af_fre - get_af)) + 5, 2).PadLeft(8, '0');
            
            string f = format;
            while (f.Contains("/"))
            {
                f = f.Substring(3);
                chk_sent++;
            }

            len_total = (chk_sent * len_t);
            List<string> SET_DATA = new List<string>();
            //0A
            //SET_DATA[0] = PI + "052C" + BinaryStringToHexString(set_af1 + set_af2) + "4355";  // Data 0A // PSN = 3137 
            //SET_DATA[0] = PI + "0528E0CD4355"; 

            //SET_DATA[0] = "3530 44C0 CD46";   //Data 3A

            int k = 0;
            for (int i = 0; i < len_t; i++)
            {
                for (int j = 0; j < chk_sent; j++)
                {
                    SET_DATA.Add(T_data[i]); //Data 8A
                    k++;
                }
            }
            Pack_Data(SET_DATA, len_total);
        }

        public void Pack_Data(List<string> SET_DATA, int len)
        {
            SEND_DATA.Clear();
            string[] Block = new string[4];
            string[] chkw = new string[4];

            for (int j = 0; j < len; j++)
            {
                Block[0] = SET_DATA[j].Substring(0, 4);
                Block[1] = SET_DATA[j].Substring(4, 4);
                Block[2] = SET_DATA[j].Substring(8, 4);
                //Block[3] = SET_DATA[j].Substring(12, 4);

                for (int i = 0; i < 3; i++)
                {
                    if (chk_encryp == 1)
                    {
                        var rdata = en.Encryption(Convert.ToInt32(Block[i], 16), ENCID);
                        //Console.WriteLine("Encryption Data : " + rdata);
                        Block[i] = Convert.ToString(rdata);
                    }
                    chkw[i] = cw.rds_syndrome(Convert.ToInt32(Block[i], 16), 16, i);
                    chkw[i] = chkw[i].PadLeft(10, '0'); // Add Zero (0)
                }
                string Con_Bin = "";
                for (int i = 0; i < 3; i++)
                {
                    if (chk_encryp == 1)
                    {
                        //Console.WriteLine("Value : " + i + " : " + Convert.ToString(Convert.ToInt32(Block[i]), 2).PadLeft(16, '0') + " , " + chkw[i]);
                        Con_Bin += Convert.ToString(Convert.ToInt32(Block[i]), 2).PadLeft(16, '0');// + chkw[i];
                    }
                    else
                    {
                        Con_Bin += hex2binary(Block[i]);//+ chkw[i];
                    }
                }
                
                string Con_Hex = BinaryStringToHexString(Con_Bin);
                SEND_DATA.Add(StringToHex("UDG2=") + StringToHex(Con_Hex)+ "0d0a");  // Sent DATA
            }
            SET_DATA.Clear();
            Send_Serial();
        }

        public void Send_Serial()
        {
            if (flag_send == 1) {
                for (int i = 0; i < len_total; i++) {
                    logger.Add("********* Sent Data *************");
                    logger.Add("SEND_DATA : " + SEND_DATA[i]);
                    byte[] s_data = ToByteArray(SEND_DATA[i]);
                    s_port.writeData(ref s_data, s_data.Length);
                }
            }
        }
        //***************************************************************************************************************
        //******************************************** Timmer ***********************************************************

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            //Read_Config();
            Read_Raw();
            textBox1.Text = c_id ;
            textBox2.Text = city;
            textBox3.Text = AF;
            textBox4.Text = Radio;
            textBox6.Text = PI;
            if (chk_encryp == 0) {
                textBox5.Text = "Not Encryption";
            }
            else
            {
                textBox5.Text = "Encryption";
            }
            
        }
       
        private void timer3_Tick(object sender, EventArgs e)
        {
            //Timer for write data to serial port
            if (logger.Count > 0)
            {
                for (int i = 0; i < logger.Count; i++)
                {
                    listBox1.Items.Insert(i, logger[i]);
                }
                logger.Clear();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            s_port = new SerialPortControl(comboBox1.Text, 2400);
            button1.Enabled = false;
            button2.Enabled = true;
            config_0A();
            flag_send = 1;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            s_port.disconnection();
            flag_send = 0;
        }

        //***************************************************************************************************************
        //******************************************** Convert Data *****************************************************

        private string StringToHex(string hexstring)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in hexstring)
            {
                //Note: X for upper, x for lower case letters
                sb.Append(Convert.ToInt32(t).ToString("x"));
            }
            return sb.ToString();
        }
        public static string BinaryStringToHexString(string binary)
        {

            // BinaryStringToHexString
            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            // TODO: check all 1's or 0's... Will throw otherwise

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        public static string hex2binary(string hexvalue)
        {
            return String.Join(String.Empty, hexvalue.Select(c => Convert.ToString(Convert.ToUInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }
        public string generateID()
        {
            long i = 1;

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }

            string number = String.Format("{0:d5}", (DateTime.Now.Ticks / 10) % 100000);

            return number;
        }

        public static byte[] ToByteArray(string value)
        {
            byte[] bytes = new byte[value.Length/2];
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
     
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.RDS_Encoder.ShowBalloonTip(100, "Notify Message", "Please click for see more detail..", ToolTipIcon.Info);
            }
        }
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (client != null && client.IsConnected) client.Disconnect();
        }
    }
}
