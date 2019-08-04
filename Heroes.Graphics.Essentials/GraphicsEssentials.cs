using System;
using System.Threading;
using System.Threading.Tasks;
using Heroes.Graphics.Essentials.Config;
using Heroes.Graphics.Essentials.Heroes;
using Heroes.Graphics.Essentials.Utility;
using Reloaded.Memory.Sources;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials
{
    public class GraphicsEssentials
    {
        private Config.Config _config;
        private DefaultSettingsHook _defaultSettingsHook;
        private StageLoadCrashFixHook _crashFixHook;
        private ClippingPlanesHook _clippingPlanesHook;
        private AspectRatioHook _aspectRatioHook;
        private ResolutionVariablePatcher _resolutionVariablePatcher;

        public GraphicsEssentials(string modFolder)
        {
            _config = new ConfigReadWriter(modFolder).FromJson();
            _defaultSettingsHook = new DefaultSettingsHook(_config.DefaultSettings);

            NativeResolutionPatcher.Patch(_config.Width, _config.Height);
            WindowStylePatcher.Patch(_config.BorderlessWindowed, _config.ResizableWindowed);
  
            if (_config.StupidlyFastLoadTimes)
                LoadTimeHack.Patch();

            if (_config.Disable2PFrameskip)
                DisableFrameskipPatch.Patch();
            
            if (_config.HighAspectRatioCrashFix)
                _crashFixHook = new StageLoadCrashFixHook();

            _clippingPlanesHook = new ClippingPlanesHook(_config);
            _aspectRatioHook = new AspectRatioHook(_config);

            Task.Run(MessagePump);
        }

        /* Patching hardcoded values in ResolutionVariablePatcher via events. */
        private void MessagePump()
        {
            _resolutionVariablePatcher = new ResolutionVariablePatcher(_config);
            _resolutionVariablePatcher.Patch(_config.Width, _config.Height);

            while (User32_Gdi.GetMessage(out var msg, HWND.NULL, 0, 0))
            {
                User32_Gdi.TranslateMessage(msg);
                User32_Gdi.DispatchMessage(msg);
            }
        }
    }
}
