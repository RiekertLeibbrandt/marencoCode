using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;
using Marenco.Sensors;

namespace canSasol
{

    public static class GlobalVariables
    {
        public static int canTimeOut = 10;
        // Accelerometer data.
        public static int x1 = 0;
        public static int y1 = 0;
        public static int z1 = 0;

        public static int x2 = 0;
        public static int y2 = 0;
        public static int z2 = 0;

        // Distance sensor.
        public static int distanceReading = 0;

        // Wheel Position
        public static UInt32 wheelTime = 0;
        public static Int32 wheelCount = 0;

        // The spi bus.
        public static ADXL345 spiBus = null;

        // CAN messages
        public static ADXL345.CANMSG cmA1 = new ADXL345.CANMSG();
        public static ADXL345.CANMSG cmA2 = new ADXL345.CANMSG();
        public static ADXL345.CANMSG cmWP = new ADXL345.CANMSG();
        public static ADXL345.CANMSG cmAD = new ADXL345.CANMSG();

        // Byte data.
        public static byte[] x1B = new byte[] { 0x00, 0x00 };
        public static byte[] y1B = new byte[] { 0x00, 0x00 };
        public static byte[] z1B = new byte[] { 0x00, 0x00 };
        public static byte[] x2B = new byte[] { 0x00, 0x00 };
        public static byte[] y2B = new byte[] { 0x00, 0x00 };
        public static byte[] z2B = new byte[] { 0x00, 0x00 };
        public static byte[] distanceReadingB = new byte[] { 0x00, 0x00 };
        public static byte[] wheelTimeB = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        public static byte[] wheelCountB = new byte[] { 0x00, 0x00, 0x00, 0x00 };
    }
}
