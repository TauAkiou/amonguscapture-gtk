using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace AmongUsCapture
{
    public class ProcessMemoryLinux : ProcessMemoryBase
    {
        public override bool HookProcess(string name)
        {
            // append .exe to the name of the process, since we will be hooking the exe run by Wine.
            name = name + ".exe";
            if (!IsHooked)
            {
                Process[] processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    process = processes[0];
                    if (process != null && !process.HasExited)
                    {
                        int pid = process.Id;

                        // Get PID - we will need this to calculate the /proc folder location.
                        if (Directory.Exists($"/proc/{pid}/"))
                        {
                            // Quickly run 'file -L' /proc/pid to get arch of file
                            var processval = new Process()
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "/usr/bin/file",
                                    Arguments = $"-L \"/proc/{pid}/exe\"",
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                }
                            };

                            processval.Start();
                            string result = processval.StandardOutput.ReadToEnd();
                            processval.WaitForExit();

                            bool flag = result.Contains("64-bit");

                            is64Bit = flag;

                            LoadModules();
                            IsHooked = true;

                        }

                    }
                }
            }

            return IsHooked;
        }

        public override void LoadModules()
        {
            modules = new List<Module>();

            // Seems like ReadOnlyCollections are too much of a horrible hassle to work with.
            // Therefore, we are going to collect our module data from /proc/<pid>/maps.
            if (!File.Exists($"/proc/{process.Id}/maps"))
            {
                // We don't have the maps file yet, or we ended up in a state where it doesn't exist.
                return;
            }


            var proc_maps = File.ReadLines($"/proc/{process.Id}/maps")
                .Where(s => s.Contains("GameAssembly.dll"))
                .ToList();

            if (proc_maps == null || proc_maps.Count <= 0)
            {
                // If we don't have a line, the maps file hasn't been populated yet,
                // or GameAssembly.dll hasn't been loaded.
                return;
            }

            // Linux appears to make two instances of WINE/DLL modules.

            // We want the first one, since that represents the beginning
            // of the GameAssembly.dll memory space.

            var loaded_module = new Module();

            // /proc/pid/maps legend:
            // address           perms offset  dev   inode   pathname

            string[] map_lines1 = proc_maps[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            string[] addr_vals1 = map_lines1[0].Split('-');


            int StartAddr = Int32.Parse(addr_vals1[0], System.Globalization.NumberStyles.HexNumber);
            int EndAddr = Int32.Parse(addr_vals1[1], System.Globalization.NumberStyles.HexNumber);
            int MemorySize = EndAddr - StartAddr;
            StringBuilder pathbuilder = new StringBuilder();

            // Ensure we have an absolute path by catting all potential additional strings after index 5.
            for (int x = 5; x < map_lines1.Length; x++)
            {
                pathbuilder.Append(map_lines1[x] + " ");
            }

            string librarypath = pathbuilder.ToString();

            loaded_module.Name = librarypath.Split('/').Last();
            loaded_module.BaseAddress = (IntPtr) StartAddr;
            loaded_module.FileName = librarypath;
            // I have no idea what this is. ProcessModule indicates it is 0x00 so I wll leave it as such for now.
            // This is also not used by the code, and may not even apply under Linux anyway.
            loaded_module.EntryPointAddress = IntPtr.Zero;

            modules.Add(new Module()
            {
                Name = librarypath.Split('/').Last().Trim(), // Make sure hidden characters aren't there.
                BaseAddress = (IntPtr) StartAddr,
                FileName = librarypath,
                EntryPointAddress = IntPtr.Zero
            });

        }

        public override T Read<T>(IntPtr address, params int[] offsets)
        {
            return ReadWithDefault<T>(address, default, offsets);
        }

        public override T ReadWithDefault<T>(IntPtr address, T defaultParam, params int[] offsets)
        {
            if (process == null || address == IntPtr.Zero)
            {
                return defaultParam;
            }

            int last = OffsetAddress(ref address, offsets);
            if (address == IntPtr.Zero)
                return defaultParam;

            unsafe
            {
                int size = sizeof(T);
                if (typeof(T) == typeof(IntPtr)) size = is64Bit ? 8 : 4;
                byte[] buffer = Read(address + last, size);
                fixed (byte* ptr = buffer)
                {
                    return *(T*) ptr;
                }
            }
        }

        public override string ReadString(IntPtr address)
        {
            if (process == null || address == IntPtr.Zero)
                return default;
            int stringLength = Read<int>(address + 0x8);
            byte[] rawString = Read(address + 0xC, stringLength << 1);
            return Encoding.Unicode.GetString(rawString);
        }

        public override IntPtr[] ReadArray(IntPtr address, int size)
        {
            byte[] bytes = Read(address, size * 4);
            IntPtr[] ints = new IntPtr[size];
            for (int i = 0; i < size; i++)
            {
                ints[i] = (IntPtr) BitConverter.ToUInt32(bytes, i * 4);
            }

            return ints;
        }

        private int OffsetAddress(ref IntPtr address, params int[] offsets)
        {
            byte[] buffer = new byte[is64Bit ? 8 : 4];
            IntPtr buffer_marshal;
            IntPtr local_ptr;
            IntPtr remote_ptr;

            // Reuse our buffers until we are finished.
            unsafe
            {
                buffer_marshal = Marshal.AllocHGlobal(is64Bit ? 8 : 4);
                local_ptr = Marshal.AllocHGlobal(sizeof(iovec));
                remote_ptr = Marshal.AllocHGlobal(sizeof(iovec));
            }

            for (int i = 0; i < offsets.Length - 1; i++)
            {


                var local = new iovec()
                {
                    iov_base = buffer_marshal,
                    iov_len = is64Bit ? 8 : 4
                };
                var remote = new iovec()
                {
                    iov_base = address + offsets[i],
                    iov_len = buffer.Length
                };

                Marshal.StructureToPtr(local, local_ptr, true);
                Marshal.StructureToPtr(remote, remote_ptr, true);

                LinuxAPI.process_vm_readv(process.Id, local_ptr, 1, remote_ptr, 1, 0);

                Marshal.Copy(local.iov_base, buffer, 0, buffer.Length);


                if (is64Bit)
                    address = (IntPtr) BitConverter.ToUInt64(buffer, 0);
                else
                    address = (IntPtr) BitConverter.ToUInt32(buffer, 0);
                if (address == IntPtr.Zero)
                    break;

            }

            Marshal.FreeHGlobal(local_ptr);
            Marshal.FreeHGlobal(remote_ptr);
            Marshal.FreeHGlobal(buffer_marshal);

            return offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
        }

        private byte[] Read(IntPtr address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];

            if (process == null || address == IntPtr.Zero)
            {
                return buffer;
            }

            IntPtr buffer_marshal = Marshal.AllocHGlobal(numBytes);
            IntPtr local_ptr;
            IntPtr remote_ptr;

            unsafe
            {
                local_ptr = Marshal.AllocHGlobal(sizeof(iovec));
                remote_ptr = Marshal.AllocHGlobal(sizeof(iovec));
            }

            // process_vm_readv uses the iovec structure, which needs additional marshalling.
            var local = new iovec()
            {
                iov_base = buffer_marshal,
                iov_len = numBytes
            };
            var remote = new iovec()
            {
                iov_base = address,
                iov_len = numBytes
            };

            Marshal.StructureToPtr(local, local_ptr, true);
            Marshal.StructureToPtr(remote, remote_ptr, true);

            var read = LinuxAPI.process_vm_readv(process.Id, local_ptr, 1, remote_ptr, 1, 0);

            Marshal.Copy(local.iov_base, buffer, 0, numBytes);

            Marshal.FreeHGlobal(local_ptr);
            Marshal.FreeHGlobal(remote_ptr);
            Marshal.FreeHGlobal(buffer_marshal);

            return buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct iovec
        {
            public IntPtr iov_base;
            public int iov_len;
        }

    }

    public static class LinuxAPI
    {

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern int process_vm_readv(int pid, IntPtr local_iov, ulong liovcnt, IntPtr remote_iov,
            ulong riovcnt, ulong flags);
    }


}