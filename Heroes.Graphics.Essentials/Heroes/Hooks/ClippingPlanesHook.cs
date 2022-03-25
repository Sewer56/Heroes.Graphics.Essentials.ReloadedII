using System;
using Heroes.Graphics.Essentials.Math;
using Heroes.SDK.API;
using Heroes.SDK.Definitions.Structures.RenderWare.Camera;
using Reloaded.Hooks.Definitions;
using Vanara.PInvoke;
using static Heroes.SDK.Classes.PseudoNativeClasses.RenderWareFunctions;

namespace Heroes.Graphics.Essentials.Heroes.Hooks;

/// <summary>
/// Hooks the internal RenderWare function used to build perspective clip planes (frustum)
/// which declares which objects should be rendered on the screen and which should not.
/// </summary>
public unsafe class ClippingPlanesHook
{
    public float AspectRatioLimit { get; set; }
    public IHook<Native_CameraBuildPerspClipPlanes> BuildClipPlanesHook { get; private set; }

    public ClippingPlanesHook(float aspectLimit)
    {
        AspectRatioLimit = aspectLimit;
        BuildClipPlanesHook = Fun_CameraBuildPerspClipPlanes.Hook(BuildClipPlanesImpl).Activate();
    }

    private int BuildClipPlanesImpl(RwCamera* rwCamera)
    {
        if (rwCamera == (void*) 0)
            return BuildClipPlanesHook.OriginalFunction(rwCamera);

        // Get window client size dimensions.
        var windowHandle = Window.WindowHandle;
        if (windowHandle != IntPtr.Zero)
        {
            // Get current resolution (size of window client area)
            RECT clientSize = new RECT();
            User32.GetClientRect(Window.WindowHandle, ref clientSize);

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
}