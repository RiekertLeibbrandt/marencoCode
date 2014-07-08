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

        public static byte[] throttleSetting = new byte[10] { 0, 22, 0,0,0,0,0,0,0,0};
        public static int x = 0;
        public static int y = 0;
        public static int z = 0;
        public static UInt32 halTime = 0;
        public static bool writeNow = false;
        public static Int16 zKeep = 0;
        public static byte throttleSettingOld = 0x3C;
    }
}
