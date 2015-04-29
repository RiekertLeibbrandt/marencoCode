using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoApplication3
{
    public class Program
    {

        public static OutputPort rpmOut = new OutputPort(Pins.GPIO_PIN_D10,false);
        public static OutputPort ledOut = new OutputPort(Pins.GPIO_PIN_D8, true);

        static InterruptPort rpmIn = new InterruptPort(Pins.GPIO_PIN_D5,false,Port.ResistorMode.Disabled,Port.InterruptMode.InterruptEdgeHigh);

        

        public static void Main()
        {

            rpmIn.OnInterrupt += rpmIn_OnInterrupt;
            Thread.Sleep(Timeout.Infinite);
        }

        static void rpmIn_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("interrupt");
            rpmOut.Write(!rpmOut.Read());
            ledOut.Write(!ledOut.Read());
        }



    }
}