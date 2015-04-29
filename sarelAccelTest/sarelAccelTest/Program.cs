using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace sarelAccelTest
{
    public class Program
    {

        static public int accelPeriod = 500;                     // 10 hz
        static public int accelStart = 500;                     // Delay before timer starts
        static public double accelValue = 0;                     // Accelerometer value.

        // Port Definitions.
        public static AnalogInput accelIn = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);

        // Now do udp setup thing.
        //public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        //public static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.246"), 49000);

        public static byte[] crap = new byte[] {0x00};


        public static void Main()
        {

            Thread.Sleep(Timeout.Infinite);

        }


        static Timer readAccel = new Timer(delegate
        {
            accelValue = accelIn.Read();
            Debug.Print("Accelerometer: " + accelValue.ToString());
            //sock.SendTo(crap, sendingEndPoint);
        },
         null,
         accelStart,
        accelPeriod);
    }
}
