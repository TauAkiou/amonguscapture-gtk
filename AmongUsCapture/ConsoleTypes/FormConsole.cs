using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using AmongUsCapture.TextColorLibrary;

namespace AmongUsCapture.ConsoleTypes
{
    public class FormConsole : ConsoleInterface
    {
        private StreamWriter logFile;
        public UserForm form;
        private static object locker = new Object();

        public FormConsole(UserForm userForm)
        {
            form = userForm;
            var directoryuri = Assembly.GetEntryAssembly().GetName().CodeBase.Substring(7);
            logFile = File.CreateText(Path.Combine(Directory.GetParent(directoryuri).ToString(), "CaptureLog.txt"));
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            throw new NotImplementedException();
        }

        public void WriteColoredText(string ColoredText)
        {
            form.WriteColoredText(ColoredText);
            WriteToLog(ColoredText);
        }


        public void WriteLine(string s)
        {
            throw new NotImplementedException();
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
            WriteToLog($"[{ModuleName}]: {text}");
        }


        public void WriteToLog(string textToLog)
        {
            WriteLogLine(DateTime.UtcNow, textToLog);
        }

        private string StripColor(string text)
        {
            return TextColor.StripColor(text);
        }

        private void WriteLogLine(DateTime time, string textToLog)
        {
            lock (locker)
            {
                logFile.WriteLine($"{time.ToLongTimeString()} | {StripColor(textToLog)}");
                logFile.Flush();
            }
            
        }
    }
}
