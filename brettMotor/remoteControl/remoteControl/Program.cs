using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace remoteControl
{
    //
    //  Simple speed and power controller for Bret's motor.
    //  Becker January 2015.
    //
    //  All in one file at the moment
    //  Use D5 and D6 for outputs.
    //  use D9 and D10 for the interrupts.
    //  Be careful not to connect stuff to d5 and d6
    //
    public class Program
    {
        //
        //  User variables
        //
        public const double maxFreq = 50;           // Maximum frequency in Hz.
        public const double maxPower = .8;         // Maximum fraction of power applied
        //
        //  Declarations
        //
        // LED's
        public static OutputPort blue = new OutputPort(Pins.ONBOARD_LED, false);
        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);
        // Interrupts
        public static InterruptPort freqPulse = new InterruptPort(Pins.GPIO_PIN_D5, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
        public static InterruptPort powerPulse = new InterruptPort(Pins.GPIO_PIN_D6, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
        // Motor driver pins
        public static OutputPort leftDrive = new OutputPort(Pins.GPIO_PIN_A1, false);
        public static OutputPort rightDrive = new OutputPort(Pins.GPIO_PIN_A2, true);
        //
        //  Global variables
        //
        public static double frequency = 0.5;
        public static double pulseFracOn = .1;
        public const long ticksPerMicro = TimeSpan.TicksPerSecond / 1000000;
        public static long freqTime = 0;
        public static long powerTime = 0;
        public static long microSecondsPower = 0;
        public static long microSecondsFreq = 0;
        public static int timeOn = 0;
        public static int timeOff = 0;

        public static void Main()
        {
            //
            //  Set up the interrupt pulses.
            //

            freqPulse.OnInterrupt += freqPulse_OnInterrupt;
            powerPulse.OnInterrupt += powerPulse_OnInterrupt;

            //
            //  Main timing loop.
            while (true)
            {
                if (frequency > 0.5d)
                {
                    red.Write(false);
                    green.Write(true);
                    blue.Write(!blue.Read());
                    timeOn = (int)(0.5d / frequency * pulseFracOn * 1000.0d);
                    timeOff = (int)(0.5d / frequency * (1.0d - pulseFracOn) * 1000.0d);
                    leftDrive.Write(true);
                    Thread.Sleep(timeOn);
                    leftDrive.Write(false);
                    Thread.Sleep(timeOff);
                    rightDrive.Write(true);
                    Thread.Sleep(timeOn);
                    rightDrive.Write(false);
                    Thread.Sleep(timeOff);
                }
                else
                {
                    leftDrive.Write(false);
                    rightDrive.Write(false);
                    red.Write(true);
                    green.Write(false);
                    Thread.Sleep(100);
                }
            }
        }

        static void powerPulse_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data2 == 0)
            // Pulse is low
            {
                microSecondsPower = (time.Ticks - powerTime) / ticksPerMicro;
                if (microSecondsPower > 1200)
                {
                    pulseFracOn = (microSecondsPower - 1200) / 900.0d * maxPower;
                }
                else
                {
                    pulseFracOn = 0.0d;
                }
            }
            else
            //  Pulse is high
            {
                powerTime = time.Ticks;
            }
//            Debug.Print("Power " + pulseFracOn.ToString());

        }

        static void freqPulse_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data2 == 0)
            // Pulse is low
            {
                microSecondsFreq = (time.Ticks - freqTime) / ticksPerMicro;
                if (microSecondsFreq > 1100)
                {
                    frequency = (double)((microSecondsFreq - 1200) / 900.0d * maxFreq);
                }
                else
                {
                    frequency = 0.5d;
                }
            }
            else
            //  Pulse is high
            {
                freqTime = time.Ticks;
            }
 //           Debug.Print(frequency.ToString());
        }
    }
}
