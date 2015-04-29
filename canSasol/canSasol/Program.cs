// Left has suspension displacement and wheel sensor. Right has only accelerometers.
// Right works fine at 200 Hz.
// Left works fine at 200Hz.
// NB!. Remember to put debug off in the final version...

//#define debug
#define left
//#define right

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Sensors;


namespace canSasol
{
    public class Program
    {
        // Chip selects
        public static OutputPort acc1CS = new OutputPort(Pins.GPIO_PIN_A5, false);        
        // Use A4 for acc2 chip select.
        //public static OutputPort acc2CS = new OutputPort(Pins.GPIO_PIN_A4, false); 

        // We only use one interrupt. On this interrupt we read both accelerometer's data.
        static InterruptPort dataReady = new InterruptPort(Pins.GPIO_PIN_D10, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

        // Now some CAN variables. Chip select on D2 aparrently.
        public static OutputPort canCS = new OutputPort(Pins.GPIO_PIN_D2, false); 

        // Thread lock
        public static object aLock = new object();

#if left
        // Distance Sensor Reading Timer Constants
        static public int distancePeriod = 100;                     // 10 hz
        static public int distanceStart = 5000;                     // Delay before timer starts

        // Port Definitions.
       public static AnalogInput distanceIn = new AnalogInput(AnalogChannels.ANALOG_PIN_A3);

        // Wheel position interrupt on pin A2.
       //static OutputPort led = new OutputPort(Pins.GPIO_PIN_D7, false);
       static InterruptPort wheelInterrupt = new InterruptPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);

#endif

        public static void Main()
        {

            acc1CS.Write(true);
            //acc2CS.Write(true);
            canCS.Write(true);
            // Create accelerometer instances. We only create 1 spi bus. We then use chip select to choose what we are reading. We use this same spiBus for can.
            // I have hacked some can functions into the ADXL345 spi class. This is just how I ended up doing it...
            Thread.Sleep(500);
            acc1CS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus = new ADXL345(Pins.GPIO_NONE, 1000);
            acc1CS.Write(true);
            Thread.Sleep(50);
            
            //
            // Initialse CAN. I've had to hack out of the functions and do most of it manually here../
            //

            canCS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.canReset();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            // Baud rate commands split up so we can manually do chip select..
            canCS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.baud1();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            canCS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.baud2();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            canCS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.baud3();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            // Set normal mode
            canCS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.SetCANNormalMode();
            canCS.Write(true);
            Thread.Sleep(50);

            // Set some variables/parameters before we start.
#if left
            GlobalVariables.cmA1.CANID = 101;
            GlobalVariables.cmA2.CANID = 102;
            GlobalVariables.cmWP.CANID = 121;
            GlobalVariables.cmAD.CANID = 122;

#elif right
            GlobalVariables.cmA1.CANID = 111;
            GlobalVariables.cmA2.CANID = 112;
#endif

            // Initialise Accelerometer 1.
            Thread.Sleep(50);
            acc1CS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.setUpInterrupt();
            acc1CS.Write(true);
            Thread.Sleep(50);

            acc1CS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.clearInterrupt();
            acc1CS.Write(true);
            Thread.Sleep(50);
            
            acc1CS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.setUpAccelRate(200);
            acc1CS.Write(true);
            Thread.Sleep(50);

            // Now we setup accelerometer 2.
            //Thread.Sleep(50);
            //acc2CS.Write(false);
            //Thread.Sleep(50);
            //GlobalVariables.spiBus.quickSetup();
            //acc2CS.Write(true);
            //Thread.Sleep(50);

            //Thread.Sleep(50);
            //acc2CS.Write(false);
            //Thread.Sleep(50);
            //GlobalVariables.spiBus.setUpInterrupt();
            //acc2CS.Write(true);
            //Thread.Sleep(50);

            //acc2CS.Write(false);
            //Thread.Sleep(50);
            //GlobalVariables.spiBus.clearInterrupt();
            //acc2CS.Write(true);
            //Thread.Sleep(50);

            //acc2CS.Write(false);
            //Thread.Sleep(50);
            //GlobalVariables.spiBus.setUpAccelRate(200);
            //acc2CS.Write(true);
            //Thread.Sleep(50);

            Debug.Print("End of main...");
            dataReady.OnInterrupt += dataReady_OnInterrupt;

            // Do a clearing interrupt read of accelerometer 1, then sleep forever.
            acc1CS.Write(false);
            Thread.Sleep(50);
            GlobalVariables.spiBus.clearInterrupt();
            acc1CS.Write(true);
            Thread.Sleep(50);

#if left
            // Assign handler to wheel position interrupt.
            wheelInterrupt.OnInterrupt += wheelInterrupt_OnInterrupt;
#endif

            Thread.Sleep(Timeout.Infinite);
        }

#if left
        static void wheelInterrupt_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            // We put out 4 bytes counter, and 4 bytes time stamp.
            GlobalVariables.wheelTime = (UInt32)time.Ticks;
            GlobalVariables.wheelCount = GlobalVariables.wheelCount + 1;

            //led.Write(!led.Read());

            // Now get the bytes.
            GlobalVariables.wheelTimeB = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.wheelTime);
            GlobalVariables.wheelCountB = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.wheelCount);

#if debug
            Debug.Print("Wheel Pos: " + GlobalVariables.wheelCount.ToString() + " " + GlobalVariables.wheelTime.ToString());
