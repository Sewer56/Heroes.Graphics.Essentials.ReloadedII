using System.Numerics;
using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.Definitions.RenderWare.Camera
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RwMatrixTag
    {
        Vector3 right;
        uint flags;

        Vector3 up;
        uint pad1;

        Vector3 at;
        uint pad2;

        Vector3 pos;
        uint pad3;
    };
}
