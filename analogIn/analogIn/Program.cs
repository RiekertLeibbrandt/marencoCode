using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace analogIn
{
    public class Program
    {
        public static void Main()
        {
            // write your code here

            int anaIn = 250; // ADC is 12-bit Resolution
            //SecretLabs.NETMF.Hardware.AnalogInput pinA4 = new Microsoft.NETMF.Hardware.AnalogInput(Cpu.AnalogChannel.ANALOG_4);
            SecretLabs.NETMF.Hardware.AnalogInput pinA4 = new SecretLabs.NETMF.Hardware.AnalogInput(Pins.GPIO_PIN_A4);

            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            //PWM led2 = new PWM(Cpu.PWMChannel.PWM_0, 100, 100, 0);
            PWM servo = new PWM(Cpu.PWMChannel.PWM_0,20000,1500,PWM.ScaleFactor.Microseconds, false);
            servo.Start();
            
            servo.DutyCycle = 0;
            servo.Duration = (uint)1500;

            while (true)
            {

                double anaRead = pinA4.Read();
                anaIn = (int)anaRead;

                //LED stuff

                //led.Write(true); // turn on the LED
                //Thread.Sleep(anaIn); // sleep for 250ms
                //led.Write(false); // turn off the LED
                //Thread.Sleep(anaIn); // sleep for 250ms
                          


                servo.Duration = (uint)((1000+anaRead));


            }
 


        }

    }
}
