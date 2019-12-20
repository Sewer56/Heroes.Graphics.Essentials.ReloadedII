using System;

namespace Heroes.Graphics.Essentials.Definitions.RenderWare.Camera.Enum
{
    public enum RwCameraProjection : int
    {
        rwNACAMERAPROJECTION = 0,   /* Invalid projection */
        rwPERSPECTIVE = 1,          /* Perspective projection */
        rwPARALLEL = 2,             /* Parallel projection */
        rwCAMERAPROJECTIONFORCEENUMSIZEINT = Int32.MaxValue
    };
}
