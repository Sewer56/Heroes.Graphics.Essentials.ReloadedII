using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.RenderWare.Object;

namespace Heroes.Graphics.Essentials.RenderWare.Camera
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwObjectHasFrame
    {
        public RwObject rwObject;
        public RwLLLink lFrame;
        public void* sync;
    }
}
