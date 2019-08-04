using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Config.Structures;
using Reloaded.Hooks;
using Reloaded.Hooks.X86;
using static Reloaded.Hooks.X86.FunctionAttribute;

namespace Heroes.Graphics.Essentials.Heroes
{
    /// <summary>
    /// Hooks game's applying of default settings.
    /// </summary>
    public unsafe class DefaultSettingsHook
    {
        public IHook<ReadConfigfromINI> ReadConfigFromIniHook;
        public DefaultSettings DefaultSettings { get; private set; }

        public DefaultSettingsHook(DefaultSettings defaultSettings)
        {
            DefaultSettings = defaultSettings;
            ReadConfigFromIniHook = new Hook<ReadConfigfromINI>(ReadConfigFromIni, 0x00629CE0).Activate();
        }

        private int ReadConfigFromIni(char* configPath)
        {
            int result = ReadConfigFromIniHook.OriginalFunction(configPath);
            DefaultSettings.Apply();
            return result;
        }

        /// <summary>
        /// Reads the default game configuration from an INI file and applies it to game.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [Function(Register.eax, Register.eax, StackCleanup.Callee)]
        public delegate int ReadConfigfromINI(char* configPath);
    }
}
