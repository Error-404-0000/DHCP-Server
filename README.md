# DHCP-Server Libraries

This repository contains a collection of C# libraries that can be used to build networking applications. The solution targets modern versions of .NET and focuses on DHCP server functionality and basic Wi‑Fi router utilities.
This repository contains a set of small C# class libraries related to building network applications.  The projects centre around a simple DHCP server implementation and a minimal Wi‑Fi router controller.  They can be compiled on Windows, Linux or macOS as long as the required .NET SDK is installed.

## Projects
## Repository layout

- **Lid** – Lightweight command binding utilities. It provides attributes and runtime helpers that parse strings and map them to object properties or method invocations. This is used to configure objects via CLI-style commands.
- **NICDevice** – Low level networking helpers including IP and MAC address utilities, network layers and packet construction logic.
- **DHCP** – Implementation of DHCP server logic that relies on `NICDevice` and `Lid`.
- **X4Router** – Example router tooling that exposes DHCP features and simple SSID management using Windows `netsh` commands.
```
DHCP-Server/
├── DHCP/          # Core DHCP server logic
├── Lid/           # Command parsing and property binding helpers
├── NICDevice/     # Low level networking utilities and packet builders
├── X4Router/      # Example router management library
├── DHCP.sln       # Solution file that groups the projects
└── README.md
```

### Lid
Lightweight parser and binding framework used by the other projects.  It exposes a minimal set of attributes (`BindAttribute`, `MethodDescriptionAttribute`) that can be placed on properties or methods so that strings such as those found in configuration files can be mapped to object models at runtime.

### NICDevice
Networking primitives used to build packets and manage IP and MAC addresses.  It also wraps basic SharpPcap functionality so packets can be transmitted and received without having to reference that package directly in higher level projects.

### DHCP
Implements a very small DHCP server.  Policies describing address ranges are created through the Lid command interface.  The project depends on `NICDevice` for packet formation and on `Lid` for parsing configuration commands.

Each project is defined in its own `.csproj` file and they are grouped in `DHCP.sln`.
### X4Router
Utility layer that demonstrates how the DHCP server can be integrated into a Wi‑Fi router.  It exposes a class named `RouterBuilder` which can launch the DHCP server and manipulate the wireless SSID by calling the Windows `netsh` command line tool.

Projects `project_test` and `DHCPEntry` are referenced in the solution but the source is not included in the repository.  They are optional and not required for compilation of the main libraries.

## Building

The projects target .NET 8.0 and newer (some use `net10.0`). To compile the entire solution you need a recent .NET SDK capable of building those target frameworks.
All projects target .NET 8.0 or .NET 10.0.  You will need a recent .NET SDK (for example from https://dotnet.microsoft.com/download) that supports these frameworks.  Once installed, the entire solution can be built from the repository root:

```bash
# Restore packages and build everything
# Requires the .NET SDK installed
 dotnet build DHCP.sln
dotnet build DHCP.sln
```

No executable entry point is provided in the repository; the libraries are intended to be consumed by your own application or by the sample commands in the configuration files under `DHCP/config` or `X4Router/configs`.
The build simply compiles the libraries; there is no executable application provided.  You can either reference the libraries from your own program or execute commands by feeding a configuration file into classes such as `DHCPCore` or `RouterBuilder`.

## Configuration files

## Configuration Samples
Inside `DHCP/config` and `X4Router/configs` you will find sample command files.  They demonstrate the small command language understood by the Lid parser.

The `DHCP/config/dhcp_config.cg` and `X4Router/configs/dhcp.cf` files show example command sequences that configure a DHCP server and router settings. They demonstrate how the `Lid` parser interprets commands to build policies, assign IP ranges and manage DNS servers.
Example from `DHCP/config/dhcp_config.cg`:

## Status
```
-debug true
set-gateway 10.0.0.1
set-nic {Qualcomm FastConnect 7800 Wi-Fi 7 High Band Simultaneous (HBS) Network Adapter}
new-policy {MyPolicy}
set-current-policy {MyPolicy}
set-ipaddress 10.0.0.1
configure-policy {MyPolicy}
  -ip-range-start 10.0.0.100
  -ip-range-end 10.0.0.200
  -subnet-mask 255.255.255.0
  -default-gateway 10.0.0.1
  -lease-time 86400
  -renewal-time 43200
  -rebinding-time 64800
  -logging true
add-dns-server 8.8.8.8
add-dns-server 10.0.0.2
exit
start-dhcp
show-dhcp
```

