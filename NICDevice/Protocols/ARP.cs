using NICDevice.Core;
using NICDevice.Interfaces;
using NICDevice.IP;
using NICDevice.Layers;
using NICDevice.MAC;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NICDevice.Protocols
{
    public class ARP : NIC, ILayer, IDisposable
    {
        private static readonly List<(ARPReply Reply, long Timestamp)> CachedARPReplies = new();
        private static readonly object CacheLock = new(); // Thread safety for cache

        private readonly AutoResetEvent receivedEvent = new(false);
        private bool disposed = false;

        public EthernetLayer EthernetLayer { get; }
        public short Operation { get; }
        public MacAddress SenderHardwareAddress { get; }
        public IPAddress SenderProtocolAddress { get; }
        public MacAddress TargetHardwareAddress { get; }
        public IPAddress TargetProtocolAddress { get; }
        public byte[] LayerBytes { get; set; }
        /// <summary>
        /// Constructs an ARP packet with specified target and sender addresses, and operation type.
        /// </summary>
        /// <param name="targetProtocolAddress">Specifies the IP address of the target device for the ARP request or reply.</param>
        /// <param name="operation">Indicates the type of ARP operation, such as request or reply.</param>
        /// <param name="targetHardwareAddress">Defines the MAC address of the target device, defaulting to a broadcast address if not provided.</param>
        /// <param name="SenderProtocolAddress_">Represents the IP address of the sender, defaulting to the system's protocol address if not specified.</param>
        public ARP(IPAddress targetProtocolAddress, short operation = 1, MacAddress? targetHardwareAddress = null, IPAddress SenderProtocolAddress_ = null,MacAddress SenderMacAddress=null)
        {
            if(SenderMacAddress is null)
                EthernetLayer = new EthernetLayer(targetHardwareAddress, SystemMacAddress, 0x806);
            else
                EthernetLayer = new EthernetLayer(targetHardwareAddress, SenderMacAddress, 0x806);



            Operation = operation;
            if (SenderMacAddress is null)
                     SenderHardwareAddress = SystemMacAddress;
            else
                     SenderHardwareAddress = SenderMacAddress;
            if (SenderProtocolAddress_ is null)
                SenderProtocolAddress = NIC.SystemProtocolAddress;
            else
                SenderProtocolAddress = SenderProtocolAddress_;
           
            TargetHardwareAddress = targetHardwareAddress ?? BroadCastMacAddress;
            TargetProtocolAddress = targetProtocolAddress;
        }
        /// <summary>
        /// Sends a packet if the cached ARP response is stale or if forced sending is enabled. It checks the cache for
        /// recent replies before sending.
        /// </summary>
        /// <param name="forceSend">Enables sending a packet regardless of the freshness of the cached ARP response.</param>
        public void Send(bool forceSend = false)
        {
            lock (CacheLock)
            {
                if (!forceSend && CachedARPReplies.Any(x =>
                        x.Reply.SenderProtocolAddress == TargetProtocolAddress &&
                        (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - x.Timestamp) < TimeSpan.FromMinutes(1).TotalMilliseconds))
                {
                    return; // Cached ARP response is still fresh, no need to send another request.
                }
            }

            SendPacket(EthernetLayer, this);
        }

        /// <summary>
        /// Listens for ARP replies and returns the first valid reply received within a specified timeout period.
        /// </summary>
        /// <param name="ForceAwait">Forces the method to wait for a reply instead of checking the cache for a recent response.</param>
        /// <returns>Returns an ARPReply object if a valid reply is received, otherwise returns null.</returns>
        public ARPReply? Listen(bool ForceAwait = false)
        {
            if (CaptureDevice is null)
            {
                Console.WriteLine("[ARP]Failed to listen arp is null");
                return default;
            }
            if (!ForceAwait)
            {
                lock (CacheLock)
                {
                    var cached = CachedARPReplies.FirstOrDefault(x =>
                        x.Reply.SenderProtocolAddress == TargetProtocolAddress &&
                        (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - x.Timestamp) < TimeSpan.FromMinutes(1).TotalMilliseconds);
                    if (cached != default)
                        return cached.Reply;
                }
            }

            ARPReply? result = null;
            var timeout = DateTime.UtcNow.AddSeconds(3); // Timeout in 3 seconds

            void PacketHandler(object sender, PacketCapture e)
            {
                var packet = e.GetPacket().GetPacket().Bytes;
                if (packet.Length < 42) return;

                if (packet[12] == 0x08 && packet[13] == 0x06 &&  // Ethernet type = ARP
                    packet[20] == 0x00 && packet[21] == 0x02 &&  // ARP Opcode = Reply
                    packet[28] == TargetProtocolAddress[0] &&
                    packet[29] == TargetProtocolAddress[1] &&
                    packet[30] == TargetProtocolAddress[2] &&
                    packet[31] == TargetProtocolAddress[3])
                {
                    result = new ARPReply(
                        new MacAddress(packet[22..28]),
                        new IPAddress(packet[28..32]),
                        new MacAddress(packet[32..38]),
                        new IPAddress(packet[38..42])
                    );

                    lock (CacheLock)
                    {
                        UpdateOrAddCache(result);
                    }

                    receivedEvent.Set(); // Signal that we received an ARP reply
                }
            }

            lock (CacheLock)
            {
                if (CaptureDevice is null)
                {
                    receivedEvent.Set();
                }
                else
                {
                    CaptureDevice!.OnPacketArrival += PacketHandler;
                    //CaptureDevice!.Filter = "arp and arp[6:2] = 2";
                    CaptureDevice!.StartCapture();
                }
            }

            receivedEvent.WaitOne(TimeSpan.FromSeconds(3)); // Wait for ARP reply

            lock (CacheLock)
            {
                if (CaptureDevice is not null)
                    CaptureDevice!.OnPacketArrival -= PacketHandler;
            }

            return result;
        }

        private void UpdateOrAddCache(ARPReply reply)
        {
            lock (CacheLock)
            {
                var existing = CachedARPReplies.FirstOrDefault(x => x.Reply.SenderProtocolAddress == reply.SenderProtocolAddress);
                if (existing != default)
                {
                    CachedARPReplies.Remove(existing);
                }
                CachedARPReplies.Add((reply, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            }
        }
        /// <summary>
        /// Cleans up resources and ensures proper disposal of the object. Prevents multiple disposals by checking the
        /// disposed state.
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;


            receivedEvent.Dispose(); // Ensure cleanup


        }
        /// <summary>
        /// Generates a byte array representing the payload for a network operation, including hardware and protocol
        /// details.
        /// </summary>
        /// <returns>Returns the constructed byte array containing the payload data.</returns>
        public byte[] Payload()
        {
            return LayerBytes ??= BitConverter.GetBytes((short)1).Reverse() // Hardware Type (Ethernet)
                .Concat(BitConverter.GetBytes((short)0x0800).Reverse()) // Protocol Type (IPv4)
                .Concat(new byte[] { 6, 4 }) // HW Address Length (6), Protocol Address Length (4)
                .Concat(BitConverter.GetBytes(Operation).Reverse()) // Operation (1 = Request, 2 = Reply)
                .Concat((byte[])SenderHardwareAddress)
                .Concat((byte[])SenderProtocolAddress)
                .Concat((byte[])TargetHardwareAddress)
                .Concat((byte[])TargetProtocolAddress)
                .ToArray();
        }


        public record ARPReply(MacAddress SenderHardwareAddress, IPAddress SenderProtocolAddress, MacAddress TargetHardwareAddress, IPAddress TargetProtocolAddress)
        {
            public override string ToString() =>
                $"Sender Hardware Address: {SenderHardwareAddress}\n" +
                $"Sender Protocol Address: {SenderProtocolAddress}\n" +
                $"Target Hardware Address: {TargetHardwareAddress}\n" +
                $"Target Protocol Address: {TargetProtocolAddress}";
        }
    }
}
