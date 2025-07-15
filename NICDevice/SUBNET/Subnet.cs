using System.Diagnostics.CodeAnalysis;

namespace NICDevice.SUBNET
{
    public struct Subnet(_4Byte subnet)
    {
        public readonly _4Byte SubnetAddress = subnet;
        public override string ToString()
        {
            return $"{SubnetAddress.byte1}.{SubnetAddress.byte2}.{SubnetAddress.byte3}.{SubnetAddress.byte4}";
        }
        public static implicit operator byte[](Subnet subnet) => subnet.SubnetAddress.bytes;
        public static implicit operator Subnet(byte[] bytes) => new Subnet(new _4Byte(bytes[0], bytes[1], bytes[2], bytes[3]));
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if(obj is Subnet s)
            {
                if(ToString() == s.ToString()) 
                    return true;
            }
            return false;
        }
        public  static Subnet Parse(byte[] bytes)
        {
            if(bytes.Length!=4)
                throw new InvalidCastException();
            return new Subnet(new _4Byte(bytes[0], bytes[1], bytes[2], bytes[3]));
        }
        public  static Subnet Parse(string subnet)
        {
            if (subnet is string str && string.IsNullOrWhiteSpace(str))
                throw new InvalidCastException(subnet);
            string[] strings = subnet.Split('.');
            if (strings.Length is > 4 or < 4)
            {
                throw new InvalidCastException(subnet); ;
            }
            byte[] bytes = new byte[4];
            byte count =0;
            foreach (string s in strings)
            {

                if (s != "255" && s != "0")
                {
                    throw new InvalidCastException($"Invalid subnet value: {s}");
                }
                bytes[count++] = byte.Parse(s);
            }
            return new Subnet(new _4Byte(bytes[0], bytes[1], bytes[2], bytes[3]));
        }
        public static bool TryParse(string subnet,out Subnet? SubNet)
        {
            try
            {
                SubNet = Parse(subnet);
                return true;
            }
            catch
            {
                SubNet = null;
                return false;
            }
        }
    }
}
