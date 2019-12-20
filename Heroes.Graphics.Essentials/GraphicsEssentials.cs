﻿using System.Threading.Tasks;
using Heroes.Graphics.Essentials.Config;
using Heroes.Graphics.Essentials.Configuration;
using Heroes.Graphics.Essentials.Heroes;
using Heroes.Graphics.Essentials.Hooks;
using Heroes.Graphics.Essentials.Shared;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials
{
    public class GraphicsEssentials
    {
        private Config.Config _config;
        private Configurator _configurator;

        private DefaultSettingsHook _defaultSettingsHook;
        private StageLoadCrashFixHook _crashFixHook;
        private ClippingPlanesHook _clippingPlanesHook;
        private AspectRatioHook _aspectRatioHook;

        private ResolutionVariablePatcher   _resolutionVariablePatcher;
        private RenderHooks                 _renderHooks;
        private ResizeEventHook             _resizeEventHook;

        public GraphicsEssentials(string modFolder, IReloadedHooks hooks)
        {
            _configurator = new Configurator(modFolder);
            _config = _configurator.GetConfiguration<Config.Config>(0);

            _defaultSettingsHook = new DefaultSettingsHook(_config.DefaultSettings, hooks);

            NativeResolutionPatcher.Patch(_config.Width, _config.Height);
            WindowStylePatcher.Patch(_config.BorderlessWindowed, _config.ResizableWindowed);
  
            if (_config.StupidlyFastLoadTimes)
                LoadTimeHack.Patch();

            if (_config.Disable2PFrameskip)
                DisableFrameskipPatch.Patch();
            
            if (_config.HighAspectRatioCrashFix)
                _crashFixHook = new StageLoadCrashFixHook(hooks);

            _clippingPlanesHook = new ClippingPlanesHook(_config.AspectRatioLimit, hooks);
            _aspectRatioHook    = new AspectRatioHook(_config.AspectRatioLimit, hooks);

            _resolutionVariablePatcher  = new ResolutionVariablePatcher();
            _renderHooks                = new RenderHooks(_config.AspectRatioLimit, hooks);

            Task.Run(MessagePump);
        }

        /* Patching hardcoded values in ResolutionVariablePatcher via events. */
        private void MessagePump()
        {
            _resizeEventHook = new ResizeEventHook();
            _resolutionVariablePatcher.SubscribeToResizeEventHook(_resizeEventHook);
            _renderHooks.SubscribeToResizeEventHook(_resizeEventHook);

            while (User32_Gdi.GetMessage(out var msg, HWND.NULL, 0, 0))
            {
                User32_Gdi.TranslateMessage(msg);
                User32_Gdi.DispatchMessage(msg);
            }
        }
    }
}
