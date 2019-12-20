using System.Numerics;

namespace Heroes.Graphics.Essentials.Definitions.RenderWare.Camera
{
    /// <summary>
    /// This type represents a 3D axis-aligned bounding-box
    /// specified by the positions of two corners which lie on a diagonal.
    /// Typically used to specify a world bounding-box when the world is created
    /// </summary>
    public struct RwBBox
    {
        /* Must be in this order */
        Vector3 sup;   /* Supremum vertex. (contains largest values)  */
        Vector3 inf;   /* Infimum vertex.  (contains smallest values) */
    };
}
