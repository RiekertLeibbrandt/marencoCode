using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace setBaudHC_06
{
    public class Program
    {

        public static void Main()
        {
            // A program to set the baud rate on a new HC-06 to
            //  115200
            //

            SerialPort blueComms = new SerialPort(SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One);

            blueComms.Open();

            //
            //  Rename the device
            //
            byte[] outBuffer = System.Text.Encoding.UTF8.GetBytes("AT+NAMEHC_A");
            blueComms.Write(outBuffer, 0, outBuffer.Length);
            Thread.Sleep(2000);
            while (blueComms.BytesToRead > 0)
            {
                byte[] a = new byte[1] { 0 };
                blueComms.Read(a, 0, 1);
                char[] cc = System.Text.Encoding.UTF8.GetChars(a, 0, a.Length);
                Debug.Print(cc.ToString());
                Thread.Sleep(10);
            }
            //
            //  Set the baud rate.
            //
            outBuffer = System.Text.Encoding.UTF8.GetBytes("AT+BAUD8");
            blueComms.Write(outBuffer, 0, outBuffer.Length);
            Thread.Sleep(2000);

            blueComms.Close();

            SerialPort blueFast = new SerialPort(SerialPorts.COM1, 115200, Parity.None, 8, StopBits.One);
            blueFast.Open();

            outBuffer = System.Text.Encoding.UTF8.GetBytes("AT");
            blueComms.Write(outBuffer, 0, outBuffer.Length);
            Debug.Print("After Baud rate set");

            while (blueComms.BytesToRead > 0)
            {
                byte[] a = new byte[1] { 0 };
                blueComms.Read(a, 0, 1);
                char[] cc = System.Text.Encoding.UTF8.GetChars(a, 0, a.Length);
                Debug.Print(cc.ToString());
                Thread.Sleep(10);
            }

        }
    }

}
