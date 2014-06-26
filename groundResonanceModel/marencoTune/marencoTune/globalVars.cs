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
        public static byte[] throttleSetting = new byte[1] { 0 };
        public static byte throttleSettingOld = 0x12;
        public static int x = 0;
        public static int y = 0;
        public static int z = 0;
        public static UInt32 halTime = 0;

    }
}
