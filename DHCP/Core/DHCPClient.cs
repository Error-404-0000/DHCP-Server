using NICDevice.IP;
using NICDevice.MAC;
using NICDevice.Protocols;

public class DHCPClient
{
    public void SendDhcpRequest(MacAddress clientMac, IPAddress requestedIP, IPAddress serverIP)
    {
        byte[] packet = ConstructDhcpRequestPacket(clientMac, requestedIP, serverIP);

        // Send DHCP Request via UDP (using your custom UDP class)
        UDP udpPacket = new UDP(clientMac, serverIP, 67, packet, 68);
        udpPacket.Send();

        Console.WriteLine($"Sent DHCP Request for {requestedIP} to DHCP Server {serverIP}");
    }

    private byte[] ConstructDhcpRequestPacket(MacAddress clientMac, IPAddress requestedIP, IPAddress serverIP)
    {
        byte[] packet = new byte[240]; // Standard BOOTP + DHCP base structure

        // BOOTP Header (First 236 Bytes)
        packet[0] = 1; // op: 1 = Boot Request
        packet[1] = 1; // htype: 1 = Ethernet
        packet[2] = 6; // hlen: Hardware Address Length (6 for MAC)
        packet[3] = 0; // hops
        Array.Copy(BitConverter.GetBytes(Environment.TickCount), 0, packet, 4, 4); // xid: Transaction ID
        packet[8] = 0; // secs
        packet[9] = 0;
        packet[10] = 0x80;  // Flags (broadcast request)
        packet[11] = 0x00;

        // Client IP Address (ciaddr) → 0.0.0.0 since it's a request
        Array.Copy(new byte[] { 0, 0, 0, 0 }, 0, packet, 12, 4);

        // 'Your' (offered) IP Address (yiaddr) → 0.0.0.0
        Array.Copy(new byte[] { 0, 0, 0, 0 }, 0, packet, 16, 4);

        // Next Server IP Address (siaddr) → 0.0.0.0
        Array.Copy(new byte[] { 0, 0, 0, 0 }, 0, packet, 20, 4);

        // Relay Agent IP Address (giaddr) → 0.0.0.0
        Array.Copy(new byte[] { 0, 0, 0, 0 }, 0, packet, 24, 4);

        // Client MAC Address
        Array.Copy((byte[])clientMac, 0, packet, 28, 6);

        // Fill remaining MAC address padding bytes with zeroes
        Array.Copy(new byte[10], 0, packet, 34, 10);

        // DHCP Magic Cookie (MUST be present, otherwise Wireshark marks it as malformed)
        packet[236] = 0x63; // Magic Cookie
        packet[237] = 0x82;
        packet[238] = 0x53;
        packet[239] = 0x63;

        // DHCP Options
        List<byte> options = new List<byte>
    {
        53, 1, 3, // DHCP Message Type: 3 = Request
       // 50, 4, requestedIP[0], requestedIP[1], requestedIP[2], requestedIP[3], // Requested IP Address
       50,4,0,0,0,0,
        54, 4, serverIP[0], serverIP[1], serverIP[2], serverIP[3], // Server Identifier
        55, 2, 1, 3, // Parameter Request List (Subnet Mask, Router)
        255 // End Option
    };

        return packet.Concat(options).ToArray();
    }

}
