using NICDevice.Exceptions;
using NICDevice.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NICDevice
{
    public sealed class PacketBuilder(params ILayer[]  Layers)
    {
        public byte[] Build()
        {
            BooleanException.ThrowIfTrue(Layers.Length == 0, "At least one layer is required");
            byte[] packet = new byte[0];
            foreach (var layer in Layers)
            {
                packet = packet.Concat(layer.Payload()).ToArray();
            }
            return packet;
        }
    }
}
