using Reloaded.Memory.Pointers;

namespace Heroes.Graphics.Essentials.Heroes;

public static unsafe class Variables
{
    public static Pointer<int> ResolutionX { get; set; } = new Pointer<int>(0x00A7793C);
    public static Pointer<int> ResolutionY { get; set; } = new Pointer<int>(0x00A77940);

    public static Pointer<float> MaestroResolutionX { get; set; } = new Pointer<float>(0xAA7140);
    public static Pointer<float> MaestroResolutionY { get; set; } = new Pointer<float>(0xAA7144);
}