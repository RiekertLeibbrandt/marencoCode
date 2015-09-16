using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace calibrateLoadCell
{
    public class Program
    {

        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
        public static Socket sockOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//        public static Socket sockIn = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.243"), 49001);  // 244,231,243
//        public static IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Parse("192.168.60.238"), 49002);
        public static AnalogInput loadCell = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);

        public static void Main()
        {

            Debug.Print(NI.IPAddress.ToString());

          

        }

        static Timer stopRotor = new Timer(delegate
        {
            UInt16 strain = (UInt16) loadCell.ReadRaw();
            byte[] strainOut = new byte[2] {
                (byte) (strain & 0xff),
                (byte) ((strain >> 8) & 0xff)};

            sockOut.SendTo(strainOut, sendingEndPoint);
       },
    null,
    100,
    500);

    }
}
