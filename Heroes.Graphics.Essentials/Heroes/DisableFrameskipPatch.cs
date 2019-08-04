using System;
using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes
{
    public static class DisableFrameskipPatch
    {
        public static void Patch()
        {
            Memory.CurrentProcess.SafeWrite((IntPtr)0x402D07, new byte[] { 0x90 });
        }
    }
}
