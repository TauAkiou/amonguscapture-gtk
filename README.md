# amonguscapture-gtk <img src="AmongUsCapture/icon.ico" width="48">
[![Build](https://github.com/TauAkiou/amonguscapture-gtk/workflows/Beta%20releases/badge.svg)](https://github.com/tauakiou/amonguscapture-gtk/actions?query=Beta%20releases)

Capture of the local Among Us executable state

The amonguscapture-gtk project brings the amonguscapture utility to other platforms by removing the Windows dependent WinForms framework and replacing it with the open-source GTK framework (https://www.gtk.org/).

AmongusCapture-gtk is currently supported under:
* Windows
* Linux

Features that are currently only supported under Windows:
* Client Verification
* Discord IPC Links

## Requirements:

### Windows:

While you can use amonguscapture-gtk under Windows, we recommend using the [official release](https://github.com/denverquane/amonguscapture).

* .NET Core 3.1 Runtime: https://dotnet.microsoft.com/download/dotnet-core/current/runtime
* GTK For Windows Runtime: https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer

### Linux:

* .NET Core 3.1: https://docs.microsoft.com/en-us/dotnet/core/install/linux
* GTK3: Check your distribution's packaging manager for information on how to install. If you already have Gnome installed, you likely already have gtk3.

## Building



### Windows


* .NET Core SDK: https://dotnet.microsoft.com/download
* Visual Studio (Recommended)
```
    - Create a new file: 'version.txt' in the AmongUsCapture/ directory.
    - Loading the 'AmongUsCapture.sln' file and building should be sufficient enough.
```
    
 * Command Line
```
  - Navigate to the directory you cloned the git repository to, or where you extraced the source package to.
  
  - Create a new file 'version.txt' in the AmongUsCapture/ directory.
  - 'dotnet build --configuration Release' for release builds
  - 'dotnet build --configuration Debug' for debug builds
```
    
### Linux
* .NET Core 3.1 SDK: https://docs.microsoft.com/en-us/dotnet/core/install/linux
* GTK3 development libraries: Check your distribution's packaging manager for information on how to install.

#### Instructions: 

I'm currently having some trouble with the makefile, so here are some manual instructions:
```
- git clone https://github.com/TauAkiou/amonguscapture-gtk

- git checkout linux-mem-new

- cd amonguscapture-gtk

- git rev-parse HEAD > AmongUsCapture/version.txt

- dotnet build -c <release/debug>

Your code will be in amonguscapture-gtk/bin/<Release/Debug>/netcoreapp3.1>

```

Makefile instructions for when I get it working again:

    - git clone https://github.com/TauAkiou/amonguscapture-gtk/tree/linux-mem-new
    
    For release builds:
    - cd amonguscapture-gtk 
    - 'make' or 'make release'
    
    For debug builds:
    - cd amonguscapture-gtk
    - 'make debug'

