using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.Shared.Heroes.Structures
{
    // TODO: Snoop around RW SDK and find real names here.
    public unsafe struct RwEngineInstance
    {
        public RwEngine* Engine;

        public struct RwEngine
        {
            public ScreenRenderRelated* ScreenRender;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ScreenRenderRelated
        {
            [FieldOffset(0x60)]
            public Viewport* ScreenViewport;

            [FieldOffset(0x64)]
            public AllViewPorts* AllViewPorts;

            [FieldOffset(0x70)]
            public float MenuYScale;

            [FieldOffset(0x74)]
            public float MenuXScale;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct AllViewPorts
        {
            /* This is technically a possibly endless array of viewports. The offsets are for convenience only. */

            /* Hidden Viewport? */
            [FieldOffset(0x0)]
            public Viewport UnknownViewPort; // ?

            [FieldOffset(0x5C)]
            public Viewport P1Viewport;  // Used by player one

            [FieldOffset(0xB8)]
            public Viewport P1UnknownViewport;  // Unused

            [FieldOffset(0x114)]
            public Viewport P2Viewport; // Used by player two

            [FieldOffset(0x170)]
            public Viewport P2UnknownViewport; // Unused

            [FieldOffset(0x1CC)]
            public Viewport P3Viewport; // Used by player three

            [FieldOffset(0x228)]
            public Viewport P3UnknownViewport; // Unused

            [FieldOffset(0x284)]
            public Viewport P4Viewport; // Used by player four

            [FieldOffset(0x2E0)]
            public Viewport P4UnknownViewport; // Unused

        }

        [StructLayout(LayoutKind.Explicit, Size = 0x5C)]
        public struct Viewport
        {
            [FieldOffset(0x0)]
            public Viewport* Parent;

            [FieldOffset(0x4)]
            public int unk_4;

            [FieldOffset(0x8)]
            public int unk_8;

            [FieldOffset(0xC)]
            public int Width; // Used by player one

            [FieldOffset(0x10)]
            public int Height; // Used by player one

            [FieldOffset(0x14)]
            public int Unknown; // Default: 32

            [FieldOffset(0x18)]
            public int Unknown2; // Default: 32

            [FieldOffset(0x1C)]
            public short OffsetX;  // X offset of viewport relative to screen.

            [FieldOffset(0x1E)]
            public short OffsetY;  // Y offset of viewport relative to screen.
        }
    }

    
}
