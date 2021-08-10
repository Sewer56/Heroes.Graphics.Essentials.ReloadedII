using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Heroes.Graphics.Essentials.Math;
using Heroes.SDK.API;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
{
    public class ResizeEventHook
    {
        private const int EventObjectLocationchange = 0x800B;

        public event WindowResized Resized = (in ResizeEventHookData sender) => { }; 

        // Cached
        public ResizeEventHookData Data;

        // Ours
        private User32.WinEventProc _onLocationChangeEventHandler;

        public ResizeEventHook()
        {
            if (!Environment.IsWine)
            {
                _onLocationChangeEventHandler = OnLocationChangeImpl;
                User32.SetWinEventHook(EventObjectLocationchange, EventObjectLocationchange, HINSTANCE.NULL, _onLocationChangeEventHandler, (uint)Process.GetCurrentProcess().Id, 0, User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
            }
            else
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        HandlePossibleSizeChange();
                        await Task.Delay(1000);
                    }
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Forces a size change check.
        /// </summary>
        public void ForceSizeChangeCheck() => HandlePossibleSizeChange();

        /* Patching Resolution Changes Section */
        private void OnLocationChangeImpl(User32.HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hWnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            if (idObject != 0 || idChild != 0 || winEvent != EventObjectLocationchange)
                return;
            
            HandlePossibleSizeChange();
        }

        private void HandlePossibleSizeChange()
        {
            var data = GetResolutionData();
            if (data.GetHashCode() == Data.GetHashCode()) 
                return;

            this.Data = data;
            Resized?.Invoke(this.Data);
        }

        private ResizeEventHookData GetResolutionData()
        {
            // Two things performed here.
            RECT rect = new RECT();
            User32.GetClientRect(Window.WindowHandle, ref rect);

            var data = new ResizeEventHookData();
            data.CurrentHeight = rect.Height;
            data.CurrentWidth = rect.Width;
            data.ActualAspectRatio = data.CurrentWidth / (float)data.CurrentHeight;
            data.RelativeAspectRatio = AspectConverter.GetRelativeAspect(data.ActualAspectRatio);

            return data;
        }

        public delegate void WindowResized(in ResizeEventHookData sender);

        /// <summary>
        /// All data related to the resize event.
        /// </summary>
        public struct ResizeEventHookData
        {
            public int CurrentHeight;
            public int CurrentWidth;
            public float ActualAspectRatio;
            public float RelativeAspectRatio;

            // Autogenerated by R#
            public bool Equals(ResizeEventHookData other) => CurrentHeight == other.CurrentHeight && CurrentWidth == other.CurrentWidth && ActualAspectRatio.Equals(other.ActualAspectRatio) && RelativeAspectRatio.Equals(other.RelativeAspectRatio);
            public override bool Equals(object obj) => obj is ResizeEventHookData other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(CurrentHeight, CurrentWidth, ActualAspectRatio, RelativeAspectRatio);
        }
    }
}
