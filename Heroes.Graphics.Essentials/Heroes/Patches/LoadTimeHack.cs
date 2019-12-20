using System;
using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Heroes.Patches
{
    public static class LoadTimeHack
    {
        public static void Patch()
        {
            // Fun fact: I forgot how this works.
            Memory.CurrentProcess.SafeWrite((IntPtr) 0x0078A578, (double) 9999999999F);
        }
    }
}
