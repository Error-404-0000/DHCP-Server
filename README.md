# DHCP-Server Libraries

This repository contains a set of small C# class libraries related to building network applications.  The projects centre around a simple DHCP server implementation and a minimal Wi‑Fi router controller.  They can be compiled on Windows, Linux or macOS as long as the required .NET SDK is installed.

## Repository layout

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

### X4Router
Utility layer that demonstrates how the DHCP server can be integrated into a Wi‑Fi router.  It exposes a class named `RouterBuilder` which can launch the DHCP server and manipulate the wireless SSID by calling the Windows `netsh` command line tool.

Projects `project_test` and `DHCPEntry` are referenced in the solution but the source is not included in the repository.  They are optional and not required for compilation of the main libraries.

## Building

All projects target .NET 8.0 or .NET 10.0.  You will need a recent .NET SDK (for example from https://dotnet.microsoft.com/download) that supports these frameworks.  Once installed, the entire solution can be built from the repository root:

```bash
dotnet build DHCP.sln
```

The build simply compiles the libraries; there is no executable application provided.  You can either reference the libraries from your own program or execute commands by feeding a configuration file into classes such as `DHCPCore` or `RouterBuilder`.

## Configuration files

Inside `DHCP/config` and `X4Router/configs` you will find sample command files.  They demonstrate the small command language understood by the Lid parser.

Example from `DHCP/config/dhcp_config.cg`:

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

The X4Router configuration is almost identical and enables the same DHCP policy while optionally controlling wireless SSID broadcasting.

## Repository status and license

The repository appears to be a work in progress.  Some projects referenced by the solution are missing and there is no license file.  Treat all code as proprietary unless a license is added in the future.
