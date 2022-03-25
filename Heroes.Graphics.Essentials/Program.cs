using System;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace Heroes.Graphics.Essentials;

public class Program : IMod
{
    private static IModLoader _modLoader;
    private GraphicsEssentials _graphicsEssentials;

    public void StartEx(IModLoaderV1 loader, IModConfigV1 config)
    {
        _modLoader = (IModLoader)loader;
        _modLoader.GetController<IReloadedHooks>().TryGetTarget(out var reloadedHooks);
        SDK.SDK.Init(reloadedHooks, null);

        /* Your mod code starts here. */
        var modFolder    = _modLoader.GetDirectoryForModId(config.ModId);
        var configFolder = _modLoader.GetModConfigDirectory(config.ModId);
        _graphicsEssentials = new GraphicsEssentials(modFolder, configFolder, reloadedHooks);
    }

    /* Mod loader actions. */
    public void Suspend() { /* Not Supported */ }
    public void Resume()  { /* Not Supported */ }
    public void Unload()  { /* Not Supported */ }

    public bool CanUnload()  => false;
    public bool CanSuspend() => false;

    /* Automatically called by the mod loader when the mod is about to be unloaded. */
    public Action Disposing { get; }

    public static void Main() { }
}