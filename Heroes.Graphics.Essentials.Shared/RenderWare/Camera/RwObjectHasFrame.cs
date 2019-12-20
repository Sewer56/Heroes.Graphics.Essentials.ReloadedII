using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Definitions.RenderWare.Object;

namespace Heroes.Graphics.Essentials.Definitions.RenderWare.Camera
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwObjectHasFrame
    {
        public RwObject rwObject;
        public RwLLLink lFrame;
        public void* sync;
    }
}
