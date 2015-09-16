using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace flap1
{
    public class Program
    {
        // Declare the interrupt ports
        static InterruptPort flapAngle = new InterruptPort(Pins.GPIO_PIN_D5, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static InterruptPort flapVertical = new InterruptPort(Pins.GPIO_PIN_D6, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        static int angleCount = 0;
        static int verticalCount = 0;
        static long previousTick1 = 0;
        static long previousTick2 = 0;
        static double sensorRadius = 0.800; // Radius of the sensor in meters.
        static long timeVert = 0;
        static long timeAngle = 0;
        static double speed = 0;
        static double flapHeight = 0;
        static double tan25 = System.Math.Tan(25 * System.Math.PI / 180);

        // Ethernet variables.
        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        public static Socket sockOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.239"), 49000); // James' desktop.

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
            timeAngle = time.Ticks/1000;

            // Calculate Period
            //long period = timeJunk - previousTick1;


            //byte[] sendJunk = new byte[12] {
            //    (byte)(1),       // 1 is the angled sensor
            //    (byte)(0),
            //    (byte)(0),
            //    (byte)(0),
            //    (byte)(timeJunk & 0xFF), 
            //    (byte)((timeJunk >> 8) & 0xFF), 
            //    (byte)((timeJunk >> 16) & 0xFF), 
            //    (byte)((timeJunk >> 24) & 0xFF),
            //    (byte)(period  & 0xFF),    //Period
            //    (byte)((period >> 8) & 0xFF),
            //    (byte)((period >> 16) & 0xFF),
            //    (byte)((period >> 24) & 0xFF)
            //};
            
            //sockOut.SendTo(sendJunk,sendingEndPoint);

            //previousTick1 = timeJunk;

            //Debug.Print("Angle " + angleCount.ToString());
            //angleCount = angleCount + 1;
        }


        // This one should always fire second, so we do calculations in here.
        static void flapVertical_onInterrupt(uint data1, uint data2, DateTime time)
        {
            timeVert = time.Ticks / 1000;

            // Calculate Period and speed
            long period = timeVert - previousTick2;
            speed = 2 * sensorRadius * System.Math.PI / ((double)period / 10000);

            // Calculations
            double distance = (double)(timeVert - timeAngle) / 10000 * speed;

            flapHeight = distance / tan25; // In meters (double)

            long flapJunk = (long)(flapHeight * 1000);  // in mm


            // Send data

            byte[] sendJunk = new byte[8] {
                (byte)(period  & 0xFF),    // period
                (byte)((period >> 8) & 0xFF),
                (byte)((period >> 16) & 0xFF),
                (byte)((period >> 24) & 0xFF),
                (byte)(flapJunk  & 0xFF),    // Flap height
                (byte)((flapJunk >> 8) & 0xFF),
                (byte)((flapJunk >> 16) & 0xFF),
                (byte)((flapJunk >> 24) & 0xFF)
            };

            sockOut.SendTo(sendJunk, sendingEndPoint);

            previousTick2 = timeVert;
            //Debug.Print("Vertical " + verticalCount.ToString());
            //verticalCount = verticalCount + 1;
        }
    }
}
