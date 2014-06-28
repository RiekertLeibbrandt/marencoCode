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

namespace marencoTune
{
    public class Program
    {
        //  Global instances
        static InterruptPort dataReady = new InterruptPort(Pins.GPIO_PIN_D10, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static SerialPort blueComms = new SerialPort(SerialPorts.COM1, 115200, Parity.None, 8, StopBits.One);
        // Channel on the top end of the board.
        public static PWM motorDrive = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32) 20000, (UInt32) 1050, PWM.ScaleFactor.Microseconds, false);
        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static ADXL345 acc = new ADXL345(Pins.GPIO_PIN_A5, 200);
        static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);


        public static void Main()
        {
            // Start everything

            blueComms.Open();
            blueComms.DataReceived += blueComms_DataReceived;
            hal.OnInterrupt += hal_OnInterrupt;
            acc.setUpInterrupt();
            acc.clearInterrupt();
            dataReady.OnInterrupt += dataReady_OnInterrupt;
            motorDrive.Start();
            blueComms.Write(new byte[] { 0x34,0x35 }, 0, 1);
            byte[] outBuffer = System.Text.Encoding.UTF8.GetBytes("Motor active");
            blueComms.Write(outBuffer, 0, outBuffer.Length);
            acc.setUpAccelRate(200);

            //  Snooze
            Thread.Sleep(Timeout.Infinite);
        }

        static void dataReady_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            UInt32 ticksOut = (UInt32) time.Ticks;
            acc.getValues(ref GlobalVariables.x, ref GlobalVariables.y, ref GlobalVariables.z);
            acc.clearInterrupt();

            //
            //  Conver to bytes and write out.
            // Optimal synch word from http://www2.l-3com.com/tw/telemetry_tutorial/r_frame_synchronization_pattern.html
            //  Stuff it in the beginning. For now just use two Synch bytes
            //
            byte[] junk = new byte[14] { 0x16, 0x16,
                    (byte)(ticksOut & 0xFF), 
                    (byte)((ticksOut >> 8) & 0xFF), 
                    (byte)((ticksOut >> 16) & 0xFF), 
                    (byte)((ticksOut >> 24) & 0xFF),                   
                    (byte)(GlobalVariables.z & 0xFF), 
                    (byte)((GlobalVariables.z >> 8) & 0xFF), 
                    (byte)((GlobalVariables.z >> 16) & 0xFF), 
                    (byte)((GlobalVariables.z >> 24) & 0xFF),
                    (byte)(GlobalVariables.halTime & 0xFF), 
                    (byte)((GlobalVariables.halTime >> 8) & 0xFF), 
                    (byte)((GlobalVariables.halTime >> 16) & 0xFF), 
                    (byte)((GlobalVariables.halTime >> 24) & 0xFF) };

            blueComms.Write(junk, 0, junk.Length);
        }

        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            GlobalVariables.halTime = (UInt32)time.Ticks;
            red.Write(!red.Read());
        }

        //
        //  All bytes are treated as throttle settings.
        //
        static void blueComms_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int deltaT = 0;
            int nSteps = 0;
            int throttleNow = 0;

            while (blueComms.BytesToRead > 0)
            {
                blueComms.Read(GlobalVariables.throttleSetting, 0, GlobalVariables.throttleSetting.Length);
            }
            //
            //  Slowly ramp up to the new setting.
            //
            if (GlobalVariables.throttleSetting[0] < 0x24) GlobalVariables.throttleSetting[0] = 0x12;

            throttleNow = GlobalVariables.throttleSettingOld;
            if (GlobalVariables.throttleSetting[0] - GlobalVariables.throttleSettingOld > 0)
            {
                deltaT = 1;
                nSteps = GlobalVariables.throttleSetting[0] - GlobalVariables.throttleSettingOld;
            }
            else
            {
                deltaT = -1;
                nSteps = -(GlobalVariables.throttleSetting[0] - GlobalVariables.throttleSettingOld);
            }

            for (int i = 0; i < nSteps; i++)
            {
                throttleNow = throttleNow + deltaT;
                motorDrive.Duration = (UInt32)((double)throttleNow * 980d / 512d + 1000);
                Thread.Sleep(150);
            }
            GlobalVariables.throttleSettingOld = GlobalVariables.throttleSetting[0];
        }
    }
}
