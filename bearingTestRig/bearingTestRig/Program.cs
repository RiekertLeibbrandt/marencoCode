using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace bearingTestRig
{
    public class Program
    {
        static public int timerPeriod = 100;                     // 10 hz
        static public int timerStart = 5000;                     // Delay before timer starts

        // Port Definitions.
        public static AnalogInput hallIn1 = new AnalogInput(AnalogChannels.ANALOG_PIN_A5);

        public static void Main()
        {
            // write your code here

            Thread.Sleep(Timeout.Infinite);
        }



        static Timer hallTimer = new Timer(delegate
            {
                GlobalVariables.hall1value = hallIn1.Read();
                Debug.Print("Hall 1: " + GlobalVariables.hall1value.ToString());

            },
             null,
             timerStart,
            timerPeriod);
    }
}
