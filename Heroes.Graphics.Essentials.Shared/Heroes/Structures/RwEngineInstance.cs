using System.Runtime.InteropServices;

namespace Heroes.Graphics.Essentials.Shared.Heroes.Structures
{
    // TODO: Snoop around RW SDK and find real names here.
    public unsafe struct RwEngineInstance
    {
        public RwEngine* Engine;

        public struct RwEngine
        {
            public MultiplayerRenderRelated* MultiplayerRender;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MultiplayerRenderRelated
        {
            [FieldOffset(0x60)]
            public MultiplayerCameraController* MultiplayerCameraController;

            [FieldOffset(0x70)]
            public float MenuYScale;

            [FieldOffset(0x74)]
            public float MenuXScale;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MultiplayerCameraController
        {
            [FieldOffset(0xC)]
            public int ScreenWidth;

            [FieldOffset(0x10)]
            public int ScreenHeight;

            [FieldOffset(0x28)]
            public int UnknownDefaultScreenWidth; // Defaults 640

            [FieldOffset(0x2C)]
            public int UnknownDefaultScreenHeight; // Defaults 480

            [FieldOffset(0x68)]
            public int UnknownScreenWidth;  // Same as width of screen, unknown effect.

            [FieldOffset(0x6C)]
            public int UnknownScreenHeight; // Same as width of screen, unknown effect.

            [FieldOffset(0xC4)]
            public PlayerViewport PlayerOneViewport; // Used by player one

            [FieldOffset(0x120)]
            public PlayerViewport PlayerTwoViewport; // Unused

            [FieldOffset(0x17C)]
            public PlayerViewport PlayerThreeViewport; // Used by player two

            [FieldOffset(0x1D8)]
            public PlayerViewport PlayerFourViewport; // Unused
        }


        [StructLayout(LayoutKind.Explicit, Size = 0x5C)]
        public struct PlayerViewport
        {
            [FieldOffset(0x0)]
            public int PlayerOneViewPortWidth; // Used by player one

            [FieldOffset(0x4)]
            public int PlayerOneViewPortHeight; // Used by player one

            [FieldOffset(0x8)]
            public int Unknown; // Default: 32

            [FieldOffset(0xC)]
            public int Unknown2; // Default: 32

            [FieldOffset(0x10)]
            public int OffsetX;  // X offset of viewport relative to screen.
        }
    }
}
