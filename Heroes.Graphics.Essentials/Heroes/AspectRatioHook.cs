using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.RenderWare.Camera;
using Heroes.Graphics.Essentials.Utility;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Heroes
{
    public unsafe class AspectRatioHook
    {
        public Config.Config Config { get; set; }
        public IHook<RwCameraSetViewWindow> SetViewWindowHook { get; set; }

        public AspectRatioHook(Config.Config config)
        {
            Config = config;
            SetViewWindowHook = Program.ReloadedHooks.CreateHook<RwCameraSetViewWindow>(SetViewWindowImpl, 0x0064AC80).Activate();
        }

        private void SetViewWindowImpl(RwCamera* rwCamera, RwView* view)
        {
            SetViewWindowHook.OriginalFunction(rwCamera, view);

            var windowHandle = Variables.WindowHandle;
            if (!windowHandle.IsNull)
            {
                // Get current resolution (size of window client area)
                RECT clientSize = new RECT();
                User32_Gdi.GetClientRect(Variables.WindowHandle, ref clientSize);

                float aspectRatio           = AspectConverter.ToAspectRatio(ref clientSize);
                float relativeAspectRatio   = AspectConverter.GetRelativeAspect(aspectRatio);

                // Unstretch X/Y
                (*rwCamera).UnStretchRecipViewWindow(aspectRatio, relativeAspectRatio, Config.AspectRatioLimit);
            }
        }

        /// <summary>
        /// Sets the aspect ratio of the current screen view.
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate void RwCameraSetViewWindow(RwCamera* rwCamera, RwView* view);
    }
}
