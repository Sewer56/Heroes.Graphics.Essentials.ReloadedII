using System.Numerics;
using Heroes.Graphics.Essentials.RenderWare.Camera.Enum;

namespace Heroes.Graphics.Essentials.RenderWare.Camera
{
    public unsafe struct RwCamera
    {
        public RwObjectHasFrame objectHasFrame;

        /* Parallel or perspective projection */
        public RwCameraProjection projectionType;

        /* Start/end update functions */
        public void* beginUpdate;  // RwCameraBeginUpdateFunc
        public void* endUpdate;    // RwCameraEndUpdateFunc

        /* The view matrix */
        public RwMatrixTag viewMatrix;

        /* The cameras image buffer */
        public void* frameBuffer;

        /* The Z buffer */
        public void* zBuffer;

        /* Camera's mathmatical characteristics */
        public Vector2 viewWindow;
        public Vector2 recipViewWindow;
        public Vector2 viewOffset;
        public float nearPlane;
        public float farPlane;
        public float fogPlane;

        /* Transformation to turn camera z or 1/z into a Z buffer z */
        public float zScale;
        public float zShift;

        /* The clip-planes making up the viewing frustum */
        public RwFrustumPlane frustumPlane1;
        public RwFrustumPlane frustumPlane2;
        public RwFrustumPlane frustumPlane3;
        public RwFrustumPlane frustumPlane4;
        public RwFrustumPlane frustumPlane5;
        public RwFrustumPlane frustumPlane6;
        public RwBBox frustumBoundBox;

        /* Points on the tips of the view frustum */
        public Vector3 frustumCorner1;
        public Vector3 frustumCorner2;
        public Vector3 frustumCorner3;
        public Vector3 frustumCorner4;
        public Vector3 frustumCorner5;
        public Vector3 frustumCorner6;
        public Vector3 frustumCorner7;
        public Vector3 frustumCorner8;

        /* Custom functionality */

        /// <summary>
        /// Stretches the view window.
        /// </summary>
        /// <param name="actualAspect">The actual aspect ratio of the window.</param>
        /// <param name="relativeAspectRatio">The relative aspect compared to the game's intended aspect.</param>
        /// <param name="aspectLimit">Stretch X (width) if above this, else stretch Y.</param>
        public void StretchViewWindow(float actualAspect, float relativeAspectRatio, float aspectLimit)
        {
            if (actualAspect > aspectLimit)
                viewWindow.X = viewWindow.X * relativeAspectRatio; // Hor+
            else
                viewWindow.X = viewWindow.X * relativeAspectRatio / (actualAspect / aspectLimit); // Preserve Horizontal for vertical expansion
                viewWindow.Y = viewWindow.Y / (actualAspect / aspectLimit); // Vert+
        }

        /// <summary>
        /// Stretches the recipient view window.
        /// What RenderWare (presumably) assumes as the view window that is visible by the user.
        /// </summary>
        /// <param name="actualAspect">The actual aspect ratio of the window.</param>
        /// <param name="relativeAspectRatio">The relative aspect compared to the game's intended aspect.</param>
        /// <param name="aspectLimit">Stretch X (width) if above this, else stretch Y.</param>
        public void StretchRecipViewWindow(float actualAspect, float relativeAspectRatio, float aspectLimit)
        {
            if (actualAspect > aspectLimit)
                recipViewWindow.X = recipViewWindow.X * relativeAspectRatio; // Hor+
            else
                recipViewWindow.X = recipViewWindow.X * relativeAspectRatio / (actualAspect / aspectLimit); // Preserve Horizontal for vertical expansion
                recipViewWindow.Y = recipViewWindow.Y / (actualAspect / aspectLimit); // Vert+
        }

        /// <summary>
        /// Unstretches the view window.
        /// </summary>
        /// <param name="actualAspect">The actual aspect ratio of the window.</param>
        /// <param name="relativeAspectRatio">The relative aspect compared to the game's intended aspect.</param>
        /// <param name="aspectLimit">Unstretch X (width) if above this, else unstretch Y.</param>
        public void UnStretchViewWindow(float actualAspect, float relativeAspectRatio, float aspectLimit)
        {
            StretchViewWindow(actualAspect, 1 / relativeAspectRatio, aspectLimit);
        }

        /// <summary>
        /// Unstretches the recipient view window.
        /// What RenderWare (presumably) assumes as the view window that is visible by the user.
        /// </summary>
        /// <param name="actualAspect">The actual aspect ratio of the window.</param>
        /// <param name="relativeAspectRatio">The relative aspect compared to the game's intended aspect.</param>
        /// <param name="aspectLimit">Unstretch X (width) if above this, else unstretch Y.</param>
        public void UnStretchRecipViewWindow(float actualAspect, float relativeAspectRatio, float aspectLimit)
        {
            StretchRecipViewWindow(actualAspect, 1 / relativeAspectRatio, aspectLimit);
        }
    }
}
