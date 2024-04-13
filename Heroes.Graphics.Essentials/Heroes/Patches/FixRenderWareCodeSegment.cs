using Reloaded.Memory.Sources;
using static Reloaded.Memory.Kernel32.Kernel32.MEM_PROTECTION;

namespace Heroes.Graphics.Essentials.Heroes.Patches;

/// <summary>
///     Due to unknown reasons, the RenderWare Code Segment is not marked as executable in all versions of the game.
///     This causes a crash when Data Execution Prevention (DEP) is used.
///
///     This patch fixes the segment for the US release.
/// </summary>
public class FixRenderWareCodeSegment
{
    public static void Patch()
    {
        Memory.CurrentProcess.ChangePermission(0xBCD000, 0x1000, PAGE_EXECUTE_READWRITE);
    }
}