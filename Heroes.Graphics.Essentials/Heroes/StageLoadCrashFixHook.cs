using System;
using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Utility;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Vanara.PInvoke;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;

namespace Heroes.Graphics.Essentials.Heroes
{
    public class StageLoadCrashFixHook
    {
        private IHook<TObjCamera_Init> _cameraInitHook;

        public StageLoadCrashFixHook()
        {
            _cameraInitHook = Program.ReloadedHooks.CreateHook<TObjCamera_Init>(TObjCameraInit, 0x0061D3B0).Activate();
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
        [Function(Register.eax, Register.eax, StackCleanup.Callee)]
        private delegate int TObjCamera_Init(IntPtr thisPointer, int camLimit);
    }
}
