using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    /// The minimum behavior that a camera should have.
    /// </summary>
    public abstract class Camera
    {
        /// <summary>
        /// Aspect ratio, defined as view space width divided by height.
        /// </summary>
        public float AspectRatio { get; set; }

        /// <summary>
        /// Distance to the far view plane.
        /// </summary>
        public float FarPlane { get; set; }

        /// <summary>
        /// Field of view in the y direction, in radians.
        /// </summary>
        public float FieldOfView { get; set; }

        /// <summary>
        /// Position where the camera is looking.
        /// </summary>
        public Vector3 LookAtDirection { get; set; }

        /// <summary>
        /// Distance to the near view plane.
        /// </summary>
        public float NearPlane { get; set; }

        /// <summary>
        /// Position where the camera is located.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The perspective projection matrix.
        /// </summary>
        public Matrix ProjectionMatrix { get; set; }

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

        /// <summary>
        /// World matrix.
        /// </summary>
        public Matrix WorldMatrix { get; set; }
    }
}