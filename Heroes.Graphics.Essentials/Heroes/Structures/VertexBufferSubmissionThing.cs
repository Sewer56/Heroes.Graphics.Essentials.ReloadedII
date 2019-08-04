﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Heroes.Graphics.Essentials.Heroes.Structures
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct VertexBufferSubmissionThing
    {
        [FieldOffset(0x8)]
        public short Width;

        [FieldOffset(0xC)]
        public short Height;

        [FieldOffset(0x1C)]
        public short X;

        [FieldOffset(0x1E)]
        public short Y;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct VertexBufferSubmissionPtr
    {
        [FieldOffset(0x60)]
        public VertexBufferSubmissionThing* SubmissionThing;
    }
}
