using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Heroes.Graphics.Essentials.RenderWare.Camera;
using Heroes.Graphics.Essentials.Utility;
using Reloaded.Hooks;
using Reloaded.Hooks.X86;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Heroes
{
    /// <summary>
    /// Hooks the internal RenderWare function used to build perspective clip planes (frustum)
    /// which declares which objects should be rendered on the screen and which should not.
    /// </summary>
    public unsafe class ClippingPlanesHook
    {
        public Config.Config Config { get; private set; }
        public IHook<CameraBuildPerspClipPlanes> BuildClipPlanesHook { get; private set; }

        public ClippingPlanesHook(Config.Config config)
        {
            Config = config;
            BuildClipPlanesHook = new Hook<CameraBuildPerspClipPlanes>(BuildClipPlanesImpl, 0x0064AF80).Activate();
        }

        private int BuildClipPlanesImpl(RwCamera* rwCamera)
        {
            if (rwCamera == (void*) 0)
                return BuildClipPlanesHook.OriginalFunction(rwCamera);

            // Get window client size dimensions.
            var windowHandle = Variables.WindowHandle;
            if (! windowHandle.IsNull)
            {
                // Get current resolution (size of window client area)
                RECT clientSize = new RECT();
                User32_Gdi.GetClientRect(Variables.WindowHandle, ref clientSize);

                float aspectRatio         = AspectConverter.ToAspectRatio(ref clientSize);
                float relativeAspectRatio = AspectConverter.GetRelativeAspect(aspectRatio);

                // Stretch X/Y
                (*rwCamera).StretchViewWindow(aspectRatio, relativeAspectRatio, Config.AspectRatioLimit);

                // Call original.
                int result = BuildClipPlanesHook.OriginalFunction(rwCamera);

                // Reverse Stretch of X/Y
                (*rwCamera).UnStretchViewWindow(aspectRatio, relativeAspectRatio, Config.AspectRatioLimit);
                return result;
            }

            return BuildClipPlanesHook.OriginalFunction(rwCamera);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate int CameraBuildPerspClipPlanes(RwCamera* RwCamera);
    }
}
