using Heroes.Graphics.Essentials.Utility.Structs;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Utility
{
    public class AspectConverter
    {
        /// <summary>
        /// Original aspect ratio of the game.
        /// </summary>
        public const float OriginalGameAspect = 4F / 3F;

        /// <summary>
        /// Converts a fixed width and aspect ratio to a width-height resolution pair.
        /// </summary>
        /// <param name="width">The width of the resolution to produce.</param>
        /// <param name="aspectRatio">The aspect ratio of the resolution to produce. e.g. 4F/3F.</param>
        /// <param name="resolution">The final output resolution where the width equals <see cref="width"/> and height is variable.</param>
        public static void WidthToResolution(int width, float aspectRatio, out Resolution resolution)
        {
            resolution = new Resolution();
            resolution.Width   = width;
            resolution.Height  = (int)(width / aspectRatio);
        }

        /// <summary>
        /// Converts a fixed height and aspect ratio to a width-height resolution pair.
        /// </summary>
        /// <param name="height">The height of the resolution to produce.</param>
        /// <param name="aspectRatio">The aspect ratio of the resolution to produce. e.g. 4F/3F.</param>
        /// <param name="resolution">The final output resolution where the height equals <see cref="height"/> and width is variable.</param>
        public static void HeightToResolution(int height, float aspectRatio, out Resolution resolution)
        {
            resolution = new Resolution();
            resolution.Height = height;
            resolution.Width = (int)(height * aspectRatio);
        }

        /// <summary>
        /// Obtains the relative aspect ratio of a given aspect compared to the game's aspect.
        /// </summary>
        /// <param name="currentAspect">The current aspect of the game window.</param>
        /// <returns>The aspect of the game window relative to the original aspect of the game.</returns>
        public static float GetRelativeAspect(float currentAspect)
        {
            return currentAspect / OriginalGameAspect;
        }

        /// <summary>
        /// Obtains the aspect ratio (represented as float) of the native RECT structure.
        /// </summary>
        public static float ToAspectRatio(ref RECT rect)
        {
            return rect.Width / (float) rect.Height;
        }
    }
}
