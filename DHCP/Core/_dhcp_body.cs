using NICDevice.IP;
using NICDevice.MAC;

public class _dhcp_body
{
    public DHCPBootpType OP { get; init; } // 1=Request, 2=Reply (BOOTP)
    public DHCPMessageType DHCPType { get; init; } // Actual DHCP Message Type (Discover, Offer, Request, etc.)

    public string OPFormatted => $"{OP} ({(byte)OP})"; // Shows "Boot Request (1)" or "Boot Reply (2)"
    public string DHCPTypeFormatted => $"{DHCPType} ({(byte)DHCPType})"; // Shows "Discover (1)", "Request (3)", etc.

    public byte HType { get; init; }
    public byte HLen { get; init; }
    public byte Hops { get; init; }
    public uint Xid { get; init; }
    public ushort Secs { get; init; }
    public ushort Flags { get; init; }

    public IPAddress CIAddr { get; init; }
    public IPAddress YIAddr { get; init; }
    public IPAddress SIAddr { get; init; }
    public IPAddress GIAddr { get; init; }

    public MacAddress CHAddr { get; init; }
    public string? SName { get; init; }
    public string? File { get; init; }

    public byte[] MagicCookie { get; init; } = [99, 130, 83, 99];

    public Dictionary<byte, byte[]> Options { get; init; } = new();
}
