using NICDevice.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NICDevice.MAC
{
    public class MacAddress: IComparable<MacAddress>
    {
        public static readonly MacAddress Broadcast = new MacAddress(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        public readonly byte[] MacAddressBytes;
        public string MacAddressString => ToString();
        public static implicit operator byte[](MacAddress MacAddress) => MacAddress.MacAddressBytes;
        public MacAddress(params byte[] Bytes)
        {
            BooleanException.ThrowIfFalse(Bytes.Length == 6, "Invalid MAC Address");
            MacAddressBytes = Bytes;
        }
        public static MacAddress Random()
        {
            Random random = new Random();
            byte[] macBytes = new byte[6];
            random.NextBytes(macBytes);
            return new MacAddress(macBytes);
        }
        public override string ToString()
        {
            return string.Join(":", MacAddressBytes.Select(b => b.ToString("X2")));
        }
        public static bool TryParse(string MacAddressString, out MacAddress? MacAddress)
        {
            MacAddress = null;
            if (MacAddressString is null or "" || string.IsNullOrWhiteSpace(MacAddressString))
            {
                return false;
            }
            if (MacAddressString.Split(':', '-').Length != 6)
            {
                return false;
            }
            var macSegments = MacAddressString.Split(':', '-');
            byte[] macBytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                if (!byte.TryParse(macSegments[i], System.Globalization.NumberStyles.HexNumber, null, out macBytes[i]))
                {
                    return false;
                }
            }
            MacAddress = new MacAddress(macBytes);
            return true;
        }
        public static MacAddress? Parse(string MacAddressString)
        {
            BooleanException.ThrowIfFalse(TryParse(MacAddressString, out MacAddress? MacAddress), "Invalid MAC Address");
            return MacAddress;
        }
        public byte this[int index]
        {
            get => MacAddressBytes[index];

        }
        public int CompareTo(MacAddress? other)
        {
            for (int i = 0; i < 6; i++)
            {
                int diff = this[i].CompareTo(other[i]);
                if (diff != 0)
                    return diff;
            }
            return 0; // They are equal
        }

        public static implicit operator string(MacAddress MacAddress) => MacAddress.MacAddressString;
        public static implicit operator MacAddress(string MacAddressString) => Parse(MacAddressString)!;
        public static implicit operator MacAddress(byte[] MacAddressBytes) => new MacAddress(MacAddressBytes);
    }
}
