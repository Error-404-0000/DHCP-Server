using Lid;

namespace NICDevice.MAC
{
    public record MACAddressBind : IBinding
    {
        public dynamic Bind(string nonParse)
        {
            return MacAddress.Parse(nonParse)!;
        }
    }
}
