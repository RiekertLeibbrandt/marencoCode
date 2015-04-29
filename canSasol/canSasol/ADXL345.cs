// James modifies this. I have to hack some CAN functions in here, as we are all using the same spi bus.

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace Marenco.Sensors
{
    public class ADXL345
    {
        //ADXL345 Register Addresses
        const byte DEVID = 0x00;	//Device ID Register
        const byte THRESH_TAP = 0x1D;	//Tap Threshold
        const byte OFSX = 0x1E;	//X-axis offset
        const byte OFSY = 0x1F;	//Y-axis offset
        const byte OFSZ = 0x20;	//Z-axis offset
        const byte DURATION = 0x21;	//Tap Duration
        const byte LATENT = 0x22;	//Tap latency
        const byte WINDOW = 0x23;	//Tap window
        const byte THRESH_ACT = 0x24;	//Activity Threshold
        const byte THRESH_INACT = 0x25;	//Inactivity Threshold
        const byte TIME_INACT = 0x26;	//Inactivity Time
        const byte ACT_INACT_CTL = 0x27;	//Axis enable control for activity and inactivity detection
        const byte THRESH_FF = 0x28;	//free-fall threshold
        const byte TIME_FF = 0x29;	//Free-Fall Time
        const byte TAP_AXES = 0x2A;	//Axis control for tap/double tap
        const byte ACT_TAP_STATUS = 0x2B;	//Source of tap/double tap
        const byte BW_RATE = 0x2C;	//Data rate and power mode control
        const byte POWER_CTL = 0x2D;	//Power Control Register
        const byte INT_ENABLE = 0x2E;	//Interrupt Enable Control
        const byte INT_MAP = 0x2F;	//Interrupt Mapping Control
        const byte INT_SOURCE = 0x30;	//Source of interrupts
        const byte DATA_FORMAT = 0x31;	//Data format control
        const byte DATAX0 = 0x32;	//X-Axis Data 0
        const byte DATAX1 = 0x33;	//X-Axis Data 1
        const byte DATAY0 = 0x34;	//Y-Axis Data 0
        const byte DATAY1 = 0x35;	//Y-Axis Data 1
        const byte DATAZ0 = 0x36;	//Z-Axis Data 0
        const byte DATAZ1 = 0x37;	//Z-Axis Data 1
        const byte FIFO_CTL = 0x38;	//FIFO control
        const byte FIFO_STATUS = 0x39;	//FIFO status

        SPI.Configuration spiConfig;
        SPI spiBus;
        byte xOffset, yOffset, zOffset;
        byte[] valueLocations;
        byte[] values;

        public ADXL345(Cpu.Pin pinCS, uint Freq)
        {
            spiConfig = new SPI.Configuration(
                pinCS,
                false,             // SS-pin active state
                0,                 // The setup time for the SS port 10
                0,                 // The hold time for the SS port 10
                true,              // The idle state of the clock
                true,             // The sampling clock edge
                Freq,              // The SPI clock rate in KHz
                SPI_Devices.SPI1   // The used SPI bus (refers to a MOSI MISO and SCLK pinset)
            );

            spiBus = new SPI(spiConfig);
            spiBus.Write(new byte[] { DATA_FORMAT, 0x0a });   //This sets to 8g, still 4mg/bit.
            spiBus.Write(new byte[] { POWER_CTL, 0x08 });

            setOffsets(0, 0, 0);

            valueLocations = new byte[6] { DATAX0 | 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00 };
            values = new byte[6];
        }

        public void setOffsets(byte x, byte y, byte z)
        {
            xOffset = x;
            yOffset = y;
            zOffset = z;

            spiBus.Write(new byte[] { OFSX, xOffset });        // Unique for each device.  Need to come up with a calibration scheme
            spiBus.Write(new byte[] { OFSY, yOffset });
            spiBus.Write(new byte[] { OFSZ, zOffset });
        }

        // Performs a quick setup, for the second accel if using more than 1 on a single bus.
        public void quickSetup()
        {
            spiBus.Write(new byte[] { DATA_FORMAT, 0x0a });   //This sets to 8g, still 4mg/bit., a is 8 g.
            spiBus.Write(new byte[] { POWER_CTL, 0x08 });
            setOffsets(0, 0, 0);
        }

        public void getValues(ref int x, ref int y, ref int z)
        {
            spiBus.WriteRead(valueLocations, values, 1);

            x = (short)(((ushort)values[1] << 8) | (ushort)values[0]);
            y = (short)(((ushort)values[3] << 8) | (ushort)values[2]);
            z = (short)(((ushort)values[5] << 8) | (ushort)values[4]);
        }

        public void setUpAccelRate(int input)
        {
            // First we set it to 100Hz, normal operation mode.
            byte rateData = 0x00;
            if (input == 100)
            {
                rateData = 0x0A;
            }
            if (input == 200)
            {
                rateData = 0x0B;
            }
            Thread.Sleep(100);
            spiBus.Write(new byte[] { BW_RATE, rateData });
            Thread.Sleep(100);
        }

        public void setUpInterrupt()
        {
            // This command sets int1 to fire when data are available. Only bit7 is set. First disable all ints, then set it up.
            spiBus.Write(new byte[] { INT_ENABLE, 0x00 });
            Thread.Sleep(100);
            // Now map the interrupt to int1 pin (0x00 sends all interrupts to int1 pin). 
            spiBus.Write(new byte[] { INT_MAP, 0x00 });
            Thread.Sleep(100);
            // Now set the interrupt.
            spiBus.Write(new byte[] { INT_ENABLE, 0x80 });
            Thread.Sleep(100);

        }

        public void clearInterrupt()
        {
            spiBus.WriteRead(valueLocations, values, 1);
        }

        //
        //  Now we paste CAN related stuff in here too.
        //
        //-------------------------------------
        #region REGISTERS
        //-------------------------------------
        private const byte RXF0SIDH = 0x00;
        private const byte RXF0SIDL = 0x01;
        private const byte RXF0EID8 = 0x02;
        private const byte RXF0EID0 = 0x03;
        private const byte RXF1SIDH = 0x04;
        private const byte RXF1SIDL = 0x05;
        private const byte RXF1EID8 = 0x06;
        private const byte RXF1EID0 = 0x07;
        private const byte RXF2SIDH = 0x08;
        private const byte RXF2SIDL = 0x09;
        private const byte RXF2EID8 = 0x0A;
        private const byte RXF2EID0 = 0x0B;
        private const byte BFPCTRL = 0x0C;
        private const byte TXRTSCTRL = 0x0D;
        private const byte CANSTAT = 0x0E;
        private const byte CANCTRL = 0x0F;
        private const byte RXF3SIDH = 0x10;
        private const byte RXF3SIDL = 0x11;
        private const byte RXF3EID8 = 0x12;
        private const byte RXF3EID0 = 0x13;
        private const byte RXF4SIDH = 0x14;
        private const byte RXF4SIDL = 0x15;
        private const byte RXF4EID8 = 0x16;
        private const byte RXF4EID0 = 0x17;
        private const byte RXF5SIDH = 0x18;
        private const byte RXF5SIDL = 0x19;
        private const byte RXF5EID8 = 0x1A;
        private const byte RXF5EID0 = 0x1B;
        private const byte TEC = 0x1C;
        private const byte REC = 0x1D;
        private const byte RXM0SIDH = 0x20;
        private const byte RXM0SIDL = 0x21;
        private const byte RXM0EID8 = 0x22;
        private const byte RXM0EID0 = 0x23;
        private const byte RXM1SIDH = 0x24;
        private const byte RXM1SIDL = 0x25;
        private const byte RXM1EID8 = 0x26;
        private const byte RXM1EID0 = 0x27;
        private const byte CNF3 = 0x28;
        private const byte CNF2 = 0x29;
        private const byte CNF1 = 0x2A;
        private const byte CANINTE = 0x2B;
        private const byte MERRE = 7;
        private const byte WAKIE = 6;
        private const byte ERRIE = 5;
        private const byte TX2IE = 4;
        private const byte TX1IE = 3;
        private const byte TX0IE = 2;
        private const byte RX1IE = 1;
        private const byte RX0IE = 0;
        private const byte CANINTF = 0x2C;
        private const byte MERRF = 7;
        private const byte WAKIF = 6;
        private const byte ERRIF = 5;
        private const byte TX2IF = 4;
        private const byte TX1IF = 3;
        private const byte TX0IF = 2;
        private const byte TX0IF_MASK = 0x04;
        private const byte RX1IF = 1;
        private const byte RX0IF = 0;
        private const byte RX1IF_MASK = 0x02;
        private const byte RX0IF_MASK = 0x01;
        private const byte EFLG = 0x2D;
        private const byte TXB0CTRL = 0x30;
        private const byte TXREQ = 3;
        private const byte TXB0SIDH = 0x31;
        private const byte TXB0SIDL = 0x32;
        private const byte EXIDE = 3;
        private const byte EXIDE_MASK = 0x08;
        private const byte TXB0EID8 = 0x33;
        private const byte TXB0EID0 = 0x34;
        private const byte TXB0DLC = 0x35;
        private const byte TXRTR = 7;
        private const byte TXB0D0 = 0x36;
        private const byte RXB0CTRL = 0x60;
        private const byte RXM1 = 6;
        private const byte RXM0 = 5;
        private const byte RXRTR = 3;
        // Bits 2:0 FILHIT2:0
        private const byte RXB0SIDH = 0x61;
        private const byte RXB0SIDL = 0x62;
        private const byte RXB0EID8 = 0x63;
        private const byte RXB0EID0 = 0x64;
        private const byte RXB0DLC = 0x65;
        private const byte RXB0D0 = 0x66;
        //-------------------------------------
        #endregion REGISTERS
        //-------------------------------------

        //MCP2515 Command Bytes
        private readonly byte RESET = 0xC0;
        private readonly byte READ = 0x03;
        private readonly byte READ_RX_BUFFER = 0x90;
        private readonly byte WRITE = 0x02;
        private readonly byte LOAD_TX_BUFFER = 0x40;
        private readonly byte RTS = 0x80;
        private readonly byte READ_STATUS = 0xA0;
        private readonly byte RX_STATUS = 0xB0;
        private readonly byte BIT_MODIFY = 0x05;

        /// <summary>Represent a LOW  (false) state.</summary>
        private const bool LOW = false;
        /// <summary>Represent a HIGH (true) state.</summary>
        private const bool HIGH = true;
        /// <summary></summary>
        private const byte DataIndexOffset = 2;
        /// <summary>The max CAN ID that can be represented on an 11 bit ID.</summary>
        private const int CANID_11BITS = 0x7FF;

        //
        // Now relevant CAN functions.
        //
        public enum enBaudRate
        {
            CAN_BAUD_10K = 1, CAN_BAUD_50K = 2, CAN_BAUD_100K = 3,
            CAN_BAUD_125K = 4, CAN_BAUD_250K = 5, CAN_BAUD_500K = 6,
            CAN_BAUD_1000K = 7
        }

        public class CANMSG
        {
            public uint CANID { get; set; }
            public bool IsExtended { get { return CANID > CANID_11BITS; } }
            public bool IsRemote { get; set; }
            public int DataLength { get { return data.Length; } }
            public byte[] data = new byte[8];
        }

        public void canReset()
        {
            spiBus.Write(new byte[] { RESET });
        }

        public bool InitCAN(enBaudRate baudrate, UInt16 filter, UInt16 mask)
        {
            // Configure SPI 
            //var configSPI = new SPI.Configuration(Pins.GPIO_PIN_A0, LOW, 0, 0, HIGH, HIGH, 10000, SPI.SPI_module.SPI1);  //a0 D10

            //spi = new SPI(configSPI);

            // Write reset to the CAN transceiver.
            //spiBus.Write(new byte[] { RESET });
            //Read mode and make sure it is config
            //Thread.Sleep(100);
            // BECKER WHAT IS HAPPENING HERE.
            //            byte mode = (byte)(ReadRegister(CANSTAT));  // >> 5);
            //            if (mode != 0x04)
            //            {
            //                return false;
            //            }
            //            else

               // SetMask(mask, filter);
           //SetCANBaud(baudrate);
           return true;
            
        }


        public bool SetCANBaud(enBaudRate baudrate)
        {
            byte brp;

            //BRP<5:0> = 00h, so divisor (0+1)*2 for 125ns per quantum at 16MHz for 500K   
            //SJW<1:0> = 00h, Sync jump width = 1
            switch (baudrate)
            {
                case enBaudRate.CAN_BAUD_10K:
                    brp = 5;
                    break;
                case enBaudRate.CAN_BAUD_50K:
                    brp = 4;
                    break;
                case enBaudRate.CAN_BAUD_100K:
                    brp = 3;
                    break;
                case enBaudRate.CAN_BAUD_125K:
                    brp = 2;
                    break;
                case enBaudRate.CAN_BAUD_250K:
                    brp = 1;
                    break;
                case enBaudRate.CAN_BAUD_500K:
                    brp = 0;
                    break;
                default:
                    return false;
                    break;
            }

            byte[] cmdBuffer = new byte[] { WRITE, CNF1, (byte)(brp & 0x3F) };
            spiBus.Write(cmdBuffer);

            //PRSEG<2:0> = 0x01, 2 time quantum for prop
            //PHSEG<2:0> = 0x06, 7 time constants to PS1 sample
            //SAM = 0, just 1 sampling
            //BTLMODE = 1, PS2 determined by CNF3
            // James comments this out.
            //spiBus.Write(new byte[] { WRITE, CNF2, 0xB1 });

            //PHSEG2<2:0> = 5 for 6 time constants after sample
            // James comments this out
            //spiBus.Write(new byte[] { WRITE, CNF3, 0x05 });


            // below is for 1mbit, James uncomments it all.
            //spiBus.Write(new byte[] { WRITE, CNF1, 0x00 }); 
            spiBus.Write(new byte[] { WRITE, CNF2, 0x90 }); 
            spiBus.Write(new byte[] { WRITE, CNF3, 0x02 });

            //SyncSeg + PropSeg + PS1 + PS2 = 1 + 2 + 7 + 6 = 16
            return true;
        }

        public void baud1()
        {
            byte brp = 0;
            byte[] cmdBuffer = new byte[] { WRITE, CNF1, (byte)(brp & 0x3F) };
            spiBus.Write(cmdBuffer);
        }

        public void baud2()
        {
            spiBus.Write(new byte[] { WRITE, CNF2, 0x90 });
        }

        public void baud3()
        {
            spiBus.Write(new byte[] { WRITE, CNF3, 0x02 });
        }

        public void WriteRegister(byte registerAddress, byte value)
        {
            spiBus.Write(new byte[] { WRITE, registerAddress, value });
        }

        /// <summary>Reads the value of the selected register.</summary>
        /// <param name="registerAddress">The address of the register.</param>
        /// <returns>A byte with the value read from the register.</returns>
        private byte ReadRegister(byte registerAddress)
        {
            byte[] CmdBuffer = new byte[] { READ, registerAddress };
            byte[] outData = new byte[1];
            spiBus.WriteRead(CmdBuffer, outData, 2);
            return outData[0];
        }

        public void WriteRegisterBit(byte registerAddress, byte bitNumber, byte value)
        {
            //spi.Write(new byte[] { BIT_MODIFY, regno, (byte)(1 << bitno) });
            if (value != (byte)0)
            {
                spiBus.Write(new byte[] { BIT_MODIFY, registerAddress, (byte)(1 << bitNumber), 0xFF });
            }
            else
            {
                spiBus.Write(new byte[] { BIT_MODIFY, registerAddress, (byte)(1 << bitNumber), 0x00 });
            }
        }

        // Have to hack the transmit function to work with chip select too.
        public void tId1(CANMSG message)
        {
            byte val = 0x00;
            uint bit11Address = 0x00;

            bit11Address = (uint)(message.CANID);
            val = (byte)(bit11Address >> 3);
            WriteRegister(TXB0SIDH, val);
            //val = (byte)(bit11Address << 5);
            //WriteRegister(TXB0SIDL, val);
        }

        public void tId2(CANMSG message)
        {
            byte val = 0x00;
            uint bit11Address = 0x00;

            bit11Address = (uint)(message.CANID);
            //val = (byte)(bit11Address >> 3);
            //WriteRegister(TXB0SIDH, val);
            val = (byte)(bit11Address << 5);
            WriteRegister(TXB0SIDL, val);
        }

        public void tRemote()
        {
            WriteRegister(TXB0DLC, 8);
        }

        public void tSend(CANMSG message)
        {
            byte[] txDATA = new byte[10];
            txDATA[0] = WRITE;
            txDATA[1] = TXB0D0;
            for (int i = DataIndexOffset; i < message.DataLength + DataIndexOffset; i++)
            {
                txDATA[i] = message.data[i - DataIndexOffset];
            }
            spiBus.Write(txDATA);
        }

        public void tTransmit()
        {
            WriteRegisterBit(TXB0CTRL, TXREQ, 1);
        }

        public bool Transmit(CANMSG message, int timeout)
        {
            // Holds if message was sent or not.
            bool sentMessage = false;

            // Calculate the end time based on current device time.
            TimeSpan startTime = Utility.GetMachineTime();
            TimeSpan endTime = startTime.Add(new TimeSpan(0, 0, 0, 0, timeout));

            //--------------------------------------
            // Set the CAN ID.
            byte val = 0x00;
            uint bit11Address = 0x00;
            uint bit18Address = 0x00;

            // Build Extended CAN ID 
            if (message.IsExtended)
            {
                // Split the 11 bit and 18 bit sections of the ID.
                bit11Address = (uint)((message.CANID >> 16) & 0xFFFF);
                bit18Address = (uint)(message.CANID & 0xFFFFF);
                // Set the first part of the ID
                val = (byte)(bit11Address >> 5);
                WriteRegister(TXB0SIDH, val);
                // Set the second part of the ID.
                val = (byte)(bit11Address << 3);
                val |= (byte)(bit11Address & 0x07);
                // Mark the message as extended.
                val |= 1 << EXIDE;
                WriteRegister(TXB0SIDL, val);
                // Write the 18 bit part of the ID
                val = (byte)(bit18Address >> 8);
                WriteRegister(TXB0EID8, val);
                val = (byte)(bit18Address);
                WriteRegister(TXB0EID0, val);
            }
            else
            {
                // Transmit a 11 bit ID.
                bit11Address = (uint)(message.CANID);
                val = (byte)(bit11Address >> 3);
                WriteRegister(TXB0SIDH, val);
                val = (byte)(bit11Address << 5);
                WriteRegister(TXB0SIDL, val);
            }


            //--------------------------------------
            val = (byte)(message.DataLength & 0x0f);
            // Check if is a remote frame
            if (message.IsRemote)
            {
                // Mark the frame as remote
                val |= (byte)(1UL << (TXRTR));
                WriteRegisterBit(val, TXRTR, 1);
            }
            WriteRegister(TXB0DLC, val);

            //--------------------------------------
            //Write Message Data
            byte[] txDATA = new byte[10];
            txDATA[0] = WRITE;
            txDATA[1] = TXB0D0;
            for (int i = DataIndexOffset; i < message.DataLength + DataIndexOffset; i++)
            {
                txDATA[i] = message.data[i - DataIndexOffset];
            }
            spiBus.Write(txDATA);

            //----------------------
            // Command to transmit the CAN message
            WriteRegisterBit(TXB0CTRL, TXREQ, 1);

            //----------------------
            // Wait untile time out to get confirmation of message was sent.
            while (Utility.GetMachineTime() < endTime)
            {
                val = ReadRegister(CANINTF);
                if ((val & TX0IF_MASK) == TX0IF_MASK)
                {
                    sentMessage = true;
                    break;
                }
            }

            ////Abort the send if failed
            WriteRegisterBit(TXB0CTRL, TXREQ, 0);

            ////And clear write interrupt
            WriteRegisterBit(CANINTF, TX0IF, 0);

            return sentMessage;
        }


        public void SetCANNormalMode()
        {
            //REQOP2<2:0> = 000 for normal mode
            //ABAT = 0, do not abort pending transmission
            //OSM = 0, not one shot
            //CLKEN = 1, disable output clock
            //CLKPRE = 0b11, clk/8
            WriteRegister(CANCTRL, 0x07);
            //Read mode and make sure it is normal
            byte mode = ReadRegister(CANSTAT);
            mode = (byte)(mode >> 5);
            if (mode != 0)
            { }

            // Set RX buffer control to turn filters OFF and receive any message.
            // Of course, this is the issue.
            //            WriteRegister(RXB0CTRL, 0x60);


        }

    }
}
