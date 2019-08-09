using System;
using System.Diagnostics;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace Heroes.Graphics.Essentials
{
    public class Program : IMod
    {
        private const string MyModId = "sonicheroes.essentials.graphics";

        public static IModLoader ModLoader;
        public static IReloadedHooks ReloadedHooks; // Not using Weak Reference here is OK because ReloadedHooks is not unloadable.

        private GraphicsEssentials _graphicsEssentials;

        public void Start(IModLoaderV1 loader)
        {
            #if DEBUG
            Debugger.Launch();
            #endif
            ModLoader = (IModLoader)loader;
            ModLoader.GetController<IReloadedHooks>().TryGetTarget(out ReloadedHooks);

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
