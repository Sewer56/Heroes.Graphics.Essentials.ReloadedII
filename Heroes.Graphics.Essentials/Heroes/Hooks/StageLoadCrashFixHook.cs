using System;
using Heroes.Graphics.Essentials.Math;
using Heroes.SDK.API;
using Heroes.SDK.Classes.NativeClasses;
using Reloaded.Hooks.Definitions;
using Vanara.PInvoke;
using static Heroes.SDK.Classes.NativeClasses.TObjCamera;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
{
    public unsafe class StageLoadCrashFixHook
    {
        private IHook<Native_Init> _cameraInitHook;

        public StageLoadCrashFixHook()
        {
            _cameraInitHook = Fun_Init.Hook(TObjCameraInit).Activate();
        }

        private int TObjCameraInit(TObjCamera* thisPointer, int camLimit)
        {
            // Backup old resolution.
            var windowHandle = Window.WindowHandle;
            if (windowHandle != IntPtr.Zero)
            {
                User32.GetWindowRect(windowHandle, out var windowRect);
                int resolutionXBackup = windowRect.Width;
                int resolutionYBackup = windowRect.Height;

                // Temp resize window, execute and restore
                User32.MoveWindow(windowHandle, windowRect.left, windowRect.top, (int) AspectConverter.GameCanvasWidth, (int) AspectConverter.GameCanvasHeight, false);
                int result = _cameraInitHook.OriginalFunction(thisPointer, camLimit);
                User32.MoveWindow(windowHandle, windowRect.left, windowRect.top, resolutionXBackup, resolutionYBackup, false);

                return result;
            }
            
            return _cameraInitHook.OriginalFunction(thisPointer, camLimit);
        }
    }
}
