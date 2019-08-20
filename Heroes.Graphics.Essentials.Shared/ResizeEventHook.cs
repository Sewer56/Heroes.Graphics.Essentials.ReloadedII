using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Heroes.Graphics.Essentials.Shared.Heroes;
using Heroes.Graphics.Essentials.Shared.Math;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Shared
{
    public class ResizeEventHook
    {
        private const int EventObjectLocationchange = 0x800B;

        public event WindowResized Resized = sender => { }; 

        // Cached
        public int CurrentHeight;
        public int CurrentWidth;
        public float ActualAspectRatio;
        public float RelativeAspectRatio;

        // Ours
        private User32.WINEVENTPROC _onLocationChangeEventHandler;

        public ResizeEventHook()
        {
            _onLocationChangeEventHandler = OnLocationChange;
            User32.SetWinEventHook(EventObjectLocationchange, EventObjectLocationchange, HINSTANCE.NULL, _onLocationChangeEventHandler, (uint)Process.GetCurrentProcess().Id, 0, User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
        }

        /* Patching Resolution Changes Section */
        private void OnLocationChange(User32.HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hWnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            if (idObject != 0 || idChild != 0)
                return;

            if (winEvent == EventObjectLocationchange)
            {
                // Two things performed here.
                RECT rect = new RECT();
                User32_Gdi.GetClientRect(Variables.WindowHandle, ref rect);
                CurrentHeight = rect.Height;
                CurrentWidth = rect.Width;
                ActualAspectRatio = CurrentWidth / (float)CurrentHeight;
                RelativeAspectRatio = AspectConverter.GetRelativeAspect(ActualAspectRatio);

                Resized?.Invoke(this);
            }
        }

        public delegate void WindowResized(ResizeEventHook sender);
    }
}
