using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO;
using System.IO.Ports;
using Marenco.Comms;


//
// Hi Jono, I didn't know better so I copied your file into jonathan_accRead.txt
// sorry
//


namespace accRead
{
    public class Program
    {
        //
        // Define analog input and serial port
        //
        public static AnalogInput accRead = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
        public static BlueSerial serial = new BlueSerial();

        public static SerialPort serial2 = new SerialPort(SerialPorts.COM2, 115200, Parity.None, 8,StopBits.One);
  

        //
        // Define some global variables
        //

        public static int nSamples = 9999;
        public static ushort[] reading = new ushort[nSamples+1];
        public static ushort[] readTime = new ushort[nSamples+1];

        

        public static void sendData()
        {

            for (int k = 0; k < nSamples; k++)
            {


                //v = reading[k];
                //vHigh = (byte)(v >> 8);
                //vLow = (byte)(v & 0x00FF);

                //Debug.Print("vHigh " + vHigh.ToString());
                //Debug.Print("vLow " + vLow.ToString());

                serial.Print(reading[k]);

            }

        }


        public static void recordData()
        {

            // record is true while recording, and false after
            bool record = true;
            // counter for recording loop
            int i = 0;


            while (record)
            {

                // start new recording cycle
                if (i == nSamples)
                {
                    i = 0;
                }


                reading[i] = (ushort)accRead.ReadRaw();

                #region old

                //// Check for max and min
                //if (i == 0)
                //{
                //    max = reading[i];
                //    min = max;
                //}

                //if (reading[i] > max)
                //{
                //    max = reading[i];

                //}
                //else if (reading[i] < min)
                //{
                //    min = reading[i];
                //}


                //if (reading[i] > 2500)
                //{
                //    trigger = true;
                //    triggerI = i;
                //}

                //if (i == 9999)
                //{
                //    j++;
                //    Debug.Print("Cycle" + j.ToString());
                //    Debug.Print("Maximum " + max.ToString());
                //    Debug.Print("Minimum " + min.ToString());
                //    Debug.Print("Reading length " + reading.Length.ToString());


                //    v = max;
                //    vHigh = (byte)(v >> 8);
                //    vLow = (byte)(v & 0x00FF);

                //    //serial.WriteByte(vHigh);
                //    //serial.WriteByte(vLow);

                //}

                #endregion

                i++;

                record = false;


            } // while loop


        } // record void

       // public static void trigger()
        //{

       // }

        public static void DataReceivedHandler( object sender, SerialDataReceivedEventArgs e)       
        {           
            
        }


        public static void Main()
        {

            serial.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
               

            serial.

            recordData();


            sendData();

            byte answer = serial2.Read(byte,0,1);

        }




    }

}
