using Reloaded.Memory.Pointers;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Definitions.Heroes
{
    public static class Variables
    {
        private const string WindowClass = "SonicHeroesPC-RW3.5";
        private static HWND _cachedWindowHandle;

        public static Pointer<int> ResolutionX { get; set; } = new Pointer<int>(0x00A7793C);
        public static Pointer<int> ResolutionY { get; set; } = new Pointer<int>(0x00A77940);

        public static Pointer<float> MaestroResolutionX { get; set; } = new Pointer<float>(0xAA7140);
        public static Pointer<float> MaestroResolutionY { get; set; } = new Pointer<float>(0xAA7144);

        public static HWND WindowHandle
        {
            get
            {
                if (User32_Gdi.IsWindow(_cachedWindowHandle))
                    return _cachedWindowHandle;
                
                _cachedWindowHandle = User32_Gdi.FindWindow(WindowClass, null);
                return _cachedWindowHandle;
            }
        }
    }
}
