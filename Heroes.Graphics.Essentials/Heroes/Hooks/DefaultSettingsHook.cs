using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Definitions.Heroes;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
{
    /// <summary>
    /// Hooks game's applying of default settings.
    /// </summary>
    public unsafe class DefaultSettingsHook
    {
        public IHook<ReadConfigfromINI> ReadConfigFromIniHook;
        public DefaultSettings DefaultSettings { get; private set; }

        public DefaultSettingsHook(DefaultSettings defaultSettings, IReloadedHooks hooks)
        {
            DefaultSettings = defaultSettings;
            ReadConfigFromIniHook = hooks.CreateHook<ReadConfigfromINI>(ReadConfigFromIni, 0x00629CE0).Activate();
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
        [Function(FunctionAttribute.Register.eax, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee)]
        public delegate int ReadConfigfromINI(char* configPath);
    }
}
