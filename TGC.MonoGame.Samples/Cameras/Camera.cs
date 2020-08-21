using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    /// The minimum behavior that a camera should have.
    /// </summary>
    public abstract class Camera
    {

        /// <summary>
        /// Direction where the camera is looking.
        /// </summary>
        public Vector3 FrontDirection { get; set; }

        /// <summary>
        /// Position where the camera is located.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Represents the positive x-axis of the camera space.
        /// </summary>
        public Vector3 RightDirection { get; set; }

        /// <summary>
        /// Vector up direction (may differ if the camera is reversed).
        /// </summary>
        public Vector3 UpDirection { get; set; }

        /// <summary>
        /// The created view matrix.
        /// </summary>
        public Matrix ViewMatrix { get; set; }
    }
}