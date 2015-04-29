using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace canTest
{
    public class Program
    {
        static public UInt16 mask = 0;                          // Filter and mask  8 16
        static public UInt16 filter = 0x0;                    // 0x7f7 and 0x7ef

        static public MCP2515 CANHandler = new MCP2515();
        static public MCP2515.CANMSG txMessage = new MCP2515.CANMSG();

        public static OutputPort canCS = new OutputPort(Pins.GPIO_PIN_D2, true); 
        
        public static void Main()
        {
            txMessage.CANID = 0x09;
            txMessage.data[0] = 0x11;
            txMessage.data[1] = 0x22;
            txMessage.data[2] = 0x33;
            txMessage.data[3] = 0x44;
            txMessage.data[4] = 0x55;
            txMessage.data[5] = 0x66;
            txMessage.data[6] = 0x77;
            txMessage.data[7] = 0x88;

            canCS.Write(false);
            Thread.Sleep(50);
            CANHandler.InitCAN(MCP2515.enBaudRate.CAN_BAUD_500K, filter, mask);
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            canCS.Write(false);
            Thread.Sleep(50);
            CANHandler.canReset();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            // SEt baud rate
            //canCS.Write(false);
            //Thread.Sleep(50);
            //CANHandler.SetCANBaud(MCP2515.enBaudRate.CAN_BAUD_500K);
            //Thread.Sleep(50);
            //canCS.Write(true);
            //Thread.Sleep(50);

            // Baud rate commands split up so we can manually do chip select..
            canCS.Write(false);
            Thread.Sleep(50);
            CANHandler.baud1();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            canCS.Write(false);
            Thread.Sleep(50);
            CANHandler.baud2();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            canCS.Write(false);
            Thread.Sleep(50);
            CANHandler.baud3();
            Thread.Sleep(50);
            canCS.Write(true);
            Thread.Sleep(50);

            // Set normal mode
            canCS.Write(false);
            Thread.Sleep(50);
            CANHandler.SetCANNormalMode();
            canCS.Write(true);
            Thread.Sleep(50);

            while (true)
            {
                // The method with auto chip select
                //CANHandler.Transmit(txMessage, 100);
                
                // Now with manual chip select.
                canCS.Write(false);
                CANHandler.tId(txMessage);
                canCS.Write(true);

                canCS.Write(false);
                CANHandler.tRemote();
                canCS.Write(true);

                canCS.Write(false);
                CANHandler.tSend(txMessage);
                canCS.Write(true);

                canCS.Write(false);
                CANHandler.tTransmit();
                canCS.Write(true);

                Thread.Sleep(1000);
            }

        }

    }
}
