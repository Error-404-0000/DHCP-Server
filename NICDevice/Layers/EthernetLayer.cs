using NICDevice.Core;
using NICDevice.Interfaces;
using NICDevice.MAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NICDevice.Layers
{
    public sealed class EthernetLayer(MacAddress DesMacAddress, MacAddress SrcMacAddress, short EtherType) : ILayer
    {
        public byte[] LayerBytes { get ; set ; }

        public byte[] Payload()
        {
           return LayerBytes??=[.. (byte[])(DesMacAddress??NIC.BroadCastMacAddress),..(byte[])SrcMacAddress, ..BitConverter.GetBytes(EtherType).Reverse()];
        }
    }
}
