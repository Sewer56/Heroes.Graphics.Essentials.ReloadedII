using System;
using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes.Patches
{
    public unsafe class StageLoadCrashPatch
    {
        public StageLoadCrashPatch()
        {
            Memory.CurrentProcess.SafeWrite((IntPtr)0x61D418, new byte[] {0x90, 0x90});
        }
    }
}
