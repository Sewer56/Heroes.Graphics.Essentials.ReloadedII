using System;
using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Definitions.Heroes;
using Heroes.Graphics.Essentials.Definitions.Math;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Vanara.PInvoke;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
{
    public class StageLoadCrashFixHook
    {
        private IHook<TObjCamera_Init> _cameraInitHook;

        public StageLoadCrashFixHook(IReloadedHooks hooks)
        {
            _cameraInitHook = hooks.CreateHook<TObjCamera_Init>(TObjCameraInit, 0x0061D3B0).Activate();
        }

        private int TObjCameraInit(IntPtr thisPointer, int camLimit)
        {
            // Backup old resolution.
            var windowHandle = Variables.WindowHandle;
            if (! windowHandle.IsNull)
            {
                User32_Gdi.GetWindowRect(windowHandle, out var windowRect);
                int resolutionXBackup = windowRect.Width;
                int resolutionYBackup = windowRect.Height;

                // Get new resolution
                int greaterResolution = resolutionXBackup > resolutionYBackup ? resolutionXBackup : resolutionYBackup;
                AspectConverter.WidthToResolution(greaterResolution, AspectConverter.OriginalGameAspect, out var resolution);

                // Temp resize window and execute.
                User32_Gdi.MoveWindow(windowHandle, windowRect.left, windowRect.top, resolution.Width, resolution.Height, false);

                int result = _cameraInitHook.OriginalFunction(thisPointer, camLimit);

                // Restore window.
                User32_Gdi.MoveWindow(windowHandle, windowRect.left, windowRect.top, resolutionXBackup, resolutionYBackup, false);

                return result;
            }
            
            return _cameraInitHook.OriginalFunction(thisPointer, camLimit);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [Function(FunctionAttribute.Register.eax, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee)]
        private delegate int TObjCamera_Init(IntPtr thisPointer, int camLimit);
    }
}
