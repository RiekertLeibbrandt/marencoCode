using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace flap1
{
    public class Program
    {
        // Declare the interrupt ports
        static InterruptPort flapAngle = new InterruptPort(Pins.GPIO_PIN_D5, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static InterruptPort flapVertical = new InterruptPort(Pins.GPIO_PIN_D6, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static int angleCount = 0;
        static int verticalCount = 0;

        // Ethernet variables.


        public static void Main()
        {
            // Register interrupts.
            flapAngle.OnInterrupt += flapAngle_onInterrupt;
            flapVertical.OnInterrupt += flapVertical_onInterrupt;









            // Final Sleep.
            Thread.Sleep(Timeout.Infinite);
        }


        //
        //  Other functions.
        //

        static void flapAngle_onInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("Angle " + angleCount.ToString());
            angleCount = angleCount + 1;
        }

        static void flapVertical_onInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("Vertical " + verticalCount.ToString());
            verticalCount = verticalCount + 1;
        }

    }
}
