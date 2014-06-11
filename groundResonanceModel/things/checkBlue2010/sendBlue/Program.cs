using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace sendBlue
{
    public class Program
    {
        public static void Main()
        {
            // write your code here

            UInt32 j = 0;


            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            BlueSerial ser = new BlueSerial();

            while (true)
            {
                j++;


                //ser.Print(System.Text.Encoding.UTF8.GetBytes("on\n"));   // Brake marker


                ser.Print(j);
                Thread.Sleep(500);
                //led.Write(false);
                

            }


        }

    }
}