#endif

            // Now assign bytes to the CAN messages.
            GlobalVariables.cmWP.data[0] = GlobalVariables.wheelCountB[0];
            GlobalVariables.cmWP.data[1] = GlobalVariables.wheelCountB[1];
            GlobalVariables.cmWP.data[2] = GlobalVariables.wheelCountB[2];
            GlobalVariables.cmWP.data[3] = GlobalVariables.wheelCountB[3];

            GlobalVariables.cmWP.data[4] = GlobalVariables.wheelTimeB[0];
            GlobalVariables.cmWP.data[5] = GlobalVariables.wheelTimeB[1];
            GlobalVariables.cmWP.data[6] = GlobalVariables.wheelTimeB[2];
            GlobalVariables.cmWP.data[7] = GlobalVariables.wheelTimeB[3];

            // Now transmit the CAN message.
            lock (aLock)
            {
                canCS.Write(false);
                GlobalVariables.spiBus.tId1(GlobalVariables.cmWP);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tId2(GlobalVariables.cmWP);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tRemote();
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tSend(GlobalVariables.cmWP);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tTransmit();
                canCS.Write(true);
            }
        }
#endif


        static void dataReady_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            lock (aLock)
            {
                // First read accelerometer 1 data.
                acc1CS.Write(false);
                GlobalVariables.spiBus.getValues(ref GlobalVariables.x1, ref GlobalVariables.y1, ref GlobalVariables.z1);
                acc1CS.Write(true);
                acc1CS.Write(false);
                GlobalVariables.spiBus.clearInterrupt();
                acc1CS.Write(true);
                // Second read accelerometer 2 data.
                //acc2CS.Write(false);
                //GlobalVariables.spiBus.getValues(ref GlobalVariables.x2, ref GlobalVariables.y2, ref GlobalVariables.z2);
                //acc2CS.Write(true);
                //acc2CS.Write(false);
                //GlobalVariables.spiBus.clearInterrupt();
                //acc2CS.Write(true);
            }

#if debug
             Debug.Print("Accel1: " + GlobalVariables.x1.ToString() + " " + GlobalVariables.y1.ToString() + " " + GlobalVariables.z1.ToString());
            //Debug.Print("Accel2: " + GlobalVariables.x2.ToString() + " " + GlobalVariables.y2.ToString() + " " + GlobalVariables.z2.ToString());
#endif
            // Write the data to bytes.
            GlobalVariables.x1B = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.x1);
            GlobalVariables.y1B = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.y1);
            GlobalVariables.z1B = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.z1);

            //GlobalVariables.x2B = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.x2);
            //GlobalVariables.y2B = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.y2);
            //GlobalVariables.z2B = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.z2);
            
            // Construct CAN messages.
            GlobalVariables.cmA1.data[0] = GlobalVariables.x1B[0];
            GlobalVariables.cmA1.data[1] = GlobalVariables.x1B[1];
            GlobalVariables.cmA1.data[2] = GlobalVariables.y1B[0];
            GlobalVariables.cmA1.data[3] = GlobalVariables.y1B[1];
            GlobalVariables.cmA1.data[4] = GlobalVariables.z1B[0];
            GlobalVariables.cmA1.data[5] = GlobalVariables.z1B[1];

            //GlobalVariables.cmA2.data[0] = GlobalVariables.x2B[0];
            //GlobalVariables.cmA2.data[1] = GlobalVariables.x2B[1];
            //GlobalVariables.cmA2.data[2] = GlobalVariables.y2B[0];
            //GlobalVariables.cmA2.data[3] = GlobalVariables.y2B[1];
            //GlobalVariables.cmA2.data[4] = GlobalVariables.z2B[0];
            //GlobalVariables.cmA2.data[5] = GlobalVariables.z2B[1];

            // Send CAN messages. Haven't checked CAN works properly yet.
            // Accel 1
            lock (aLock)
            {
                canCS.Write(false);
                GlobalVariables.spiBus.tId1(GlobalVariables.cmA1);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tId2(GlobalVariables.cmA1);
                canCS.Write(true);
    
                canCS.Write(false);
                GlobalVariables.spiBus.tRemote();
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tSend(GlobalVariables.cmA1);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tTransmit();
                canCS.Write(true);

                // Accel 2
                //canCS.Write(false);
                //GlobalVariables.spiBus.tId1(GlobalVariables.cmA2);
                //canCS.Write(true);

                //canCS.Write(false);
                //GlobalVariables.spiBus.tId2(GlobalVariables.cmA2);
                //canCS.Write(true);

                //canCS.Write(false);
                //GlobalVariables.spiBus.tRemote();
                //canCS.Write(true);

                //canCS.Write(false);
                //GlobalVariables.spiBus.tSend(GlobalVariables.cmA2);
                //canCS.Write(true);

                //canCS.Write(false);
                //GlobalVariables.spiBus.tTransmit();
                //canCS.Write(true);
            }
        }


#if left
        static Timer readDistanceSensor = new Timer(delegate
        {
            GlobalVariables.distanceReading = distanceIn.ReadRaw();
#if debug
            Debug.Print("Distance Sensor: " + GlobalVariables.distanceReading.ToString());
#endif
            // Construct the CAN message.
            GlobalVariables.distanceReadingB = LoveElectronics.Resources.BitConverter.GetBytes(GlobalVariables.distanceReading);
            GlobalVariables.cmAD.data[0] = GlobalVariables.distanceReadingB[0];
            GlobalVariables.cmAD.data[1] = GlobalVariables.distanceReadingB[1];

            // Send the CAN message.
            // Now with manual chip select. This is quite a procedure, but should work now.
            lock (aLock)
            {
                canCS.Write(false);
                GlobalVariables.spiBus.tId1(GlobalVariables.cmAD);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tId2(GlobalVariables.cmAD);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tRemote();
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tSend(GlobalVariables.cmAD);
                canCS.Write(true);

                canCS.Write(false);
                GlobalVariables.spiBus.tTransmit();
                canCS.Write(true);
            }
        },
         null,
         distanceStart,
        distancePeriod);
#endif
    }
}
