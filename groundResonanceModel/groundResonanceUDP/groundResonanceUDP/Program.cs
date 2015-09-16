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
//  31 October
//  James modifies this for the new ring model. We
//  now have full collective/cyclic control via pwm output.
//  This is driven by a MATLAB script. 
//


namespace marencoTune
{
    public class Program
    {
        public const int maxSpeed = 70;    // This is effectively ground idle. 180;
        //  Global instances
       // static InterruptPort dataReady = new InterruptPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

        public static InterruptPort accelInt = new InterruptPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);   
        // Define PWM channels
        // Channel on the top end of the board. Pins D 5,6,9,10
        // Servo 1 is the back
        // Servo 2 is the right one.
        // Servo 3 is the left one.
        public static PWM motorDrive = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)20000, (UInt32)1050, PWM.ScaleFactor.Microseconds, false);
        public static PWM servo1 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D6, (UInt32)20000, (UInt32)1500, PWM.ScaleFactor.Microseconds, false);
        public static PWM servo2 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D9, (UInt32)20000, (UInt32)1100, PWM.ScaleFactor.Microseconds, false);
        public static PWM servo3 = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D10, (UInt32)20000, (UInt32)1800, PWM.ScaleFactor.Microseconds, false);
                
        // Now other crap.
        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static ADXL345 acc = new ADXL345(Pins.GPIO_PIN_A5, 1000);
        static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);
        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        public static Socket sockOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static Socket sockIn = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.239"), 49001);    // This is James' desktop.
        public static IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.238") , 49002);   // This is the netduino's ip.

        //public static AnalogInput analogIn = new AnalogInput(Cpu.AnalogChannel.ANALOG_4);

        //
        //  Get power setting from UDP
        //


        public static void Main()
        {
            // Start everything
            hal.OnInterrupt += hal_OnInterrupt;
            acc.setUpAccelRate(200);
            acc.setRange(enRange.range2g);
            Thread.Sleep(100);
            acc.setUpInterrupt();
            Thread.Sleep(100);
            acc.clearInterrupt();
            Thread.Sleep(100);
            accelInt.OnInterrupt += accelInt_OnInterrupt;
            
            // Start the pwms.
            motorDrive.Start();
            servo1.Start();
            servo2.Start();
            servo3.Start();

  //          byte[] outBuffer = System.Text.Encoding.UTF8.GetBytes("Motor active");
           

            

            //
            //  Bind the IP address, etc
            //

            //Debug.Print(NI.IPAddress.ToString());
            //sockIn.ReceiveTimeout = 5;
            //sockIn.Bind(receiveEndPoint);
            Debug.Print("Some crap");
            //  Snooze
            


            Thread.Sleep(Timeout.Infinite);
        }

        static void accelInt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("Interrupting Sheep");

            acc.getValues(ref GlobalVariables.x, ref GlobalVariables.y, ref GlobalVariables.z);
            acc.clearInterrupt();

            //UInt16 xDig = (UInt16) (- GlobalVariables.x + 2048);

            //UInt16 zDig = (UInt16) (-GlobalVariables.z + 2048);

            //
            //  Convert to bytes and write out.
            //  Optimal sync word from http://www2.l-3com.com/tw/telemetry_tutorial/r_frame_synchronization_pattern.html
            //  Stuff it in the beginning. For now just use two Sync bytes
            //
            //  In an effort to save bytes change the sync word to FF, use only
            //  one and store the timers into 24 bits.
            //  Also write every second time only.
            //  Make the minimum on setting 0x3C to save the motor
            //

            //byte[] junk = new byte[8] { 
            //    (byte)(zDig & 0xFF), 
            //    (byte)((zDig >> 8) & 0xFF),                // Only 2 bytes
            //    (byte)(xDig & 0xFF),
            //    (byte)((xDig >> 8) & 0xFF),
            //    (byte)(GlobalVariables.halTimeShifted & 0xFF), 
            //    (byte)((GlobalVariables.halTimeShifted >> 8) & 0xFF),               // Only 2 bytes
            //    (byte)(GlobalVariables.hertz & 0xFF), 
            //    (byte)((GlobalVariables.hertz >> 8) & 0xFF)};               // Only 2 bytes

            //  sockOut.SendTo(junk, sendingEndPoint);

            //
            //  Get the power setting and cyclic settings
            //
            // int bytesAvailable = sockIn.Available;
            //Debug.Print(bytesAvailable.ToString());
            // if (bytesAvailable > 0)
            //{
            // I'm not sure of the flags here.
            //     int byteCount = sockIn.Receive(GlobalVariables.receivedMessage,4,SocketFlags.None);
            //     Debug.Print(byteCount.ToString());
            //      GlobalVariables.throttleByte[0] = GlobalVariables.receivedMessage[0];
            //  }

            //
            //  Change to power setting accordingly. We'll set the servos here as well.
            //
            //            if (GlobalVariables.throttleByte[0] != GlobalVariables.throttleOld)
            //            {
            ////                Debug.Print(GlobalVariables.throttleByte[0].ToString());
            //                motorDrive.Duration = (UInt32)((double) GlobalVariables.throttleByte[0] * 980d / 512d + 1000);

            //                // Do some mixing. Servos 1 & 3 are +, 2 is -. For collective.
            //                // We take everything relative to 127. So it is 127 + collective. No cyclic for now.
            //                UInt32 servo1out = (UInt32)((double)(127 + (GlobalVariables.receivedMessage[3] - 127)) * 1000d / 256d + GlobalVariables.servo1offset);
            //                UInt32 servo2out = (UInt32)((double)(127 - (GlobalVariables.receivedMessage[3] - 127)) * 1000d / 256d + GlobalVariables.servo2offset);
            //                UInt32 servo3out = (UInt32)((double)(127 + (GlobalVariables.receivedMessage[3] - 127)) * 1000d / 256d + GlobalVariables.servo3offset);

            //                // Check limits.
            //                // 1.
            //                if (servo1out > GlobalVariables.servo1u)
            //                    servo1out = GlobalVariables.servo1u;
            //                if (servo1out < GlobalVariables.servo1l)
            //                    servo1out = GlobalVariables.servo1l;
            //                // 2.
            //                if (servo2out > GlobalVariables.servo2u)
            //                    servo2out = GlobalVariables.servo2u;
            //                if (servo2out < GlobalVariables.servo2l)
            //                    servo2out = GlobalVariables.servo2l;
            //                // 3.
            //                if (servo3out > GlobalVariables.servo3u)
            //                    servo3out = GlobalVariables.servo3u;
            //                if (servo3out < GlobalVariables.servo3l)
            //                    servo3out = GlobalVariables.servo3l;

            //                // Set PWMs.
            //                servo1.Duration = servo1out;
            //                servo2.Duration = servo2out;
            //                servo3.Duration = servo3out;

            //            }
            //            GlobalVariables.throttleOld = GlobalVariables.throttleByte[0];
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
