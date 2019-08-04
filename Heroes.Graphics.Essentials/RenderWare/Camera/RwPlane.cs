using System.Numerics;
using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.RenderWare.Camera
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RwPlane
    {
        Vector3 normal;     /* Normal to the plane */
        float distance;     /* Distance to plane from origin in normal direction */
    };
}
