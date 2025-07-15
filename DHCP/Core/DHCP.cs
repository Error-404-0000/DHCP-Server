using NICDevice.IP;
using NICDevice.MAC;
using NICDevice.Protocols;
using NICDevice.SUBNET;
using Lid;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using NICDevice.Core;

namespace DHCP.Core
{
    public class DHCPCore : NIC
    {
        #region HELP
        [MethodDescription("help", "Displays detailed information about all method and property")]

        public void Help()
        {
            Console.WriteLine("\rProperties: ");
            foreach (var item in this.GetType().GetProperties().Where(x => x.GetCustomAttributes<PropertyAttribute>(true).Any()).Select(x => new { Property = x.GetCustomAttribute<PropertyAttribute>(), Type = x.PropertyType }))
            {
                Console.WriteLine(item.Property.PropertyName);
                Console.WriteLine("\tType : " + item!.Type.Name);
                Console.WriteLine("\tDescription : " + item!.Property.Description);

            }
            Console.WriteLine("\rMethod(s): ");
            foreach (var item in this.GetType().GetMethods().Where(x => x.GetCustomAttributes<MethodDescriptionAttribute>(true).Any()))
            {
                var matt = item.GetCustomAttribute<MethodDescriptionAttribute>();
                Console.WriteLine(matt.Method_Name);
                Console.WriteLine("\t" + matt!.Description);
                foreach (var parm in item.GetParameters())
                {
                    if (parm.GetCustomAttribute<ParameterMethodDescriptionAttribute>() is ParameterMethodDescriptionAttribute pt)
                    {
                        Console.WriteLine($"\t {parm.Name} : {pt.Description}");
                    }
                    else
                    {
                        Console.WriteLine($"\t {parm.Name}");
                    }
                }
            }

        }
        [MethodDescription("help", "Displays detailed information about a specific method or property")]
        public void Help([ParameterMethodDescription("Method or Property Name")] string name)
        {
            var item = this.GetType().GetProperties().FirstOrDefault(x => x.GetCustomAttribute<PropertyAttribute>(true) is PropertyAttribute pt && pt.PropertyName == name);
            if (item is not null)
            {
                var Property = item.GetCustomAttribute<PropertyAttribute>();
                Console.WriteLine(Property.PropertyName);
                Console.WriteLine("\tType : " + item!.PropertyType.Name);
                Console.WriteLine("\tDescription : " + Property!.Description);
                return;
            }
            var firstMethod = this.GetType().GetMethods()
        .FirstOrDefault(x => x.GetCustomAttribute<MethodDescriptionAttribute>(true) is MethodDescriptionAttribute MD && MD.Method_Name == name);

            if (firstMethod != null)
            {
                var matt = firstMethod.GetCustomAttribute<MethodDescriptionAttribute>();
                Console.WriteLine($"mthname: {matt.Method_Name}");
                Console.WriteLine($"\t{matt!.Description}");

                var firstParam = firstMethod.GetParameters().FirstOrDefault();
                if (firstParam != null)
                {
                    if (firstParam.GetCustomAttribute<ParameterMethodDescriptionAttribute>() is ParameterMethodDescriptionAttribute pm)
                    {
                        Console.WriteLine($"\t {firstParam.Name} : {pm.Description}");
                    }
                    else
                    {
                        Console.WriteLine($"\t {firstParam.Name}");
                    }
                }
                return;
            }
            Console.WriteLine("No method or property found with the given name.");



        }
        #endregion
        public readonly List<Policy> Policies = new List<Policy>();
        private readonly Random _r = new Random();
        private Policy _current_policy = null;
        private bool is_running = false;
        private string _config_file = @"C:\\Users\\Demon\\source\\repos\\DHCP_Server\\DHCP_Server\\config\\dhcp_config.cg";
        private string _res_ips_config;
        [Property("config-file", "The configuration file for the DHCP server")]
        public string config_file
        {
            get
            {
                if (is_running)
                {
                    Console.WriteLine("Cannot change config file while DHCP server is running.");
                    return null;
                }
                if (File.Exists(_config_file))
                {
                    return _config_file;
                }
                Console.WriteLine($"{_config_file} does not exist or don't have permission");
                return null;
            }
            set
            {
                if (is_running)
                {
                    Console.WriteLine("Cannot change config file while DHCP server is running.");
                    return;
                }
                if (File.Exists(_config_file))
                    _config_file = value;
                Console.WriteLine($"{_config_file} does not exist or don't have permission");
                _config_file =value;
            }
        }
        private object SaveRes = new object();
        private void DBIP(IPAddress address,MacAddress macAddress)
        {
            if(File.Exists(_res_ips_config))
            {
                lock (SaveRes)
                {
                    using (StreamWriter sw = File.AppendText(_res_ips_config))
                    {
                        sw.WriteLine($"{macAddress} {address}");
                    }
                }
            }
        }
        public DHCPCore()
        {
            DefaultGateWay ??= "0.0.0.0";
            
        }
        [MethodDescription("set-reserved-ips-file", "set the dhcp reserved ips")]
        public void Set_res_Ip(string filename)
        {
            _res_ips_config = filename;
        }
        [MethodDescription("load-reserve-ips", "load the dhcp reserved ips")]
        public void load_reserve_ips()
        {
            if (!File.Exists(_res_ips_config))
            {
                Console.WriteLine($"Reserved IPs file not found. {_res_ips_config}");
                return;
            }
            Setter<DHCPCore> set = new Setter<DHCPCore>(this);
            var lines = File.ReadAllLines(_res_ips_config);
            foreach (var line in lines)
            {
                if (line != "exit")
                {
                    try
                    {
                        set.Execute(line);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing line '{line}': {ex.Message}");
                    }
                }
                else
                    break;
            }


        }
        [MethodDescription("load-config", "load the dhcp config file")]
        public void load_config()
        {
            if (!File.Exists(_config_file))
            {
                Console.WriteLine($"Config file not found. {_config_file}");
                return;
            }
            Setter<DHCPCore> set = new Setter<DHCPCore>(this);
            var lines = File.ReadAllLines(_config_file);
            foreach (var line in lines)
            {
                if (line != "exit")
                {
                    try
                    {
                        set.Execute(line);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing line '{line}': {ex.Message}");
                    }
                }
                else
                    break;
            }
        }

