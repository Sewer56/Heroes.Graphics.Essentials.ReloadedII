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
    public unsafe class AspectRatioHook
    {
        public float AspectRatioLimit { get; set; }
        public IHook<RwCameraSetViewWindow> SetViewWindowHook { get; set; }

        public AspectRatioHook(float aspectRatioLimit, IReloadedHooks hooks)
        {
            AspectRatioLimit  = aspectRatioLimit;
            SetViewWindowHook = hooks.CreateHook<RwCameraSetViewWindow>(SetViewWindowImpl, 0x0064AC80).Activate();
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
                (*rwCamera).UnStretchRecipViewWindow(aspectRatio, relativeAspectRatio, AspectRatioLimit);
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
