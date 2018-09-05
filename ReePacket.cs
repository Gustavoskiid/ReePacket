using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ReeCode
{
    public class ReePacket
    {
        int Version;
        ushort TotalLength;
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
        uint SequenceNumber;
        uint AckNumber;
        ushort PacketLength;

        // Useful stuff
        public byte[] Payload;
        public string PayloadText;

        public ReePacket(byte[] b)
        {
            // https://en.wikipedia.org/wiki/IPv4#Header
            // b[0]
            // b[0] = Version + IHL (Header Length)
            Version = b[0] >> 4; // IPv4 or IPv6
            int IHL = b[0] << 4;
            // b[1] = DSCP + ECN
            // b[2] + b[3] = Total Length
            TotalLength = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 2)));
            // b[4] + b[5] = Identification
            // b[6] + b[7] = Flags + Fragment Offset
            TTL = b[8];
            ProtocolNum = b[9];
            // b[10] + b[11] = Header Checksum
            SourceIP = b[12] + "." + b[13] + "." + b[14] + "." + b[15];
            DestIP = b[16] + "." + b[17] + "." + b[18] + "." + b[19];
            if (ProtocolNum == (int)ProtocolType.Icmp) // 1
            {
                // throw new NotImplementedException("ICMP has not been implemented yet :/");
            }
            else if (ProtocolNum == (int)ProtocolType.Tcp) // 6
            {
                // https://en.wikipedia.org/wiki/Transmission_Control_Protocol
                SourcePort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 20))); // b[20] + b[21]
                DestPort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 22))); // b[22] + b[23]
                SequenceNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(b, 24)); // b[24] + b[25] + b[26] + b[27]
                AckNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(b, 28)); // b[28] + b[29] + b[30] + b[31]

                // Data offset (4 bits) + Reserved (3 bits) + Flags (9 bits Control Bits)
                ushort DataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 32)); // b[32] + b[33]
                ushort WindowSize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 34)); // b[34] + b[35]
                short Checksum = (short)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 336)); // b[36] + b[37]
                ushort UrgentPointer = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 38)); // b[38] +b[39]

                // The length of the data section is not specified in the TCP segment header.
                // It can be calculated by subtracting the combined length of the TCP header and the encapsulating IP header from the total IP datagram length (specified in the IP header).
                // Console.WriteLine("Total: " + TotalLength);
                PacketLength = (ushort)(Convert.ToInt32(TotalLength) - 20); // TCP Header is  20 long (20 -> 40)
                // (Variable 0–320 bits, divisible by 32) -- o_O

                Payload = new byte[PacketLength];
                for (int j = 0; j < PacketLength; j++)
                {
                    Payload[j] = b[40 + j]; // First 40 bytes are headers (20 IP + 20 TCP)
                }
                PayloadText = string.Join(" ", Payload);
            }

            else if (ProtocolNum == (int)ProtocolType.Udp) // 17
            {
                // https://en.wikipedia.org/wiki/User_Datagram_Protocol
                SourcePort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 20))); // b[20] + b[21]
                DestPort = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 22))); // b[22] + b[23]
                PacketLength = ((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 24))); // b[24] + b[25]
                short Checksum = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b, 26)); // b[26] + b[27]

                PacketLength -= 8; // PacketLength is header AND Data - We only want data
                // Custom
                Payload = new byte[PacketLength]; // The first 8 bytes are just data
                for (int j = 0; j < PacketLength; j++)
                {
                    Payload[j] = b[28 + j]; // First 28 bytes are headers (20 IP + 8 UDP)
                }

                PayloadText = string.Join(" ", Payload);
            }
        }

        public static string PayloadToText(byte[] payload)
        {
            string text = Encoding.UTF8.GetString(payload);
            return text;

        }
    }
}
