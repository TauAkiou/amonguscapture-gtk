using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AmongUsCapture
{
    
    [Flags]
    public enum AmongUsValidity
    {
        OK = 0b_0000_0000,
        STEAM_VERIFICAITON_FAIL = 0b_0000_0001,
        GAME_VERIFICATION_FAIL = 0b_0000_0010
    }

    public class ValidatorEventArgs : EventArgs
    {
        public AmongUsValidity Validity;
    }
    
    public static class GameVerifier
    {
        private const string steamapi32_orig_hash = "d99d425793f588fbc15f91c7765977cdd642b477be01dac41c0388ab6a5d492d";
        private const string steamapi64_orig_hash = "b8246e1a629b945fe526b24c3e4f002c4f6eb86aa1b5ed9744399f22a0d2ca9f";
        private const string amongusexe_orig_hash = "2e9029ce680f52d19d8355417e4f577bc1a69f8250f771d3ddb875f9fb586bdc";
        private const string gameassembly_orig_hash = "20530292500cadb9bbc203476a9f138159aabd09";

        public static bool VerifySteamHash(string executablePath)
        {

            var baseDllFolder = Path.Join(Directory.GetParent(executablePath).FullName,
                "/Among Us_Data/Plugins/x86/");

            var steam_apiPath = Path.Join(baseDllFolder, "steam_api.dll");
            var steam_api64Path = Path.Join(baseDllFolder, "steam_api64.dll");
            var steam_apiHash = String.Empty;
            var steam_api64Hash = String.Empty;

            using (SHA256Managed sha1 = new SHA256Managed())
            {

                using (FileStream fs = new FileStream(steam_apiPath, FileMode.Open, FileAccess.Read))
                {
                    using (var bs = new BufferedStream(fs))
                    {
                        var hash = sha1.ComputeHash(bs);
                        StringBuilder steam_apihashSb = new StringBuilder(2 * hash.Length);
                        foreach (byte byt in hash)
                        {
                            steam_apihashSb.AppendFormat("{0:X2}", byt);
                        }

                        steam_apiHash = steam_apihashSb.ToString();
                    }
                }

                using (FileStream fs = new FileStream(steam_api64Path, FileMode.Open, FileAccess.Read))
                {
                    using (var bs = new BufferedStream(fs))
                    {
                        var hash = sha1.ComputeHash(bs);
                        StringBuilder steam_api64hashSb = new StringBuilder(2 * hash.Length);
                        foreach (byte byt in hash)
                        {
                            steam_api64hashSb.AppendFormat("{0:X2}", byt);
                        }

                        steam_api64Hash = steam_api64hashSb.ToString();
                    }


                    return (String.Equals(steamapi32_orig_hash.ToUpper(), steam_apiHash) &&
                            String.Equals(steamapi64_orig_hash.ToUpper(), steam_api64Hash));

                }
            }
        }

        public static bool VerifyGameHash(string executablePath)
        {
            // This is for Beta detection.
            var baseDllFolder = Directory.GetParent(executablePath).FullName;

            var amongus_exePath = Path.Join(baseDllFolder, "Among Us.exe");
            var gameassembly_dllPath = Path.Join(baseDllFolder, "GameAssembly.dll");
            var amongus_exeHash = String.Empty;
            var gameassembly_dllHash = String.Empty;

            using (SHA256Managed sha1 = new SHA256Managed())
            {
                
                using (FileStream fs = new FileStream(amongus_exePath, FileMode.Open, FileAccess.Read))
                {
                    using (var bs = new BufferedStream(fs))
                    {
                        var hash = sha1.ComputeHash(bs);
                        StringBuilder steam_apihashSb = new StringBuilder(2 * hash.Length);
                        foreach (byte byt in hash)
                        {
                            steam_apihashSb.AppendFormat("{0:X2}", byt);
                        }

                        amongus_exeHash = steam_apihashSb.ToString();
                    }
                }
            
                    
                using (FileStream fs = new FileStream(gameassembly_dllPath, FileMode.Open, FileAccess.Read))
                {
                    using (var bs = new BufferedStream(fs))
                    {
                        var hash = sha1.ComputeHash(bs);
                        StringBuilder steam_api64hashSb = new StringBuilder(2 * hash.Length);
                        foreach (byte byt in hash)
                        {
                            steam_api64hashSb.AppendFormat("{0:X2}", byt);
                        }

                        gameassembly_dllHash = steam_api64hashSb.ToString();
                    }
                }
            }
            return (String.Equals(amongusexe_orig_hash.ToUpper(), amongus_exeHash) &&
                    String.Equals(Settings.GameOffsets.GameHash.ToUpper(), gameassembly_dllHash));
        }

    }
}