using System;
using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes
{
    public unsafe class WindowStylePatcher
    {
        #region Native Constants
        private const uint WS_BORDER = 0x800000;
        private const uint WS_CAPTION = 0xC00000;
        private const uint WS_MINIMIZEBOX = 0x20000;
        private const uint WS_MAXIMIZEBOX = 0x10000;
        private const uint WS_SIZEBOX = 0x40000;
        #endregion

        private const int StockWindowStyle = 0x00C80000;

        // Pointers to hardcoded/embedded window styles for application.
        private static readonly int* _windowStyleA = (int*)0x00446D88;
        private static readonly int* _windowStyleB = (int*)0x00446DBE;

        public static void Patch(bool borderless, bool resizable)
        {
            uint stockStyle = StockWindowStyle;
            if (borderless)
                SetBorderless(ref stockStyle);

            if (resizable)
                SetResizable(ref stockStyle);

            Memory.CurrentProcess.SafeWrite((IntPtr) _windowStyleA, ref stockStyle);
            Memory.CurrentProcess.SafeWrite((IntPtr) _windowStyleB, ref stockStyle);
        }

        private static void SetBorderless(ref uint currentWindowStyle)
        {
            currentWindowStyle &= ~WS_BORDER;
            currentWindowStyle &= ~WS_CAPTION;
            currentWindowStyle &= ~WS_MAXIMIZEBOX;
            currentWindowStyle &= ~WS_MINIMIZEBOX;
        }

        private static void SetResizable(ref uint currentWindowStyle)
        {
            currentWindowStyle |= WS_SIZEBOX;
        }
    }
}
