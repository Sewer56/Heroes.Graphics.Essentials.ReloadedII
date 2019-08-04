using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Hooks;
using Reloaded.Hooks.X86;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace Heroes.Graphics.Essentials
{
    public class Program : IMod
    {
        private const string MyModId = "sonicheroes.essentials.graphics";

        public static IModLoader ModLoader;
        private GraphicsEssentials _graphicsEssentials;

        public void Start(IModLoaderV1 loader)
        {
            #if DEBUG
            Debugger.Launch();
            #endif
            ModLoader = (IModLoader)loader;

            /* Your mod code starts here. */
            _graphicsEssentials = new GraphicsEssentials(ModLoader.GetDirectoryForModId(MyModId));
        }

        /* Mod loader actions. */
        public void Suspend() { /* Not Supported */ }
        public void Resume()  { /* Not Supported */ }
        public void Unload()  { /* Not Supported */ }

        public bool CanUnload()  => false;
        public bool CanSuspend() => false;

        /* Automatically called by the mod loader when the mod is about to be unloaded. */
        public Action Disposing { get; }
    }
}
