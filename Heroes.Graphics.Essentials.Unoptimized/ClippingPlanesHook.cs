using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Shared.Heroes;
using Heroes.Graphics.Essentials.Shared.Math;
using Heroes.Graphics.Essentials.Shared.RenderWare.Camera;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Hooks
{
    /// <summary>
    /// Hooks the internal RenderWare function used to build perspective clip planes (frustum)
    /// which declares which objects should be rendered on the screen and which should not.
    /// </summary>
    public unsafe class ClippingPlanesHook
    {
        public float AspectRatioLimit                                   { get; private set; }
        public IHook<CameraBuildPerspClipPlanes> BuildClipPlanesHook    { get; private set; }

        public ClippingPlanesHook(float aspectLimit, IReloadedHooks hooks)
        {
            AspectRatioLimit = aspectLimit;
            BuildClipPlanesHook = hooks.CreateHook<CameraBuildPerspClipPlanes>(BuildClipPlanesImpl, 0x0064AF80).Activate();
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
                (*rwCamera).StretchViewWindow(aspectRatio, relativeAspectRatio, AspectRatioLimit);

                // Call original.
                int result = BuildClipPlanesHook.OriginalFunction(rwCamera);

                // Reverse Stretch of X/Y
                (*rwCamera).UnStretchViewWindow(aspectRatio, relativeAspectRatio, AspectRatioLimit);
                return result;
            }

            return BuildClipPlanesHook.OriginalFunction(rwCamera);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate int CameraBuildPerspClipPlanes(RwCamera* RwCamera);
    }
}
