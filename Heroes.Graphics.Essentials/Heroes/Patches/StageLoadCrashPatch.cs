using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes.Patches;

public class StageLoadCrashPatch
{
    public StageLoadCrashPatch()
    {
        Memory.CurrentProcess.SafeWriteRaw(0x61D418, new byte[] {0x90, 0x90});
    }
}