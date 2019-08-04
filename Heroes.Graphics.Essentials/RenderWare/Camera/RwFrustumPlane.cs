﻿using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.RenderWare.Camera
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RwFrustumPlane
    {
        RwPlane plane;
        byte closestX;
        byte closestY;
        byte closestZ;
        byte pad;
    };
}
