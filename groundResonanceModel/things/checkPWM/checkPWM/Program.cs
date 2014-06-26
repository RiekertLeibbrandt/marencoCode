using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace checkPWM
{
    public class Program
    {
        public static PWM junk = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, 20000, 1500, PWM.ScaleFactor.Microseconds, false);
        public static void Main()
        {
            // write your code here
            while (true)
            {
                junk.Duration = 1100;
                Thread.Sleep(500);
                junk.Duration = 1900;
                Thread.Sleep(500);

            }
            

        }

    }
}
