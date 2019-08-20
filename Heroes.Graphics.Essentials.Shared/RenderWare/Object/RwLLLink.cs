using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.Shared.RenderWare.Object
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwLLLink
    {
        public RwLLLink* next;
        public RwLLLink* prev;
    }
}
