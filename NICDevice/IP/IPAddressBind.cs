using Lid;

namespace NICDevice.IP
{
    public record IPAddressBind: IBinding
    {
        public dynamic Bind(string nonParse)
        {
            return IPAddress.Parse(nonParse);
        }
    }
}
