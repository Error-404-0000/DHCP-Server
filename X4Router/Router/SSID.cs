using NICDevice.MAC;
using System;
using System.Diagnostics;

namespace X4Router.Router;

/// <summary>
/// Represents a software-hosted SSID (SoftAP) using Windows built-in Hosted Network features.
/// </summary>
public class SSID
{
    public string NetworkName { get; }
    public string Password { get; }
    public MacAddress DeviceMacAddress { get; }

    public SSID(string networkName, MacAddress deviceMacAddress, string password = "Password123")
    {
        NetworkName = networkName;
        Password = password;
        DeviceMacAddress = deviceMacAddress;
    }

    /// <summary>
    /// Creates and starts the SSID broadcast using Windows Hosted Network / SoftAP.
    /// </summary>
    public void Broadcast()
    {
        try
        {
            RunNetshCommand($"wlan set hostednetwork mode=allow ssid={NetworkName} key={Password}");
            RunNetshCommand("wlan start hostednetwork");
            Console.WriteLine($"✅ SSID \"{NetworkName}\" is now broadcasting.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to broadcast SSID: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops the hosted SSID broadcast.
    /// </summary>
    public void Stop()
    {
        try
        {
            RunNetshCommand("wlan stop hostednetwork");
            Console.WriteLine($"🛑 SSID \"{NetworkName}\" stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to stop SSID: {ex.Message}");
        }
    }

    private static void RunNetshCommand(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            Verb = "runas", // requires admin
            UseShellExecute = true
        };
        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }
}
