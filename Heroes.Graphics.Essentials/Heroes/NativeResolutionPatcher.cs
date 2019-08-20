using Heroes.Graphics.Essentials.Shared.Heroes.Structures;

namespace Heroes.Graphics.Essentials.Heroes
{
    /// <summary>
    /// Utility class which patches all of the hardcoded sets of resolutions to a specific resolution.
    /// </summary>
    public unsafe class NativeResolutionPatcher
    {
        private const int StockResolutionPresetCount    = 8;
        private const int NativeResolutionPointer       = 0x7C9290;

        public static void Patch(int width, int height)
        {
            var settingPointer = (NativeResolutionEntry*) NativeResolutionPointer;
            for (int x = 0; x < StockResolutionPresetCount; x++)
            {
                settingPointer[x].Width  = width;
                settingPointer[x].Height = height;
            }
        }
    }
}
