using Heroes.SDK.Definitions.Structures.Graphics.Device;

namespace Heroes.Graphics.Essentials.Heroes.Patches;

/// <summary>
/// Utility class which patches all of the hardcoded sets of resolutions to a specific resolution.
/// </summary>
public class NativeResolutionPatcher
{
    public static void Patch(int width, int height)
    {
        var entries = NativeResolutionEntry.GetEntries();
        for (int x = 0; x < entries.Count; x++)
        {
            entries[x].Width  = width;
            entries[x].Height = height;
        }
    }
}