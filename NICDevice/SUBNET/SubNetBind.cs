using Lid;

namespace NICDevice.SUBNET
{
    public record SubNetBind: IBinding
    {
        public dynamic Bind(string nonParse)
        {
            return Subnet.Parse(nonParse);
        }
    }
}
