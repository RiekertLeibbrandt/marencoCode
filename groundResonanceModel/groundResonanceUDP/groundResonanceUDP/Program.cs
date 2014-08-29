using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;
using Marenco.Sensors;

//
//  A blade balancing application based on
//  a Hal effect sensor for azimuth, a 3 axis
//  acceleraqtion and a blue tooth serial data 
//  write. This was written to balance the blades 
//  an the new rotor head model and to use during
//  testing.
//
//  Becker and Riekert, June 2014.
//
//  June 24
//  Almost brke the model.
//  Becker changes Baud rate to 57600 and
//  scale the power to half for 256
//
//  28 June, pull speed change out of read interrupt,
//  reduce time to 24 bits and in milliseconds.
//  
//  13 August, add an Analog accelerometer.
//  Write out interrupt time as well.
//

namespace marencoTune
{
    public class Program
    {
        public const int maxSpeed = 70;    // This is effectively ground idle. 180;
        //  Global instances
        static InterruptPort dataReady = new InterruptPort(Pins.GPIO_PIN_D10, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        // Channel on the top end of the board.
        public static PWM motorDrive = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)20000, (UInt32)1050, PWM.ScaleFactor.Microseconds, false);
        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static ADXL345 acc = new ADXL345(Pins.GPIO_PIN_A5, 1000);
        static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);
        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        public static Socket sockOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static Socket sockIn = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.231"), 49001);
        public static IPEndPoint recieveEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.238") , 49002);

        public static AnalogInput analogIn = new AnalogInput(Cpu.AnalogChannel.ANALOG_4);

        //
        //  Get power setting from UDP
        //


        public static void Main()
        {
            // Start everything
            hal.OnInterrupt += hal_OnInterrupt;
            acc.setRange(enRange.range2g);
            acc.setUpInterrupt();
            acc.clearInterrupt();
            dataReady.OnInterrupt += dataReady_OnInterrupt;
            motorDrive.Start();
            byte[] outBuffer = System.Text.Encoding.UTF8.GetBytes("Motor active");
            acc.setUpAccelRate(200);

            //
            //  Bind the IP address, etc
            //

            Debug.Print(NI.IPAddress.ToString());
            sockIn.ReceiveTimeout = 5;
            sockIn.Bind(recieveEndPoint);

            //Int16 a = -1;
            //a = 0;
            //a = 1;

            //
            //  Set zero time
            //

            //
            //  Ramp up the rotor speed
            //
            //int speedNow = 60;          // Minimum speed.
            //Thread.Sleep(6000);         // Wait for humans to complete connection
            //while (speedNow < maxSpeed)
            //{
            //    speedNow++;
            //    motorDrive.Duration = (UInt32)((double)speedNow * 980d / 512d + 1000);

            //    //
            //    //  This loop is now obsolete. Speed is set in Matlab
            //    //
            //    if (speedNow > 70)
            //    {
            //        Thread.Sleep(1000);   // Was 2000
            //    }
            //    else
            //    {
            //        Thread.Sleep(50);
            //    }
            //}


            //  Snooze
            Thread.Sleep(Timeout.Infinite);
        }

        static void dataReady_OnInterrupt(uint data1, uint data2, DateTime time)
        {

            acc.getValues(ref GlobalVariables.x, ref GlobalVariables.y, ref GlobalVariables.z);
            acc.clearInterrupt();

            UInt16 xDig = (UInt16) (- GlobalVariables.x + 2048);

            UInt16 zDig = (UInt16) (-GlobalVariables.z + 2048);

            //
            //  Conver to bytes and write out.
            // Optimal synch word from http://www2.l-3com.com/tw/telemetry_tutorial/r_frame_synchronization_pattern.html
            //  Stuff it in the beginning. For now just use two Synch bytes
            //
            //  In an effort to save bytes change the synch word to FF, use only
            //  one and store the tymers into 24 bits.
            //  Also write every second time only.
            //  Make the minimum on setting 0x3C to save the motor
            //

                byte[] junk = new byte[8] { 
                    (byte)(zDig & 0xFF), 
                    (byte)((zDig >> 8) & 0xFF),                // Only 2 bytes
                    (byte)(xDig & 0xFF),
                    (byte)((xDig >> 8) & 0xFF),
                    (byte)(GlobalVariables.halTimeShifted & 0xFF), 
                    (byte)((GlobalVariables.halTimeShifted >> 8) & 0xFF),               // Only 2 bytes
                    (byte)(GlobalVariables.hertz & 0xFF), 
                    (byte)((GlobalVariables.hertz >> 8) & 0xFF)};               // Only 2 bytes
 
                sockOut.SendTo(junk, sendingEndPoint);

            //
            //  Get the power setting.
            //
                int bytesAvailable = sockIn.Available;
                if (bytesAvailable > 0)
                {
                    sockIn.Receive(GlobalVariables.throttleByte);
                }

                //
                //  Change to power setting accordingly.
                //
            if (GlobalVariables.throttleByte[0] != GlobalVariables.throttleOld)
            {
//                Debug.Print(GlobalVariables.throttleByte[0].ToString());
                motorDrive.Duration = (UInt32)((double) GlobalVariables.throttleByte[0] * 980d / 512d + 1000);
            }
            GlobalVariables.throttleOld = GlobalVariables.throttleByte[0];
        }



        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (GlobalVariables.dateSet)
            {
                GlobalVariables.ticksZero = time.Ticks;
                GlobalVariables.dateSet = false;
            }
            else
            {
                GlobalVariables.halTime = (time.Ticks - GlobalVariables.ticksZero) / 10000000.0d;
 //               Debug.Print(GlobalVariables.halTime.ToString());
                GlobalVariables.halTimeShifted = (UInt16)(GlobalVariables.halTime * 1000.0d);
                GlobalVariables.hertz = (Int16)(1000.0d / (GlobalVariables.halTime - GlobalVariables.halTimeOld));
                GlobalVariables.halTimeOld = GlobalVariables.halTime;
                green.Write(!green.Read());
            }
        }

    }
}
