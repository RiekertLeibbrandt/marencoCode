using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Marenco.Comms;





namespace leadLagSensor
{
    public class Program
    {
        public static void Main()
        {
            // write your code here

            BlueSerial ser = new BlueSerial();

            AnalogInput input = new AnalogInput(AnalogChannels.ANALOG_PIN_A4);

            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            UInt16 j = 0;
            int rawValue = 0;
            UInt16 ll = 0;

            while (true)
            {


                
                //  0 e 1023 (ADC a 10 bit)
                rawValue = input.ReadRaw();
               // Debug.Print(rawValue.ToString());

                ll =  (UInt16)rawValue;
                

                // ritorna un valore tra 0 ed 1 che va moltiplicato per
                // la ARef per ottenere il valore in Volt della tensione
                //double volt = input.Read() * 3.3;

                if (ll > 4000)
                {
                    led.Write(true);
                }
                else
                {
                    led.Write(false);
                }


                ser.Print(ll);
                Thread.Sleep(5);
            }

        }
    }
}
