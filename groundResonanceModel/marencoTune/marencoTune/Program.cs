﻿using System;
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

namespace marencoTune
{
    public class Program
    {
        public const int maxSpeed = 65;
        //  Global instances
        static InterruptPort dataReady = new InterruptPort(Pins.GPIO_PIN_D10, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static SerialPort blueComms = new SerialPort(SerialPorts.COM1, 115200, Parity.None, 8, StopBits.One);
        // Channel on the top end of the board.
        public static PWM motorDrive = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)20000, (UInt32)1050, PWM.ScaleFactor.Microseconds, false);
        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static ADXL345 acc = new ADXL345(Pins.GPIO_PIN_A5, 1000);
        static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);


        public static void Main()
        {
            // Start everything
            blueComms.Open();
            blueComms.DataReceived += blueComms_DataReceived;
            hal.OnInterrupt += hal_OnInterrupt;
            acc.setRange(enRange.range2g);
            acc.setUpInterrupt();
            acc.clearInterrupt();
            dataReady.OnInterrupt += dataReady_OnInterrupt;
            motorDrive.Start();
            byte[] outBuffer = System.Text.Encoding.UTF8.GetBytes("Motor active");
            blueComms.Write(outBuffer, 0, outBuffer.Length);
            acc.setUpAccelRate(200);

            //Int16 a = -1;
            //a = 0;
            //a = 1;

            //
            //  Ramp up the rotor speed
            //
             int speedNow = 0x3c;
             Thread.Sleep(6000);
            while (speedNow < maxSpeed)
            {
                speedNow ++;
                motorDrive.Duration = (UInt32)((double) speedNow * 980d / 512d + 1000);
                Thread.Sleep(150);
            }


            //  Snooze
            Thread.Sleep(Timeout.Infinite);
        }

        static void dataReady_OnInterrupt(uint data1, uint data2, DateTime time)
        {

            acc.getValues (ref GlobalVariables.x, ref GlobalVariables.y, ref GlobalVariables.z);
            acc.clearInterrupt();
            Int16 zShort = (Int16) GlobalVariables.z;

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

            if (GlobalVariables.writeNow)
            {
                GlobalVariables.writeNow = false;
                UInt32 ticksOut = (UInt32)time.Ticks;
                byte[] junk = new byte[6] { 
                    0x41, 0x42,                                                  // Synch word, stick with 2
                //    (byte)((ticksOut >> 8) & 0xFF), 
                //    (byte)((ticksOut >> 16) & 0xFF),                        // Only three bytes                
                //    (byte)((ticksOut >> 24) & 0xFF),                        // Only three bytes                
                    (byte)(GlobalVariables.zKeep & 0xFF), 
                    (byte)((GlobalVariables.zKeep >> 8) & 0xFF),                // Only 2 bytes
                    (byte)(zShort & 0xFF), 
                    (byte)((zShort >> 8) & 0xFF)};               // Only 2 bytes
                    //(byte)((GlobalVariables.halTime >> 8) & 0xFF), 
                    //(byte)((GlobalVariables.halTime >> 16) & 0xFF),        // Only 3 bytes
                    //(byte)((GlobalVariables.halTime >> 24) & 0xFF)};        // Only 3 bytes
                blueComms.Write(junk, 0, junk.Length);
                Debug.Print(zShort.ToString());
            }
            else
            {
                GlobalVariables.writeNow = true;
                GlobalVariables.zKeep = zShort;
            }
        }

        static void hal_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            GlobalVariables.halTime = (UInt32) time.Ticks;
            green.Write(!green.Read());
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
            if (GlobalVariables.throttleSetting[0] < 0x24) GlobalVariables.throttleSetting[0] = 0x24;

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
 //               motorDrive.Duration = (UInt32)((double)throttleNow * 980d / 512d + 1000);
                Thread.Sleep(150);
            }
            GlobalVariables.throttleSettingOld = GlobalVariables.throttleSetting[0];
        }

    }
}
