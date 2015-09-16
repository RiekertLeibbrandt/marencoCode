using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace NetduinoApplication1
{
    public class Program
    {
        public const int nSamples = 16000;
        public const UInt32 armPos = 1200;
        public const UInt32 relPos = 600;
        public const int waitTime = 1000;

        public static AnalogInput accRead = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
        public static SerialPort serial = new SerialPort(SerialPorts.COM1, 115200, Parity.None, 8, StopBits.None);
        public static ushort[] reading = new ushort[nSamples];
        public static ushort[] readTime = new ushort[nSamples];
        public static PWM flex = new PWM(SecretLabs.NETMF.Hardware.Netduino.PWMChannels.PWM_PIN_D5, (UInt32)5000, armPos, PWM.ScaleFactor.Microseconds, false);
        public const Int64 ticksPerMicroSecond = System.TimeSpan.TicksPerMillisecond / 1000;
        public static OutputPort red = new OutputPort(Pins.GPIO_PIN_D7, true);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, true);

        public static void Main()
        {
            // write your code here
            flex.Start();
            serial.DataReceived += serial_DataReceived;
            serial.Open();
            Thread.Sleep(Timeout.Infinite);
        }

        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
//            throw new NotImplementedException();
            byte[] bytes = new byte[serial.BytesToRead];
            serial.Read(bytes, 0, bytes.Length);
            //
            //  "a" received, move the servo and start
            //  recording
            //
            if (bytes[0] == 97)
            {
                red.Write(false);
                green.Write(false);
                Thread.Sleep(waitTime);
                red.Write(true);
                flex.Duration = relPos;
                collectData();
                green.Write(true);
                red.Write(false);
                flex.Duration = armPos;
            }
            //
            //  "b" received, write the data to the port
            //
            else if (bytes[0] == 98)
            {
                for (int i = 0; i < nSamples; i++)
                {                   
                    byte[] send = System.Text.Encoding.UTF8.GetBytes(readTime[i].ToString() + " " + reading[i].ToString() + "\n");
                    serial.Write(send, 0, send.Length);
                    red.Write(true);
                    green.Write(true);
                }
            }
        }
        static void collectData()
        {
            for (int i = 0; i < nSamples; i++)
            {
                reading[i] = (ushort) accRead.ReadRaw();
                readTime[i] = (ushort) (Utility.GetMachineTime().Ticks / ticksPerMicroSecond);
            }
        }
    }
}
