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

## Prebuilt Releases
Prebuilt releases are available here: [prebuilt releases](https://github.com/TauAkiou/amonguscapture-gtk/releases)

## NOTE FOR LINUX USERS:

Some linux distributions will prevent AmongUsCapture from linking with the Among Us game process. This is due to a [ptrace scope security feature](https://askubuntu.com/questions/146160/what-is-the-ptrace-scope-workaround-for-wine-programs-and-are-there-any-risks).

In order for the capture to work, you must use `setcap` to add ptrace capabilities to the Capture process. Note that doing so will require root!

```
sudo setcap cap_sys_ptrace=eip <capture executable>
```

Also note that moving or changing the capture in any way will strip these permissions from the executable and you will have to setcap the capture again.

Distributions known to be affected by this:
* Ubuntu
* Arch

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
* Visual Studio
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

```
- git clone https://github.com/TauAkiou/amonguscapture-gtk

- cd amonguscapture-gtk

- dotnet build -c <release/debug>
```

Your code will be in amonguscapture-gtk/bin/<Release/Debug>/netcoreapp3.1> as "amonguscapture.dll".

If you want a standalone executable: 

```
- cd amonguscapture-gtk

- dotnet publish "AmongUsCapture\AmongUsCapture.csproj" -p:PublishProfile=LinuxProfilex64 -p:AssemblyVersion=0.0.1
```



