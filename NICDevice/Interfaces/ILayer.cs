using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NICDevice.Interfaces
{
    public interface  ILayer 
    {
        /// <summary>
        /// Represents the byte array for a layer, allowing storage and manipulation of binary data. Useful for handling
        /// image or data layers.
        /// </summary>
        public byte[] LayerBytes { get; set; }
        /// <summary>
        /// Retrieves the payload as a byte array. This data can be used for various purposes such as transmission or
        /// storage.
        /// </summary>
        /// <returns>Returns the payload in the form of a byte array.</returns>
        public byte[] Payload();
        public bool SequenceEqual(byte[] other)
        {
            return Payload().SequenceEqual(other);
        }
    }

}
