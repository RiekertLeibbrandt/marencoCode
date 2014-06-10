using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace checkBlue
{
    public class Program
    {
        public static void Main()
        {
            // write your code here

            BlueSerial ser = new BlueSerial();

            int j = 0;

            while (true)
            {
                Thread.Sleep(500);
                j++;
                ser.Print(j);

            }
        }

    }
}
