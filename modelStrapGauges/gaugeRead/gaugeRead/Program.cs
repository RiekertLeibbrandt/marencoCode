// Reads the 2 loadcells attached to the bearing test setup. The 200kN loadcell is attached to channel 1 with a gain of 330 (150 ohm). The 2kN loadcell is on channel 2 with a gain of 225 (220 ohm). 
// Last updated on 06/01/2015

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

            AnalogInput input1 = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);  // 2 kN loadcell
            AnalogInput input2 = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);  // 200 kN loadcell

            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            UInt16 j = 0;
            int rawValue1 = 0;
            int rawValue2 = 0;
            float rawAve1kN = 0;
            float rawAve2kN = 0;
            int rawAve1 = 0;
            int rawAve2 = 0;
            int i = 0;
            UInt16 uintIn1 = 0;
            UInt16 uintIn2 = 0;

            while (true)
            {
                rawValue1 = 0;
                rawValue2 = 0;
                rawAve1 = 0;
                rawAve2 = 0;

                for (i = 1; i <= 50; i++)   // Take an average of 50 samples
                {
                    rawValue1 = input1.ReadRaw();
                    rawAve1 = rawAve1 + rawValue1;
                    rawValue2 = input2.ReadRaw();
                    rawAve2 = rawAve2 + rawValue2;
                    Thread.Sleep(2);
                }

                rawAve1 = rawAve1 / (i-1);
                rawAve2 = rawAve2 / (i-1);

                rawAve1kN = (float)((rawAve1 - 2084) / -943.20518975);  // 2 kN loadcell calibration under "loadcellcalibration.xlsx"
                rawAve2kN = (float)((rawAve2 - 2064) / -13.75116883);  // 200 kN loadcell calibration under "loadcellcalibration.xlsx"


                // Print some values to the output window. This is primative, but works for now.

                Debug.Print("Value 1 Ave: " + rawAve1.ToString() + "    Value 1 Raw: " + rawValue1.ToString() + "    Value 1 kN: " + rawAve1kN.ToString() + "    Value 2 Ave: " + rawAve2.ToString() + "    Value 2 Raw: " + rawValue2.ToString() + "    Value 2 kN: " + rawAve2kN.ToString());
                
                uintIn1 = (UInt16)rawAve1;
                uintIn2 = (UInt16)rawAve2;

                
                // Send the data via bluetooth

                byte[] data = new byte[4] { (byte)(uintIn1 & 0xFF), (byte)((uintIn1 >> 8) & 0xFF), (byte)(uintIn2 & 0xFF), (byte)((uintIn2 >> 8) & 0xFF)};  // Small loadcell first, then large loadcell

               // ser.Print(strain1);
               // Thread.Sleep(5);
                ser.Print(data);
               // Thread.Sleep(5);
            }

        }
    }
}
