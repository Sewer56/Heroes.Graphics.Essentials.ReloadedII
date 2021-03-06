﻿using System.Diagnostics;
using Heroes.Graphics.Essentials.Math;
using Heroes.SDK.API;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
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
        private User32.WinEventProc _onLocationChangeEventHandler;

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
                User32.GetClientRect(Window.WindowHandle, ref rect);
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
