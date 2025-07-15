using NICDevice.Core;
using NICDevice.Interfaces;
using NICDevice.IP;
using NICDevice.Layers;
using NICDevice.MAC;
using System;
using System.Linq;

namespace NICDevice.Protocols
{
    public class UDP : NIC, ILayer
    {
        public EthernetLayer EthernetLayer { get; }
        public ushort SourcePort { get; }
        public ushort DestinationPort { get; }
        public IPAddress SourceIP { get; }
        public IPAddress DestinationIP { get; }
        public byte[] Data { get; }
        [Obsolete("Not used/set")]
        public byte[] LayerBytes { get ; set ; }

        public UDP(MacAddress DesMac,IPAddress destinationIP, ushort destinationPort, byte[] data, ushort sourcePort = 68)
        {
            EthernetLayer = new EthernetLayer(DesMac, SystemMacAddress, 0x800); // IPv4
            SourcePort = sourcePort;
            DestinationPort = destinationPort;
            SourceIP = SystemProtocolAddress;
            DestinationIP = destinationIP;
            Data = data;
        }

        /// <summary>
        /// Constructs the raw UDP packet
        /// </summary>
        public byte[] CreateIPv4Header(int udpPayloadLength)
        {
            byte[] ipHeader = new byte[20];

            ipHeader[0] = 0x45; // Version (4) + IHL (5 -> 20-byte header)
            ipHeader[1] = 0x00; // DSCP + ECN
            Array.Copy(BitConverter.GetBytes((short)(20 + 8 + udpPayloadLength)).Reverse().ToArray(), 0, ipHeader, 2, 2); // Total Length
            ipHeader[4] = 0x00; ipHeader[5] = 0x00; // Identification
            ipHeader[6] = 0x40; ipHeader[7] = 0x00; // Flags + Fragment Offset
            ipHeader[8] = 0x40; // TTL (64)
            ipHeader[9] = 0x11; // Protocol (UDP = 17)
            Array.Copy(SourceIP, 0, ipHeader, 12, 4); // Source IP
            Array.Copy(DestinationIP, 0, ipHeader, 16, 4); // Destination IP

            return ipHeader;
        }

        public byte[] Payload()
        {

            byte[] udpHeader = CreateUDPHeader();
            byte[] ipHeader = CreateIPv4Header(Data.Length);
            return ipHeader.Concat(udpHeader).Concat(Data).ToArray();
        }


        /// <summary>
        /// Sends the UDP packet using NIC
        /// </summary>
        public void Send()
        {
           
            SendPacket(EthernetLayer, this);
        }

        private byte[] CreateUDPHeader()
        {
            byte[] udpHeader = new byte[8];

            ushort udpLength = (ushort)(8 + Data.Length);
            Array.Copy(BitConverter.GetBytes(SourcePort).Reverse().ToArray(), 0, udpHeader, 0, 2);
            Array.Copy(BitConverter.GetBytes(DestinationPort).Reverse().ToArray(), 0, udpHeader, 2, 2);
            Array.Copy(BitConverter.GetBytes(udpLength).Reverse().ToArray(), 0, udpHeader, 4, 2);

            // Create pseudo-header
            byte[] pseudoHeader = new byte[12];
            Array.Copy(SourceIP, 0, pseudoHeader, 0, 4);
            Array.Copy(DestinationIP, 0, pseudoHeader, 4, 4);
            pseudoHeader[8] = 0; // zero
            pseudoHeader[9] = 17; // UDP protocol
            Array.Copy(BitConverter.GetBytes(udpLength).Reverse().ToArray(), 0, pseudoHeader, 10, 2);

            // Build entire segment for checksum: pseudo-header + udp header + data
            byte[] segment = pseudoHeader
                .Concat(udpHeader.Take(6)) // checksum field is zeroed for calculation
                .Concat(new byte[] { 0, 0 }) // temporary checksum
                .Concat(Data).ToArray();

            if (segment.Length % 2 != 0)
                segment = segment.Concat(new byte[] { 0 }).ToArray(); // pad if odd length

            ushort checksum = CalculateChecksum(segment);
            Array.Copy(BitConverter.GetBytes(checksum).Reverse().ToArray(), 0, udpHeader, 6, 2); // Insert checksum

            return udpHeader;
        }

        private ushort CalculateChecksum(byte[] buffer)
        {
            uint sum = 0;
            for (int i = 0; i < buffer.Length; i += 2)
            {
                ushort word = (ushort)((buffer[i] << 8) + (i + 1 < buffer.Length ? buffer[i + 1] : 0));
                sum += word;

                // Fold 32-bit sum to 16 bits
                if ((sum & 0xFFFF0000) != 0)
                {
                    sum = (sum & 0xFFFF) + (sum >> 16);
                }
            }

            while ((sum >> 16) != 0)
                sum = (sum & 0xFFFF) + (sum >> 16);

            return (ushort)~sum;
        }




    }
}
