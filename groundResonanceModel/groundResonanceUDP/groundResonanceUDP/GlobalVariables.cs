using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;
namespace marencoTune
{

    public static class GlobalVariables
    {
        public static int rampUpPeriod = 100;

        public static byte[] throttleSetting = new byte[10] { 0, 22, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int x = 0;
        public static int y = 0;
        public static int z = 0;
        public static double halTime = 0;
        public static UInt16 halTimeShifted = 0;
        public static long ticksZero = 0;
        public static bool dateSet = true;
        public static bool writeNow = false;
        public static Int16 zKeep = 0;
        public static double halTimeOld = 0;
        public static Int16 hertz = 0;
        public static byte throttleSettingOld = 0x3C;
        public static byte[] throttleByte = new byte[1] { 0 };
        public static byte throttleOld = 0;
        public static byte[] receivedMessage = new byte[4] { 0, 127, 0,180 };

        // The servo offsets
        public const UInt32 servo1offset = 1000;    //1040
        public const UInt32 servo2offset = 900;     //580
        public const UInt32 servo3offset = 700;    //1400

        // Servo limits. I limit the pwm output, so that we can rezero and do whatever we want to.
        public static UInt32 servo1u = 2000;    //1673
        public static UInt32 servo1l = 900;    //1360
        public static UInt32 servo2u = 2000;
        public static UInt32 servo2l = 900;
        public static UInt32 servo3u = 2000;
        public static UInt32 servo3l = 900;
    }
}
