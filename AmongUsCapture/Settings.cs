using System;
using System.IO;
using Config.Net;
using Gtk;

namespace AmongUsCapture
{
    public static class Settings
    {
        public static string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmongUsCapture");

        public static ConsoleInterface conInterface;

        public static Application app;

        public static UserForm form;

        //Global persistent settings that are saved to a json file. Limited Types
        public static IPersistentSettings PersistentSettings = new ConfigurationBuilder<IPersistentSettings>().UseJsonFile(Path.Join(StorageLocation, "Settings.json")).Build();
        public static IGameOffsets GameOffsets = new ConfigurationBuilder<IGameOffsets>().UseJsonFile(Path.Join(StorageLocation, "GameOffsets.json")).Build();
    }


    public interface IPersistentSettings
    {
        //Types allowed: bool, double, int, long, string, TimeSpan, DateTime, Uri, Guid
        //DateTime is always converted to UTC
        [Option(Alias = "Host", DefaultValue = "http://localhost:8123")]
        string host { get; set; }
        
        [Option(Alias = "DebugConsole", DefaultValue = false)]
        bool debugConsole { get; set; }
        
        [Option(Alias = "SkipHandlerInstall", DefaultValue = false)]
        bool skipHandlerInstall { get; set; }
    }

    public interface IGameOffsets
    {
        //Types allowed: bool, double, int, long, string, TimeSpan, DateTime, Uri, Guid
        //DateTime is always converted to UTC

        [Option(Alias = "GameHash", DefaultValue = "5ab7b3419ed29af0728e66ae8f1a207aedd6456280128060fedf74621b287be6")]
        string GameHash { get; set; }

        [Option(Alias = "Offsets.Client", DefaultValue = 0x193C154)]
        int AmongUsClientOffset { get; set; }
        
        [Option(Alias = "Offsets.GameData", DefaultValue = 0x193C054)]
        int GameDataOffset { get; set; }
        
        [Option(Alias = "Offsets.MeetingHud", DefaultValue = 0x193BA9C)]
        int MeetingHudOffset { get; set; }
        
        [Option(Alias = "Offsets.GameStartManager", DefaultValue = 0x1858970)]
        int GameStartManagerOffset { get; set; }
        
        [Option(Alias = "Offsets.HudManager", DefaultValue = 0x1849100)]
        int HudManagerOffset { get; set; }
        
        [Option(Alias = "Offsets.ServerManager", DefaultValue = 0x184C230)]
        int ServerManagerOffset { get; set; }
    }
}