using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Sensors;

namespace checkADXL345
{
    public class Program
    {
        static InterruptPort dataReady = new InterruptPort(Pins.GPIO_PIN_D10, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        public static ADXL345 acc = new ADXL345(Pins.GPIO_PIN_A5, 100);
        public static int x = 0;
        public static int y = 0;
        public static int z = 0;

        public static void Main()
        {
            // write your code here

            acc.setUpInterrupt();
            acc.clearInterrupt();


            dataReady.OnInterrupt += dataReady_OnInterrupt;

            Thread.Sleep(Timeout.Infinite);

        }

        static void dataReady_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            ulong ticksOut = (ulong) time.Ticks;
            led.Write(!led.Read());
            acc.getValues(ref x, ref y, ref z);
            acc.clearInterrupt();
            Debug.Print(ticksOut.ToString() + " " + x.ToString());
        }

    }
}
