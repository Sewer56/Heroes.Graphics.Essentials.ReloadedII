using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes.Patches;

public class DontSlowdownOnFocusLoss
{
    public static void Patch()
    {
        // Fun fact: I forgot how this works.
        Memory.CurrentProcess.SafeWriteRaw(0x004466AA, new byte[] { 0xEB, 0x2C });
    }
}