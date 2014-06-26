using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace checkHAL
{
    public class Program
    {
        static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);

        public static void Main()
        {
            // write your code here

            InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

            hal.OnInterrupt += hal_OnInterrupt;

            Thread.Sleep(Timeout.Infinite);

        }

        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data2 == 0)
            {
                red.Write(true);
            }
            else
            {
                red.Write(false);
            }
        }

    }
}
