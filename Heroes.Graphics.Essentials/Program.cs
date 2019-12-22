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

        private static IModLoader _modLoader;
        private GraphicsEssentials _graphicsEssentials;

        public void Start(IModLoaderV1 loader)
        {
            _modLoader = (IModLoader)loader;
            _modLoader.GetController<IReloadedHooks>().TryGetTarget(out var reloadedHooks);
            SDK.SDK.Init(reloadedHooks);

            /* Your mod code starts here. */
            _graphicsEssentials = new GraphicsEssentials(_modLoader.GetDirectoryForModId(MyModId), reloadedHooks);
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
