using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes.Patches;

public static class DisableSubtitlesPatch
{
    public static void Patch()
    {
        // Fun fact: I forgot how this works.
        Memory.CurrentProcess.SafeWriteRaw(0x428560, new byte[] { 0xC2, 0x1C, 0x00 });
    }
}