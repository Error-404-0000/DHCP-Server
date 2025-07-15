using NICDevice.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NICDevice.IP
{
    public class IPAddress(params byte[] bytes):IComparable<IPAddress>
    {
        public byte this[int index]
        {
            get => ((byte[])this)[index];
        }
        public static readonly IPAddress Broadcast = "255.255.255.255";
        public readonly byte[] IPAddressBytes = bytes;
        public readonly string IPAddressString = bytes[0].ToString() + "." + bytes[1].ToString() + "." + bytes[2].ToString() + "." + bytes[3].ToString();
        public override string ToString()
        => IPAddressString;
        public static bool TryParse(string IPAddressString, out IPAddress? IPAddress)
        {
            IPAddress = null;
            if (IPAddressString is null or "" || string.IsNullOrWhiteSpace(IPAddressString))
            {
                return false;
            }
            if (IPAddressString.Split('.').Length != 4)
            {
                return false;
            }
            var ipSegments = IPAddressString.Split('.');
            byte[] ipBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (!byte.TryParse(ipSegments[i], out ipBytes[i]))
                {
                    return false;
                }
            }
            IPAddress = new IPAddress(ipBytes[0], ipBytes[1], ipBytes[2], ipBytes[3]);
            return true;

        }
        public static implicit operator byte[](IPAddress IPAddress) => IPAddress.IPAddressBytes;

        public static IPAddress? Parse(string IPAddressString)
        {
            BooleanException.ThrowIfFalse(TryParse(IPAddressString, out IPAddress? IPAddress), "Invalid IP Address");
            return IPAddress;
        }
        public static implicit operator uint(IPAddress IPAddress) => BitConverter.ToUInt32((byte[])IPAddress, 0);
        public static implicit operator IPAddress(uint IPAddressUint) => new IPAddress(BitConverter.GetBytes(IPAddressUint).Reverse().ToArray());
        public override bool Equals(object? obj)
        {
            if (obj is IPAddress other)
            {
                return this.IPAddressString == other.IPAddressString;
            }
            return false;
        }
        public static bool operator ==(IPAddress left, IPAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IPAddress left, IPAddress right)
        {
            return !left.Equals(right);
        }
        public int CompareTo(IPAddress? other)
        {
            return BitConverter.ToUInt32((byte[])this).CompareTo(BitConverter.ToUInt32(((byte[])other ?? [0,0,0,0])));
        }

        public static implicit operator string(IPAddress IPAddress) => IPAddress.IPAddressString;
        public static implicit operator IPAddress(string IPAddressString) => Parse(IPAddressString)!;
    }
}
