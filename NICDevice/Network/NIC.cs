using NICDevice.IP;
using NICDevice.MAC;
using NICDevice.Exceptions;
using SharpPcap;
using System;
using System.Linq;
using NICDevice.Interfaces;
using Lid;
using System.Net.WebSockets;
using NICDevice.Layers;

namespace NICDevice.Core
{
    /// <summary>
    /// Base class for Network Interface Card (NIC) operations.
    /// </summary>
    public  class NIC
    {
        [Property("mac", "SystemMacAddress")]
        [Bind(typeof(MACAddressBind))]
        public static MacAddress SystemMacAddress { get; private set; } = "AA:AA:AA:AA:99:99";
        public static IPAddress DefaultGateWay { get; protected set; } = "10.0.0.1";
        [Property("nic", "NICName")]
        public static string NICName { get; protected set; } = "Realtek Gaming 2.5GbE Family Controller";
        public static IPAddress SystemProtocolAddress { get; protected set; } = "10.0.0.100";
        public static MacAddress BroadCastMacAddress { get; protected set; } = "FF:FF:FF:FF:FF:FF";
        private  volatile   ICaptureDevice? _captureDevice;

        public NIC()
        {
            SetCaptureDevice();

        }
        protected void SetCaptureDevice()
        {
            try
            {
                if (_captureDevice != null)
                {
                    _captureDevice.StopCapture();
                    _captureDevice.Close();
                    _captureDevice.Dispose();
                }

                if (NICName is null)
                {
                    Console.WriteLine("NO SET NIC NAME");
                    return;
                }

                var devices = CaptureDeviceList.Instance;
                if (devices is null || devices.Count == 0)
                {
                    Console.WriteLine("No network devices available.");
                    return;
                }


            retry:
                // Select the first available device if none match the filter
                _captureDevice = devices.FirstOrDefault(x =>
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(NICName, StringComparison.OrdinalIgnoreCase)) ||
                    x.Name.Contains(NICName ?? "", StringComparison.OrdinalIgnoreCase));

                if (_captureDevice == null)
                {
                    Console.WriteLine("No valid NIC found, trying to select the first available interface.");
                    foreach (var device in devices)
                    {
                        Console.WriteLine(device.Description);
                    }
                    Console.WriteLine("ENTER WIFI NAME: ");
                     NICName= Console.ReadLine();
                    goto retry;
                }

                if (_captureDevice == null)
                {
                    Console.WriteLine("ERROR: No valid network interface found!");
                    return;
                }

              

                _captureDevice.Open(DeviceModes.Promiscuous);
                SystemMacAddress = _captureDevice?.MacAddress?.GetAddressBytes() ?? [0, 0, 0, 0, 0, 0];
               
            }
            catch (Exception ece)
            {
                Console.WriteLine(ece);
                Console.WriteLine("Error initializing capture device.");
            }
        }

        public ILiveDevice? CaptureDevice
        {
            get
            {
                //BooleanException.ThrowIfNull(_captureDevice, "Capture device is not initialized.");
                if(_captureDevice is null)
                {
                    SetCaptureDevice();
                    if (_captureDevice is null)
                    {
                        Console.WriteLine("NO VALID NIC");
                        return null;
                    }
                }
                if (!_captureDevice.Started)
                {
                    _captureDevice.Open(DeviceModes.Promiscuous);
                }
                return _captureDevice as ILiveDevice;
            }
        }

        public void SendPacket(params ILayer[] layers)
        {
          BooleanException.ThrowIfFalse(layers.Length > 0, "At least one layer is required");


            var packet = new PacketBuilder(layers).Build();
            CaptureDevice?.SendPacket(packet);
        }
      


        public static byte[] ConvertToSystemBytes(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
    }
}
