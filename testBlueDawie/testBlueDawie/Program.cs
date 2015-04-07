using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace testBlueDawie
{
    public class Program
    {
        public static BlueSerial blue = new BlueSerial();
        public static UInt16 byteOut = 0;

        public static void Main()
        {
            // write your code here
            Thread.Sleep(Timeout.Infinite);

        }

        static Timer stopRotor = new Timer(delegate
        {
            blue.Print(byteOut);
            byteOut ++;
            if (byteOut > 999)
            {
                byteOut = 0;
            }
        },
    null,
    2,
    500);
    }
}
