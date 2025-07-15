using NICDevice.IP;
using NICDevice.MAC;
using NICDevice.SUBNET;
using Lid;
using System;
using System.Collections.Generic;
public class Policy:CLIHelp
{
    public Policy()
    {
        
    }
    public Policy(int policy_id)
    {
        _policy_id  = policy_id;
    }
    public readonly int _policy_id ;
    private IPAddress _start_range;
    private IPAddress _end_range;

    [Property("name", "PolicyName")]
    public string Name { get; set; }

    [Property(cmd: "ip-range-start", des: "Starting IP address for allocation")]
    [Bind(typeof(IPAddressBind))]
    public IPAddress StartRange
    {
        get
        {
            return _start_range;
        }
        set
        {
            if (EndRange is not null&& BitConverter.ToUInt32((byte[])value) >= BitConverter.ToUInt32(((byte[])EndRange)))
            {
                throw new IndexOutOfRangeException("ip-range-start can't be smaller than(<) or eq(==) to ip-range-end");
            }
            _start_range = value;
        }
    }

    [Bind(typeof(IPAddressBind))]
    [Property(cmd: "ip-range-end", des: "Ending IP address for allocation")]
    public IPAddress EndRange { get=> _end_range; set {
            if (StartRange is not null && BitConverter.ToUInt32(((byte[])StartRange))>=BitConverter.ToUInt32((byte[])value))
            {
                throw new IndexOutOfRangeException("ip-range-end can't be grater than(>) or eq(==) to  ip-range-start");
            }
            _end_range = value;
    } }

    [Property(cmd: "subnet-mask", des: "Subnet mask defining network size")]
    [Bind(typeof(SubNetBind))]
    public Subnet SubnetMask { get; set; }

    [Property(cmd: "default-gateway", des: "Default gateway assigned to clients")]
    [Bind(typeof(IPAddressBind))]
    public IPAddress Gateway { get; set; }

    [Property(cmd: "lease-time", des: "Duration (in seconds) before lease expiration")]
    public int LeaseTime { get; set; }

    [Property(cmd: "renewal-time", des: "Time (in seconds) before clients renew lease")]
    public int RenewalTime { get; set; }

    [Property(cmd: "rebinding-time", des: "Time (in seconds) before clients rebind to any DHCP server")]
    public int RebindingTime { get; set; }

    [Property(cmd: "excluded-addresses", des: "List of excluded IP addresses")]
    public List<IPAddress> Exclusions { get; set; } = new List<IPAddress>();

    [Property(cmd: "reserved-addresses", des: "Reserved IP addresses mapped to MAC addresses")]
    public Dictionary<MacAddress, IPAddress> Reservations { get; set; } = new Dictionary<MacAddress, IPAddress>();

    [Property(cmd: "relay-agent", des: "IP helper address for forwarding DHCP requests")]
    [Bind(typeof(IPAddressBind))]
    public IPAddress RelayAgent { get; set; }

    [Property(cmd: "dhcp-options", des: "Additional DHCP options (PXE, domain suffix, etc.)")]
    public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();

    [Property(cmd: "failover-partner", des: "Secondary DHCP server for failover")]
    [Bind(typeof(IPAddressBind))]
    public IPAddress FailoverPartner { get; set; }

    [Property(cmd: "rate-limit", des: "Maximum number of leases per MAC address")]
    public int RateLimit { get; set; }

    [Property(cmd: "binding-mode", des: "Lease binding mode (Strict or Flexible)")]
    public string BindingMode { get; set; }

    [Property(cmd: "logging", des: "Enable or disable DHCP logging")]
    public bool Logging { get; set; }
  
    [MethodDescription("add-dns-server", "Add a DNS server to the policy")]
    public void AddDnsServer(
     [ParameterMethodDescription( "DNS server IP address"),Bind(typeof(IPAddressBind))] IPAddress dns)
    {
        if (!Options.ContainsKey("DNS"))
        {
            Options["DNS"] = new List<IPAddress>();
        }
     ((List<IPAddress>)Options["DNS"]).Add(dns);
        Console.WriteLine($"DNS server {dns} added.");
    }

