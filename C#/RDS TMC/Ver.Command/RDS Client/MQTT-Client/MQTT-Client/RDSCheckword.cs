using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQTT_Client
{
    class RDSCheckword
    {
        string chkw = "";
        public string rds_syndrome(int message, int mlen , int offset)
        {
            int POLY = 0x5B9;	//# 10110111001, g(x)=x^10+x^8+x^7+x^5+x^4+x^3+1
            int PLEN = 10;
            int[] OFFSET = { 252, 408, 360, 436, 848 };
            int[] SYNDROME = { 383, 14, 303, 663, 748 };
            string[] OFFSET_NAME = { "A", "B", "C", "D", "C" };
            int reg = 0;
            int checkword = 0;
            //Console.WriteLine("message : {0} ", message);
            if ((mlen != 16) && (mlen != 26))
            {
                //raise ValueError, "mlen must be either 16 or 26"
                Console.WriteLine("mlen must be either 16 or 26");
            }
            // start calculation

            for (int i = mlen; i > 0; i--)
            {
                reg = (reg << 1) | (message >> (i - 1) & 0x1);
                //Console.WriteLine("reg = {0} : {1} ", i, reg);
                if ((reg & (1 << PLEN)) >= (1 << PLEN))
                {
                    reg = reg ^ POLY;
                }

            }
            for (int i = PLEN; i > 0; i--)
            {
                reg = reg << 1;
                if ((reg & (1 << PLEN)) >= (1 << PLEN))
                {
                    reg = reg ^ POLY;
                }
            }
            checkword = reg & ((1 << PLEN) - 1);
            //Console.WriteLine("checkword : {0} ", checkword);

            for (int i = 0; i < 5; i++)
            {
                if (mlen == 16)
                {   
                    //listBox1.Items.Insert(i, "checkword : " + OFFSET_NAME[i] + " : " + Convert.ToString(checkword ^ OFFSET[i], 2));
                    if (offset == i)
                    {
                        //Console.WriteLine("checkword: {0} : {1} ", OFFSET_NAME[i], Convert.ToString(checkword ^ OFFSET[i], 2));
                        chkw = Convert.ToString(checkword ^ OFFSET[i], 2);
                    }
                }
                else
                {
                    if (checkword == SYNDROME[i])
                    {
                        Console.WriteLine("checkword matches syndrome for offset", OFFSET_NAME[i]);

                    }
                }
            }

            return chkw;
        }

    }
}
