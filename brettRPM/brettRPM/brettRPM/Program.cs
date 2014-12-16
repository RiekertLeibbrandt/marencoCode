using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;

namespace brettRPM
{
    public class Program
    {
        public static InterruptPort hal = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
        public static OutputPort green = new OutputPort(Pins.GPIO_PIN_D8, false);
        public static BlueSerial serial = new BlueSerial();

        static Timer sendTimer = new Timer(delegate
        {
            byte[] send = System.Text.Encoding.UTF8.GetBytes(GlobalVariables.rpm.ToString() + "\n");
            //Debug.Print(send.ToString());
            serial.Print(send);
        }, null, 1000, 200);
        
        public static void Main()
        {
            
            hal.OnInterrupt += hal_OnInterrupt;
            Thread.Sleep(Timeout.Infinite);
            
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
                //Debug.Print(GlobalVariables.halTime.ToString());
                GlobalVariables.halTimeShifted = (UInt16)(GlobalVariables.halTime * 1000.0d);
                GlobalVariables.hertz = (Int16)(1000.0d / (GlobalVariables.halTime - GlobalVariables.halTimeOld));
                GlobalVariables.rpm = (Int16)(GlobalVariables.hertz / 1000.0d * 60.0d);
                //Debug.Print(GlobalVariables.rpm.ToString());
               

                GlobalVariables.halTimeOld = GlobalVariables.halTime;
                green.Write(!green.Read());
            }
        }


    
    }


}