    [MethodDescription("remove-dns-server", "Remove a DNS server from the policy")]
    public void RemoveDnsServer(
        [ParameterMethodDescription("DNS server IP address"), Bind(typeof(IPAddressBind))] IPAddress dns)
    {
        if (!Options.ContainsKey("DNS"))
        {
            Console.WriteLine("No DNS servers set.");
            return;
        }
        if (!((List<IPAddress>)Options["DNS"]).Contains(dns))
        {
            Console.WriteLine($"DNS server {dns} not found (no changes made).");
            return;
        }
        ((List<IPAddress>)Options["DNS"]).Remove(dns);
        Console.WriteLine($"Removed DNS server {dns}.");
    }
    [MethodDescription("exclude-ip", "Exclude a specific IP from being assigned")]
    public void ExcludeIp(
        [ParameterMethodDescription("IP address to exclude"), Bind(typeof(IPAddressBind))] IPAddress ip)
    {
        if(Exclusions.Contains(ip))
        {
            Console.WriteLine($"IP {ip} already excluded (no changes made).");
            return;
        }
        Exclusions.Add(ip);
        Console.WriteLine($"Excluded IP {ip}.");
    }
    [MethodDescription("remove-excluded-ip", "Remove an excluded IP from the list")]
    public void RemoveExcludedIp(
        [ParameterMethodDescription("IP address to remove from exclusion"), Bind(typeof(IPAddressBind))] IPAddress ip)
    {
        if (!Exclusions.Contains(ip))
        {
            Console.WriteLine($"IP {ip} not found in exclusions (no changes made).");
            return;
        }
        Exclusions.Remove(ip);
        Console.WriteLine($"Removed IP {ip} from exclusions.");
    }

    [MethodDescription("reserve-ip", "Reserve an IP for a specific MAC address")]
    public void ReserveIp(
        [ParameterMethodDescription( "MAC address to reserve for"), Bind(typeof(MACAddressBind))] MacAddress mac,
        [ParameterMethodDescription("IP address to assign"), Bind(typeof(IPAddressBind))] IPAddress ip)
    {
        Reservations[mac] = ip;
        Console.WriteLine($"Reserved {ip} for MAC {mac}.");
    }


    [MethodDescription("show-dhcp-policy", "Show full DHCP policy details")]
    public void ShowDhcpPolicy()
    {
        Console.WriteLine("\nDHCP POLICY CONFIGURATION:");
        Console.WriteLine($"  Policy Name      : {Name}");
        Console.WriteLine($"  IP Range         : {StartRange} - {EndRange}");
        Console.WriteLine($"  Subnet Mask      : {SubnetMask}");
        Console.WriteLine($"  Default Gateway  : {Gateway}");
        Console.WriteLine($"  Lease Time       : {LeaseTime} sec");
        Console.WriteLine($"  Renewal Time     : {RenewalTime} sec");
        Console.WriteLine($"  Rebinding Time   : {RebindingTime} sec");
        Console.WriteLine($"  Failover Partner : {FailoverPartner}");
        Console.WriteLine($"  Binding Mode     : {BindingMode}");
        Console.WriteLine($"  Logging Enabled  : {Logging}");
    }

    [MethodDescription("show-excluded-ips", "Show all excluded IPs")]
    public void ShowExcludedIps()
    {
        Console.WriteLine("\nEXCLUDED IP ADDRESSES:");
        if (Exclusions.Count == 0)
        {
            Console.WriteLine("  None.");
        }
        else
        {
            foreach (var ip in Exclusions)
            {
                Console.WriteLine($"  - {ip}");
            }
        }
    }

    [MethodDescription("show-reserved-ips", "Show all reserved IPs")]
    public void ShowReservedIps()
    {
        Console.WriteLine("\nRESERVED IP ADDRESSES:");
        if (Reservations.Count == 0)
        {
            Console.WriteLine("  None.");
        }
        else
        {
            foreach (var res in Reservations)
            {
                Console.WriteLine($"  - MAC: {res.Key} → IP: {res.Value}");
            }
        }
    }
    [MethodDescription("add-dhcp-option", "Add a custom DHCP option")]
    public void AddDhcpOption(
        [ParameterMethodDescription("Option name")] string name,
        [ParameterMethodDescription("Option value")] object value)
    {
        if (Options.ContainsKey(name))
        {
            Console.WriteLine($"Option {name} already set (no changes made).");
            return;
        }
        Options[name] = value;
        Console.WriteLine($"Added DHCP option: {name} = {value}");
    }
    [MethodDescription("remove-dhcp-option", "Remove a custom DHCP option")]
    public void RemoveDhcpOption(
        [ParameterMethodDescription("Option name")] string name)
    {
        if (!Options.ContainsKey(name))
        {
            Console.WriteLine($"Option {name} not found (no changes made).");
            return;
        }
        Options.Remove(name);
        Console.WriteLine($"Removed DHCP option: {name}");
    }
    [MethodDescription("show-dhcp-options", "Show all DHCP options set")]
    public void ShowDhcpOptions()
    {
        Console.WriteLine("\nDHCP OPTIONS:");
        if (Options.Count == 0)
        {
            Console.WriteLine("  No additional options set.");
        }
        else
        {
            foreach (var option in Options)
            {
                Console.WriteLine($"  - {option.Key}: {option.Value}");
            }
        }
    }

}
