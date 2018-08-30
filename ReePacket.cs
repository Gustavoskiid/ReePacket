using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ReeCode
{
    public class ReePacket
    {
        int TTL;
        int ProtocolNum = -1;
        public ProtocolType ProtocolType
        {
            get
            {
                // To fix
                if (ProtocolNum == 1)
                {
                    return ProtocolType.Icmp;
                }
                else if (ProtocolNum == 6)
                {
                    return ProtocolType.Tcp;
                }
                else if (ProtocolNum == 17)
                {
                    return ProtocolType.Udp;
                }
                return ProtocolType.Unknown;
            }
        }
        public string SourceIP;
        public string DestIP;

        // The protocol determines the rest
        public ushort SourcePort;
        public ushort DestPort;
        string SequenceNumber = "";
        string AckNumber = "";
        public ushort PacketLength;

        // Useful stuff
        public byte[] DataBytes;
        public string DataBytesText;

        public ReePacket(byte[] b)
        {
            // b[0] = Version + IHL (Header Length)
            // b[1] = DSCP + ECN
            // b[2] + b[3] = Total Length
            // b[4] + b[5] = Identification
            // b[6] + b[7] = Flags + Fragment Offset
            TTL = b[8];
            ProtocolNum = b[9];
            // b[10] + b[11] = Header Checksum
            SourceIP = b[12] + "." + b[13] + "." + b[14] + "." + b[15];
            DestIP = b[16] + "." + b[17] + "." + b[18] + "." + b[19];
            if (ProtocolNum == (int)ProtocolType.Icmp) // 1
            {
                throw new NotImplementedException("ICMP has not been implemented yet :/");
            }
            else if (ProtocolNum == (int)ProtocolType.Tcp) // 6
            {
                // https://en.wikipedia.org/wiki/Transmission_Control_Protocol
                SourcePort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 20))); // b[20] + b[21]
                DestPort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 22))); // b[22] + b[23]
                SequenceNumber = Convert.ToString(BitConverter.ToInt32(b, 24)); // b[24] + b[25] + b[26] + b[27]
                AckNumber = Convert.ToString(BitConverter.ToInt32(b, 28)); // b[28] + b[29] + b[30] + b[31]
                // Data offset + Reserved + Flags (Control Bits) // b[32] + b[33]
                // Window Size // b[34] + b[35]
                // Checksum // b[36] + b[37]
                // Urgent Pointer // b[38] +b[39]
                // (Variable 0–320 bits, divisible by 32) -- o_O

                // To Fix
                PacketLength = 256;

                DataBytes = new byte[PacketLength];
                for (int j = 0; j < PacketLength; j++)
                {
                    DataBytes[j] = b[56 + j]; // 56 since the previous data is headers
                }
                DataBytesText = string.Join(" ", DataBytes);
            }

            else if (ProtocolNum == (int)ProtocolType.Udp) // 17
            {
                // https://en.wikipedia.org/wiki/User_Datagram_Protocol
                SourcePort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 20))); // b[20] + b[21]
                DestPort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 22))); // b[22] + b[23]
                PacketLength = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 2))); // b[24] + b[25]

                // Custom
                DataBytes = new byte[PacketLength];
                for (int j = 0; j < PacketLength; j++)
                {
                    DataBytes[j] = b[26 + j]; // 26 since the previous data is headers
                }

                DataBytesText = string.Join(" ", DataBytes);
            }
        }

        public static string BytesToText(string bytes)
        {
            bytes = bytes.Trim();
            byte[] myList = bytes.Split(' ').Select(byte.Parse).ToArray();
            string text = Encoding.UTF8.GetString(myList);
            return text;
        }
    }
}
