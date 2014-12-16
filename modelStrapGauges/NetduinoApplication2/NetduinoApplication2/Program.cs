using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoApplication2
{
    public class Program
    {

        static OutputPort relay = new OutputPort(Pins.GPIO_PIN_A0, false);
        static OutputPort greenLed = new OutputPort(Pins.GPIO_PIN_D8, false);
        static OutputPort redLed = new OutputPort(Pins.GPIO_PIN_D7, true);

        public static void Main()
        {

            while (true)
            {
               // Thread.Sleep(5000);
                relay.Write(true);
                greenLed.Write(!greenLed.Read());
                redLed.Write(!redLed.Read());
//                Debug.Print("ON");
                Thread.Sleep(2000);
                relay.Write(false);
                greenLed.Write(!greenLed.Read());
                redLed.Write(!redLed.Read());
//                Debug.Print("OFF");
                Thread.Sleep(2000);
                // write your code here
            }

        }

    }
}
