using NICDevice.Core;
using NICDevice.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NICDevice.Protocols.DNSHANDLER
{
    public sealed class DNSLookUp(string domain) : NIC,ILayer
    {

        public byte[] LayerBytes { get; set; }

        public  byte[] Payload() =>
            LayerBytes??= [
                ..Encoding.ASCII.GetBytes(domain) ,0x00//end of Name
                ,
            ];
       
    }
    

}
