using NICDevice.Core;
using NICDevice.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NICDevice.Layers
{
    public abstract class Layer :NIC, ILayer
    {
        public abstract byte[] LayerBytes { get; set; }

        public abstract byte[] Payload();
        public static implicit operator byte[](Layer layer) => layer.LayerBytes;
        public static ILayer operator +(ILayer Layer, Layer layer)
        {
            Layer.LayerBytes = [.. Layer.LayerBytes, .. layer.LayerBytes];
            return Layer;
        }
        public static bool operator ==(Layer left, ILayer right)
         => left.LayerBytes.SequenceEqual(right.LayerBytes);
        public static bool operator !=(Layer left, ILayer right) =>left!=right;
    }
}
