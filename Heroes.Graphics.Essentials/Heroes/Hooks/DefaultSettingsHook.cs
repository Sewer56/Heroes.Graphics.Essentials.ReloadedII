using Reloaded.Hooks.Definitions;
using static Heroes.SDK.Classes.PseudoNativeClasses.PcPortFunctions;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
{
    /// <summary>
    /// Hooks game's applying of default settings.
    /// </summary>
    public unsafe class DefaultSettingsHook
    {
        public IHook<Native_ReadConfigfromINI> ReadConfigFromIniHook;
        public DefaultSettings DefaultSettings { get; private set; }

        public DefaultSettingsHook(DefaultSettings defaultSettings)
        {
            DefaultSettings = defaultSettings;
            ReadConfigFromIniHook = Fun_ReadConfigfromINI.Hook(ReadConfigFromIni).Activate();
        }

        private int ReadConfigFromIni(char* configPath)
        {
            int result = ReadConfigFromIniHook.OriginalFunction(configPath);
            DefaultSettings.Apply();
            return result;
        }
    }
}
