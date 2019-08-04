using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.RenderWare.Object
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwObject
    {
        public byte type;
        public byte subType;
        public byte flags;
        public byte privateFlags;
        public void* parent;
    }
}
