﻿using Heroes.Graphics.Essentials.Configuration;
using Heroes.Graphics.Essentials.Heroes.Hooks;
using Heroes.Graphics.Essentials.Heroes.Patches;
using Heroes.SDK.API;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials;

public class GraphicsEssentials
{
    private Config.Config _config;
    private Configurator _configurator;

    private DefaultSettingsHook _defaultSettingsHook;
    private StageLoadCrashPatch _crashPatch;
    private ClippingPlanesHook _clippingPlanesHook;
    private AspectRatioHook _aspectRatioHook;

    private ResolutionVariablePatcher   _resolutionVariablePatcher;
    private RenderHooks                 _renderHooks;
    private ResizeEventHook             _resizeEventHook;

    public GraphicsEssentials(string modFolder, string configDirectory, IReloadedHooks hooks)
    {
        _configurator = new Configurator(configDirectory);
        _configurator.Migrate(modFolder, configDirectory);

        _config = _configurator.GetConfiguration<Config.Config>(0);
        _defaultSettingsHook = new DefaultSettingsHook(_config.DefaultSettings);

        NativeResolutionPatcher.Patch(_config.Width, _config.Height);
        WindowStylePatcher.Patch(_config.BorderlessWindowed, _config.ResizableWindowed);
  
        if (_config.StupidlyFastLoadTimes)
            LoadTimeHack.Patch();

        if (_config.Disable2PFrameskip)
            DisableFrameskipPatch.Patch();
            
        if (_config.HighAspectRatioCrashFix)
            _crashPatch = new StageLoadCrashPatch();

        if (_config.NoSubtitles)
            DisableSubtitlesPatch.Patch();
        
        if (_config.DontSlowdownOnFocusLost)
            DontSlowdownOnFocusLoss.Patch();

        _clippingPlanesHook = new ClippingPlanesHook(_config.AspectRatioLimit);
        _aspectRatioHook    = new AspectRatioHook(_config.AspectRatioLimit);

        _resolutionVariablePatcher  = new ResolutionVariablePatcher();
        _renderHooks                = new RenderHooks(_config.AspectRatioLimit, hooks);

        FixRenderWareCodeSegment.Patch();
        Task.Run(MessagePump);
        Task.Run(async () =>
        {
            while (Window.WindowHandle == IntPtr.Zero)
                await Task.Delay(32);

            int left   = 0;
            int top    = 0;

            if (_config.CenterWindow)
            {
                var monitor = User32.MonitorFromWindow(Window.WindowHandle, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST);
                var info = new User32.MONITORINFO { cbSize = (uint) Struct.GetSize<User32.MONITORINFO>() };
                if (User32.GetMonitorInfo(monitor, ref info))
                {
                    left += (info.rcMonitor.Width - _config.Width) / 2;
                    top += (info.rcMonitor.Height - _config.Height) / 2;
                }
            }

            // Force a resize event, with proper window resize, just in case.
            User32.MoveWindow(Window.WindowHandle, left, top, _config.Width - 1, _config.Height - 1, false);
            User32.MoveWindow(Window.WindowHandle, left, top, _config.Width, _config.Height, false);

            await Task.Delay(32);
            _resizeEventHook.ForceSizeChangeCheck();
        }).ConfigureAwait(false);
    }

    /* Patching hardcoded values in ResolutionVariablePatcher via events. */
    private void MessagePump()
    {
        _resizeEventHook = new ResizeEventHook();
        _resolutionVariablePatcher.SubscribeToResizeEventHook(_resizeEventHook);
        _renderHooks.SubscribeToResizeEventHook(_resizeEventHook);

        while (User32.GetMessage(out var msg, HWND.NULL, 0, 0))
        {
            User32.TranslateMessage(msg);
            User32.DispatchMessage(msg);
        }
    }
}