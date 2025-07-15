using DHCP.Core;
using Lid;
using NICDevice.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using X4Router.Router;
namespace X4Router.Router
{
    public class RouterBuilder
    {
        public SSID? SSID { get; init ; }
        public bool BroadCastSSID { get; set; } = true;
        public bool stopssid=false;
        public DHCP.Core.DHCPCore DHCP { get; set; } = new();

        
        [MethodDescription("config-dhcp", "")]

        public void edit_dhcp()
        {
            string input = null;
            while (input!= "!exit")
            {
                Console.Write("[dhcp-config]~> ");
                input = Console.ReadLine() ?? "";
                new Lid.Setter<DHCPCore>(DHCP).Execute(input);
            }
        }
        [MethodDescription("stop-ssid","")]
        public void SSID_STOP()
        {
            if(SSID is not null)
            {
                SSID.Stop();
            }
            else
            {
                Console.WriteLine("[Error] Failed to stop ssid because it is null");
            }
        }
        [MethodDescription("start-ssid", "")]
        public void SSID_Start()
        {
            if (SSID is not null)
            {
                SSID.Broadcast();
            }
            else
            {
                Console.WriteLine("[Error] Failed to stop ssid because it is null");
            }
        }
    }
}
