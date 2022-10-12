using Heroes.Graphics.Essentials.Math;
using Heroes.SDK.API;
using Heroes.SDK.Definitions.Structures.RenderWare.Camera;
using Reloaded.Hooks.Definitions;
using Vanara.PInvoke;
using static Heroes.SDK.Classes.PseudoNativeClasses.RenderWareFunctions;

namespace Heroes.Graphics.Essentials.Heroes.Hooks;

public unsafe class AspectRatioHook
{
    public float AspectRatioLimit { get; set; }
    public IHook<Native_RwCameraSetViewWindow> SetViewWindowHook { get; private set; }

    public AspectRatioHook(float aspectRatioLimit)
    {
        AspectRatioLimit  = aspectRatioLimit;
        SetViewWindowHook = Fun_RwCameraSetViewWindow.Hook(SetViewWindowImpl).Activate();
    }

    private void SetViewWindowImpl(RwCamera* rwCamera, RwView* view)
    {
        SetViewWindowHook.OriginalFunction(rwCamera, view);

        var windowHandle = Window.WindowHandle;
        if (windowHandle != IntPtr.Zero)
        {
            // Get current resolution (size of window client area)
            User32.GetClientRect(Window.WindowHandle, out RECT clientSize);

            float aspectRatio           = AspectConverter.ToAspectRatio(ref clientSize);
            float relativeAspectRatio   = AspectConverter.GetRelativeAspect(aspectRatio);

            // Unstretch X/Y
            (*rwCamera).UnStretchRecipViewWindow(aspectRatio, relativeAspectRatio, AspectRatioLimit);
        }
    }
}