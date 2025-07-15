using NICDevice.Core;
using NICDevice.Interfaces;
using NICDevice.IP;
using System;
using System.Linq;

namespace NICDevice.Layers
{
    public sealed class IPV4Layer : NIC, ILayer
    {
        private readonly IPAddress SourceAddress;
        private readonly IPAddress DestinationAddress;
        private readonly byte Protocol;
        private readonly byte[] PayloadBytes;
        public byte[] LayerBytes { get; set; }
        public byte TTL { get; set; }
        public IPV4Layer(IPAddress sourceAddress, IPAddress destinationAddress, byte protocol,byte TTL, byte[] payloadBytes)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
            Protocol = protocol;
            PayloadBytes = payloadBytes;
            this.TTL = TTL;
        }

        public byte[] Payload()
        {
            int headerLength = 20; // IPv4 header without options
            int totalLength = headerLength + PayloadBytes.Length; // Total length of IP packet

            byte[] header = new byte[headerLength];

            header[0] = 0x45; // Version (IPv4) + Header Length (5 * 4 = 20 bytes)
            header[1] = 0x00; // Type of Service
            header[2] = (byte)(totalLength >> 8);
            header[3] = (byte)(totalLength & 0xFF); // Total Length
            header[4] = 0x00; header[5] = 0x00; // Identification
            header[6] = 0x40; header[7] = 0x00; // Flags/Fragment Offset
            header[8] = TTL; // TTL
            header[9] = Protocol; // Protocol
            header[10] = 0x00; header[11] = 0x00; // Checksum (to be calculated)

            // Insert Source & Destination IP Addresses
            byte[] srcIP = (byte[])SourceAddress;
            byte[] destIP = DestinationAddress;
            Array.Copy(srcIP, 0, header, 12, 4);
            Array.Copy(destIP, 0, header, 16, 4);

            // Calculate and insert checksum
            ushort checksum = CalculateChecksum(header);
            header[10] = (byte)(checksum >> 8);
            header[11] = (byte)(checksum & 0xFF);

            // Return final packet (Header + Payload)
            return header.Concat(PayloadBytes).ToArray();
        }

        public static ushort CalculateChecksum(byte[] header)
        {
            uint sum = 0;

            // Sum every 16-bit word in the header
            for (int i = 0; i < header.Length; i += 2)
            {
                ushort word = (ushort)((header[i] << 8) + (i + 1 < header.Length ? header[i + 1] : 0));
                sum += word;
            }

            // Fold 32-bit sum into 16 bits
            while ((sum >> 16) > 0)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            return (ushort)~sum; // One's complement
        }
    }
}
