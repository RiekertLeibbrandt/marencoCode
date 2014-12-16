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

            AnalogInput input1 = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
            AnalogInput input2 = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);

            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            UInt16 j = 0;
            int rawValue1 = 0;
            int rawValue2 = 0;
            int rawAve1 = 0;
            int rawAve2 = 0;
            int i = 0;
            UInt16 uintIn1 = 0;
            UInt16 uintIn2 = 0;

            while (true)
            {
                                
                //  0 e 1023 (ADC a 10 bit)

                rawValue1 = 0;
                rawValue2 = 0;
                rawAve1 = 0;
                rawAve2 = 0;

                for (i = 1; i <= 50; i++)   // Take an average of 15 samples
                {
                    rawValue1 = input1.ReadRaw();
                    rawAve1 = rawAve1 + rawValue1;
                    rawValue2 = input2.ReadRaw();
                    rawAve2 = rawAve2 + rawValue2;
                    Thread.Sleep(2);
                }

                rawAve1 = rawAve1 / (i-1);
                rawAve2 = rawAve2 / (i-1);


                Debug.Print("Value 1 Ave: " + rawAve1.ToString() + "    Value 1 Raw: " + rawValue1.ToString() + "    Value 2 Ave: " + rawAve2.ToString() + "    Value 2 Raw: " + rawValue2.ToString());
                
                uintIn1 = (UInt16)rawAve1;
                uintIn2 = (UInt16)rawAve2;


                // ritorna un valore tra 0 ed 1 che va moltiplicato per
                // la ARef per ottenere il valore in Volt della tensione
                //double volt = input.Read() * 3.3;

                //if (strain1 > 4000)
                //{
                //    led.Write(true);
                //}
                //else
                //{
                //    led.Write(false);
                //}

           // This is very primitive for now. We must make a sync word or something to id the data on the other side

                byte[] junk = new byte[4] { (byte)(uintIn1 & 0xFF), (byte)((uintIn1 >> 8) & 0xFF), (byte)(uintIn2 & 0xFF), (byte)((uintIn2 >> 8) & 0xFF)};

               // ser.Print(strain1);
               // Thread.Sleep(5);
                ser.Print(junk);
               // Thread.Sleep(5);
            }

        }
    }
}
