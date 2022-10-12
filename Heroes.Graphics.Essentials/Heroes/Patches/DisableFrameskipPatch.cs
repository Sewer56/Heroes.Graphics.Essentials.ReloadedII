using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes.Patches;

public static class DisableFrameskipPatch
{
    public static void Patch()
    {
        Memory.CurrentProcess.SafeWriteRaw(0x402D07, new byte[] { 0x90 });
    }
}