        [MethodDescription("set-gateway","set the dhcp gateway")]
        public void SetGateway(
            [ParameterMethodDescription("Gateway IP address"),Bind(typeof(IPAddressBind))] IPAddress gateway)
        {
            DefaultGateWay = gateway;
         
        }

        [MethodDescription("new-policy", "create a new policy")]
        public void NewPolicy(
            [ParameterMethodDescription("Name of the new policy")] string name)
        {
            if (Policies.Any(p => p.Name == name))
            {
                Console.WriteLine($"Policy with name {name} already exists.");
                return;
            }
            Policies.Add(new Policy(_r.Next()) { Name = name });
            Console.WriteLine($"Created new policy: {name}");
        }
        [MethodDescription("show-policies", "Show all available DHCP policies")]
        public void ShowPolicies()
        {
            Console.WriteLine("\nAVAILABLE DHCP POLICIES:");
            if (Policies.Count == 0)
            {
                Console.WriteLine("  None.");
            }
            else
            {
                foreach (var policy in Policies)
                {
                    Console.WriteLine($"  - {policy.Name}");
                }
            }
        }
        [MethodDescription("show-dhcp", "Show the current DHCP server information")]
        public void show_details()
        {
            Console.WriteLine("\nDETAILS :");
            Console.WriteLine($"\t- Is running : {is_running}");
            Console.WriteLine($"\t- Current Gateway : {DefaultGateWay}");
            Console.WriteLine($"\t- Current NIC : {NICName}");
            Console.WriteLine($"\t- Policy : {(_current_policy?.Name??"NOT SET")}:{(_current_policy?._policy_id??0)}");
        }
        [MethodDescription("set-nic", "Set the network interface card (NIC)")]
        public void SetNIC(
            [ParameterMethodDescription("Name of the NIC")] string name)
        {
            if(is_running)
            {
                Console.WriteLine("Cannot change NIC while DHCP server is running.");
                return;
            }
            NICName = name;
            SetCaptureDevice();

        }
        [MethodDescription("show-nics", "Show all available network interfaces")]
        public void ShowNIC()
        {
            Console.WriteLine("\nAVAILABLE NETWORK INTERFACES:");
            foreach (var nic in CaptureDeviceList.Instance)
            {
                Console.WriteLine($"  - {nic.Name} : Name : {nic.Description}");
            }
        }
        [MethodDescription("delete-policy", "Delete a DHCP policy")]
        public void DeletePolicy(
            [ParameterMethodDescription("Name of the policy to delete")] string name)
        {
            var policy = Policies.FirstOrDefault(p => p.Name == name);
            if (policy != null)
            {
                Policies.Remove(policy);
                Console.WriteLine($"Deleted policy: {name}");
            }
            else
            {
                Console.WriteLine($"Policy with name {name} does not exist.");
            }

        }
        [MethodDescription("set-ipaddress", "Set the dhcp ip address")]
        public void SetIPAddress(
            [ParameterMethodDescription("IP address for the DHCP server"), Bind(typeof(IPAddressBind))] IPAddress ip)
        {
            SystemProtocolAddress = ip;
        }
        [MethodDescription("set-current-policy", "Set the current DHCP policy")]
        public void SetCurrentPolicy(
            [ParameterMethodDescription("Name of the policy to set as current")] string name)
        {
            var policy = Policies.FirstOrDefault(p => p.Name == name);
            if (policy != null)
            {
                _current_policy = policy;
                Console.WriteLine($"Set current policy: {name}");
            }
            else
            {
                Console.WriteLine($"Policy with name {name} does not exist.");
            }
        }
        [MethodDescription("show-current-policy", "Show the current DHCP policy")]
        public void ShowCurrentPolicy()
        {
            if (_current_policy != null)
            {
                Console.WriteLine($"Current policy: {_current_policy.Name}");
            }
            else
            {
                Console.WriteLine("No current policy set.");
            }
        }
        [MethodDescription("show-current-policy-details", "Show the current DHCP policy details")]
        public void ShowCurrentPolicyDetails()
        {
            if (_current_policy != null)
            {
                _current_policy.ShowDhcpPolicy();
                _current_policy.ShowExcludedIps();
                _current_policy.ShowReservedIps();
            }
            else
            {
                Console.WriteLine("No current policy set.");
            }
        }
        [MethodDescription("configure-policy", "Configure the current DHCP policy")]
        public void ConfigurePolicy(
            [ParameterMethodDescription("Name of the policy to configure")] string name)
        {
            var policy = Policies.FirstOrDefault(p => p.Name == name);
            Setter<Policy> setter = new Setter<Policy>(policy);

            if (policy != null)
            {
                bool exit = false;
                while (!exit)
                {
                    Console.Write($"~/config [#{policy.Name}] > ");
                    var input = Console.ReadLine();
                    if (input == "exit")
                    {
                        exit = true;
                    }
                    else
                    {
                        try
                        {
                            setter.Execute(input);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Policy with name {name} does not exist.");
            }
        }
        [MethodDescription("add-reserved-ip", "add reserved IPs")]
        public void LoadReservedIPs(
           [ParameterMethodDescription("IP address to reserve"), Bind(typeof(IPAddressBind))] IPAddress IpAddress,
           [ParameterMethodDescription("MAC address to reserve for"), Bind(typeof(MACAddressBind))] MacAddress mac)
        {
            if (_current_policy is null)
            {
                Console.WriteLine("No current policy set. Please set a policy before adding reserved IPs.");
                return;
            }
            _current_policy.Reservations[mac] = IpAddress;
        }

     
        [MethodDescription("start-dhcp", "start dhcp server")]
        public void StartDHCP()
        {
            if(is_running)
            {
                Console.WriteLine("DHCP Server is already running.");
                return;
            }
            if(_current_policy is null)
            {
                Console.WriteLine("No current policy set. Please set a policy before starting the DHCP server.");
                return;
            }
            if(CaptureDevice is null)
            {
                Console.WriteLine("NIC NOT SET.");
                return;
            }
            is_running = true;
            CaptureDevice!.OnPacketArrival += PacketHandler;
            CaptureDevice!.StartCapture();
        }
        [MethodDescription("stop-dhcp", "stop dhcp server")]
        public void StopDHCP()
        {
           if(!is_running)
                {
                Console.WriteLine("DHCP Server is not running.");
                return;
            }
            is_running = false;
            CaptureDevice!.OnPacketArrival -= PacketHandler;
        

        }
        [MethodDescription("test-dhcp", "test dhcp server")]
        public void test_dhcp()
        {
            // This method can be used to simulate a DHCP request for testing purposes.
            // For now, it can be left empty or used to send a test DHCPDISCOVER packet.
            DHCPClient client = new DHCPClient();
            client.SendDhcpRequest("00:11:22:33:44:55", new IPAddress(new byte[] { 10, 0, 0, 3 }), new IPAddress(new byte[] { 255, 255, 255, 255 }));
        }
        [MethodDescription("test-dhcp", "test dhcp server")]
        public void test_dhcp([ParameterMethodDescription("mac"),Bind(typeof(MACAddressBind))]MacAddress mac)
        {
            // This method can be used to simulate a DHCP request for testing purposes.
            // For now, it can be left empty or used to send a test DHCPDISCOVER packet.
            DHCPClient client = new DHCPClient();
            client.SendDhcpRequest(mac, new IPAddress(new byte[] { 10, 0, 0, 3 }), new IPAddress(new byte[] { 255, 255, 255, 255 }));
        }
        #region DHCP HANDLER
        private SortedList<MacAddress, IPAddress> _reservedIPs = new SortedList<MacAddress, IPAddress>();
        public _dhcp_body _byte_to_dhcp_body(byte[] bytes)
        {
            if (bytes.Length < 240) throw new ArgumentException("Invalid DHCP packet size");

            // Parse DHCP Options
            var options = ParseDHCPOptions(bytes[240..]);

            // Extract DHCP Message Type from Option 53 (if present)
            DHCPMessageType dhcpType = DHCPMessageType.Discover; // Default to Discover if Option 53 is missing
            if (options.TryGetValue(53, out byte[] typeBytes) && typeBytes.Length == 1)
            {
                dhcpType = (DHCPMessageType)typeBytes[0];
            }

            return new _dhcp_body()
            {
                OP = (DHCPBootpType)bytes[0], // BOOTP Operation Code (1=Request, 2=Reply)
                DHCPType = dhcpType, // Set the actual DHCP Message Type

                HType = bytes[1],
                HLen = bytes[2],
                Hops = bytes[3],
                Xid = BitConverter.ToUInt32(bytes, 4),
                Secs = BitConverter.ToUInt16(bytes, 8),
                Flags = BitConverter.ToUInt16(bytes, 10),
                CIAddr = new IPAddress(bytes[12..16]),
                YIAddr = new IPAddress(bytes[16..20]),
                SIAddr = new IPAddress(bytes[20..24]),
                GIAddr = new IPAddress(bytes[24..28]),
                CHAddr = BitConverter.ToString(bytes[28..34]).Replace("-", ":"), // Convert MAC to String
                SName = Encoding.ASCII.GetString(bytes[44..108]).Trim('\0'),
                File = Encoding.ASCII.GetString(bytes[108..236]).Trim('\0'),
                MagicCookie = bytes[236..240],
                Options = options // Store parsed options
            };
        }


        // **Dynamic DHCP Options Parser**
        private static Dictionary<byte, byte[]> ParseDHCPOptions(byte[] optionsData)
        {
            var options = new Dictionary<byte, byte[]>();
            int index = 0;

            while (index < optionsData.Length)
            {
                if (optionsData[index] == 255) break; // End of Options (0xFF)

                byte optionType = optionsData[index];

                // Prevent out of range when only an option type exists without length
                if (index + 1 >= optionsData.Length) break;

                byte optionLen = optionsData[index + 1];

                // Ensure we do not read beyond array length
                if (index + 2 + optionLen > optionsData.Length) break;

                // Correct slicing: Extract bytes correctly
                byte[] optionValue = optionsData[(index + 2)..(index + 2 + optionLen)];

                options[optionType] = optionValue;

                index += 2 + optionLen; // Move to the next option
            }

            return options;
        }
        #region Threading
        private static readonly Channel<byte[]> _dhcpRequestQueue =
    Channel.CreateBounded<byte[]>(new BoundedChannelOptions(10000) // Queue max size
    {
        SingleWriter = false,
        SingleReader = false,
        FullMode = BoundedChannelFullMode.Wait // Prevents excessive memory usage
    });

        #endregion

        private void PacketHandler(object sender, scoped PacketCapture e)
        {
            if(_current_policy is null || !is_running)
            {
                return;
            }
             byte[] packet = e.GetPacket().Data;
            if (packet.Length is 0 or < 278) return;//DHCP MIN SIZE
            ushort udpPort = (ushort)((packet[36] << 8) | packet[37]); // Convert to integer
            if (udpPort != 67&&udpPort!=68) return;
            if (!(packet[278] == 99 && packet[279] == 130 && packet[280] == 83 && packet[281] == 99)) return;//Magic Cookie Verification
         
            _=Task.Run(() => HANDLE_DHCP_REQUEST( packet.Skip(42).ToArray()));

        }
         
        private void HANDLE_DHCP_REQUEST( byte[] DHCPPacket)
        {
            _dhcp_body _Dhcp_Body = _byte_to_dhcp_body(DHCPPacket);
            if (_Dhcp_Body.DHCPType == DHCPMessageType.Discover)
                Discover(_Dhcp_Body);
            else if(_Dhcp_Body.DHCPType == DHCPMessageType.Request)
            {
                HANDLE_DHCP_REQUEST(_Dhcp_Body); ;
            }
            else if(_Dhcp_Body.DHCPType == DHCPMessageType.Acknowledge)
            {
                var src_ip = _Dhcp_Body.Options[54];
                HANDLE_ACK_REQUEST(_Dhcp_Body, new IPAddress([..src_ip]));
            }
        }
        private void HANDLE_ACK_REQUEST(_dhcp_body dhcp_Body,IPAddress src)
        {
           
                if (_reservedIPs.TryGetValue(dhcp_Body.CHAddr, out var ip))
                {
                    SEND_DHCP_ACK(dhcp_Body, ip);
                }
                else
                {
                    SEND_DHCP_NAK(dhcp_Body);
                }
            
        }
        private void HANDLE_DHCP_REQUEST(_dhcp_body _dhcp_body)
        {
            if (_current_policy is null)
                return;
            if (_dhcp_body.DHCPType != DHCPMessageType.Request)
                return;

            // Extract Requested IP (Option 50)
            IPAddress requestedIP = ExtractOptionIPAddress(_dhcp_body.Options, 50);
            if (requestedIP.ToString() == "0.0.0.0")
                requestedIP = _dhcp_body.CIAddr;
            if (_current_policy.Exclusions.Any(x=>x.ToString()== requestedIP))
            {
                SEND_DHCP_NAK(_dhcp_body); // Requested IP is excluded
                return;
            }
            // Ensure requested IP is available for this client
            if (_current_policy.Reservations.TryGetValue(_dhcp_body.CHAddr, out var reservedIP) && reservedIP.Equals(requestedIP))
            {
                _reservedIPs[_dhcp_body.CHAddr] = requestedIP;
                Task.Run(() => DBIP(requestedIP, _dhcp_body.CHAddr));
                SEND_DHCP_ACK(_dhcp_body, requestedIP);
            }
            else if (requestedIP.ToString() != "0.0.0.0"&&_reservedIPs.ContainsValue(requestedIP) && !_reservedIPs.ContainsKey(_dhcp_body.CHAddr))
            {
                SEND_DHCP_NAK(_dhcp_body); // IP already in use by another client
            }
            else if(_reservedIPs.TryGetValue(_dhcp_body.CHAddr,out var ip) && ip == requestedIP)
            {
                SEND_DHCP_ACK(_dhcp_body, requestedIP);

            }
            else if (!IPInUse(requestedIP))
            {
                NICDevice.Protocols.ARP _arp_request = new(requestedIP);
                _arp_request.Send();
                if (_arp_request.Listen() is  null)
                {
                    if(requestedIP.ToString() is not "0.0.0.0")
                    {
                        _reservedIPs[_dhcp_body.CHAddr] = requestedIP;

                    }
                    SEND_DHCP_ACK(_dhcp_body, requestedIP);
                }
                else
                    SEND_DHCP_NAK(_dhcp_body);
                
            }
            else
            {
                SEND_DHCP_NAK(_dhcp_body);
            }
        }

        private void SEND_DHCP_NAK(_dhcp_body _dhcp_body)
        {
            IPAddress destination = _dhcp_body.GIAddr.Equals(IPAddress.Parse("0.0.0.0"))
                ? IPAddress.Parse("255.255.255.255")
                : _dhcp_body.GIAddr;

            UDP udpPacket = new UDP(_dhcp_body.CHAddr, destination, 67,
                _dhcp_body_to_bytes(_build_dhcp_body(_dhcp_body, DHCPMessageType.NegativeAcknowledge, IPAddress.Parse("0.0.0.0"))),
                68);

            udpPacket.Send();
            Console.WriteLine($"Declined request for {_dhcp_body.CHAddr}");
        }

        private void SEND_DHCP_ACK(_dhcp_body _dhcp_body, IPAddress assignedIP)
        {
            // Determine if we should use broadcast or unicast
            IPAddress destination = _dhcp_body.GIAddr.Equals(IPAddress.Parse("0.0.0.0"))
                ? IPAddress.Broadcast  // Broadcast is safer for DHCPACK
                : _dhcp_body.GIAddr;   // Use GIAddr if relayed


            // Construct the DHCPACK packet with correct ports
            UDP udpPacket = new UDP(
                _dhcp_body.CHAddr,    // Client MAC Address
                destination,          // Destination IP
                68,                   // Dees Port (Server)
                _dhcp_body_to_bytes(
                    _build_dhcp_body(_dhcp_body, DHCPMessageType.Acknowledge, assignedIP)
                ),
                67              // SRC Port (Client)
            );

            udpPacket.Send();
            Console.WriteLine($"Accpted {assignedIP} for {_dhcp_body.CHAddr}");
        }

        private IPAddress ExtractOptionIPAddress(Dictionary<byte, byte[]> options, byte optionCode)
        {
            if (options.TryGetValue(optionCode, out byte[] ipBytes) && ipBytes.Length == 4)
            {
                return new IPAddress(ipBytes);
            }
            return IPAddress.Parse("0.0.0.0"); // Return default if option is missing
        }

        private void Discover(_dhcp_body _dhcp_body)
        {
            IPAddress _av_ip_address = IPAddress.Parse("0.0.0.0");
            if (_dhcp_body.DHCPType != DHCPMessageType.Discover || _current_policy is null)
                return;

            // Check if the client has a reserved IP
            if (_current_policy.Reservations.FirstOrDefault(x=>x.Key.ToString()==_dhcp_body.CHAddr).Value is IPAddress v && v is not null)
            {
                _av_ip_address = v;
                _send_dhcp_offer(_dhcp_body, _av_ip_address);
                return;
            }

            // Check if the client is already in the reserved list
            if (_reservedIPs.FirstOrDefault(x => x.Key.ToString() == _dhcp_body.CHAddr).Value is IPAddress _e && _e is not null)
                _av_ip_address = _e;
            else
            {
                _av_ip_address = _find_ip();
                if (_av_ip_address.ToString() == "0.0.0.0") return; // No IP available
            }

            _send_dhcp_offer(_dhcp_body, _av_ip_address);
        }

        private _dhcp_body _build_dhcp_body(_dhcp_body _dhcp_body, DHCPMessageType _message_type, IPAddress offeredIP = null)
        {
            return new _dhcp_body()
            {
                OP = (_message_type == DHCPMessageType.Offer || _message_type == DHCPMessageType.Acknowledge || _message_type == DHCPMessageType.NegativeAcknowledge)
                    ? DHCPBootpType.Reply // Server response messages
                    : DHCPBootpType.Request, // Client messages (Discover, Request, Release)

                DHCPType = _message_type, // Store the detailed DHCP message type

                CIAddr = offeredIP??IPAddress.Parse("0.0.0.0"),
                YIAddr = offeredIP ?? _dhcp_body.YIAddr,
                SIAddr = SystemProtocolAddress,
                HLen = _dhcp_body.HLen,
                Hops = _dhcp_body.Hops,
                HType = _dhcp_body.HType,
                GIAddr = _dhcp_body.GIAddr,
                CHAddr = _dhcp_body.CHAddr,
                Xid = _dhcp_body.Xid,

                Options = new Dictionary<byte, byte[]>
        {
            { 53, new byte[] { (byte)_message_type } }, // DHCP Message Type
            { 1, ConvertToByteArray(_current_policy.SubnetMask) },
            { 3, ConvertToByteArray(_current_policy.Gateway) },
            { 51, ConvertInt32ToBytes(_current_policy.LeaseTime) },
            { 58, ConvertInt32ToBytes(_current_policy.RenewalTime) },
            { 59, ConvertInt32ToBytes(_current_policy.RebindingTime) }
        }
            };
        }


        private void _send_dhcp_offer(_dhcp_body _dhcp_body, IPAddress offeredIP)
        {
            Console.WriteLine($"Sending DHCPOFFER to {_dhcp_body.CHAddr} with IP {offeredIP}");
            UDP udpPacket = new UDP(_dhcp_body.CHAddr, "255.255.255.255", 68, _dhcp_body_to_bytes(_build_dhcp_body(_dhcp_body,DHCPMessageType.Offer, offeredIP)),67);
            udpPacket.Send();
        }
        private byte[] ConvertInt32ToBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes); // Ensure Big-Endian
            return bytes;
        }

        private byte[] ConvertToByteArray(object value)
        {
            if (value is byte[] bytes) return bytes; // Already byte[], return directly

            if (value is Subnet subnet) return subnet; // Convert SubnetMask
            if (value is IPAddress ip) return ip;// Convert Custom IP Address
            

            throw new InvalidCastException("Unsupported type for DHCP conversion.");
        }


        private byte[] _dhcp_body_to_bytes(_dhcp_body dhcp)
        {
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);

            // Write fixed DHCP fields
            writer.Write((byte)dhcp.DHCPType);
            writer.Write(dhcp.HType);
            writer.Write(dhcp.HLen);
            writer.Write(dhcp.Hops);
            writer.Write(BitConverter.GetBytes(dhcp.Xid));
            writer.Write(BitConverter.GetBytes(dhcp.Secs));
            writer.Write(BitConverter.GetBytes(dhcp.Flags));
            writer.Write((byte[])dhcp.CIAddr);
            writer.Write((byte[])dhcp.YIAddr);
            writer.Write((byte[])dhcp.SIAddr);
            writer.Write((byte[])dhcp.GIAddr);

            // Write MAC Address (Client Hardware Address, 16 bytes)
            byte[] macBytes = dhcp.CHAddr; // Convert MAC to bytes
            writer.Write(macBytes);
            writer.Write(new byte[16 - macBytes.Length]); // Fill remaining space with 0s

            // Write Server Name (64 bytes, padded with 0s)
            byte[] snameBytes = Encoding.ASCII.GetBytes(dhcp.SName ?? "");
            writer.Write(snameBytes);
            writer.Write(new byte[64 - snameBytes.Length]); // Padding

            // Write Boot File Name (128 bytes, padded with 0s)
            byte[] fileBytes = Encoding.ASCII.GetBytes(dhcp.File ?? "");
            writer.Write(fileBytes);
            writer.Write(new byte[128 - fileBytes.Length]); // Padding

            // Write Magic Cookie (Always 99 130 83 99)
            writer.Write(new byte[] { 99, 130, 83, 99 });

            // Write DHCP Options
            foreach (var option in dhcp.Options)
            {
                writer.Write(option.Key); // Option Type
                writer.Write((byte)option.Value.Length); // Option Length
                writer.Write(option.Value); // Option Value
            }

            // End Option (255)
            writer.Write((byte)255);

            return ms.ToArray();
        }

        private IPAddress _find_ip()
        {
            var start_ip = _current_policy.StartRange;
            var end_ip = _current_policy.EndRange;

            uint start = IPToUint(start_ip);
            uint end = IPToUint(end_ip);

            HashSet<string> usedIPs = new HashSet<string>(_reservedIPs.Values.Select(x => x.ToString()));
            HashSet<string> excludedIPs = new HashSet<string>(_current_policy.Exclusions.Select(x=>x.ToString()));

            for (uint ip = start; ip <= end; ip++)
            {
                var candidateIP = UintToIP(ip);

                if (!usedIPs.Contains(candidateIP) && !excludedIPs.Contains(candidateIP))
                {
                    return candidateIP;
                }
            }

            return IPAddress.Parse("0.0.0.0"); 
        }
        private bool IPInUse(IPAddress ip)
        {
            return _reservedIPs.Any(x=>x.Value.ToString()== ip);
        }
        // Convert IP to uint for iteration
        private uint IPToUint(IPAddress ip)
        {
            byte[] bytes = ip;
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        }

        // Convert uint to IP
        private IPAddress UintToIP(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        #endregion


    }
}
