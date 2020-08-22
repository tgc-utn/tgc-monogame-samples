using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    ///     The minimum behavior that a camera should have.
    /// </summary>
    public abstract class Camera
    {
        public const float DefaultFieldOfViewDegrees = MathHelper.PiOver4;
        public const float DefaultNearPlaneDistance = 0.1f;
        public const float DefaultFarPlaneDistance = 2000;

        public Camera(float aspectRatio) : this(aspectRatio, DefaultFieldOfViewDegrees, DefaultNearPlaneDistance, DefaultFarPlaneDistance)
        {
        }

        public Camera(float aspectRatio, float fieldOfViewDegrees, float nearPlaneDistance, float farPlaneDistance)
        {
            BuildProjection(aspectRatio, fieldOfViewDegrees, nearPlaneDistance, farPlaneDistance);
        }

        public void BuildProjection(float aspectRatio, float fieldOfViewDegrees, float nearPlaneDistance, float farPlaneDistance)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfViewDegrees, aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        /// <summary>
        ///     Aspect ratio, defined as view space width divided by height.
        /// </summary>
        public float AspectRatio { get; set; }

        /// <summary>
        ///     Distance to the far view plane.
        /// </summary>
        public float FarPlane { get; set; }

        /// <summary>
        ///     Field of view in the y direction, in radians.
        /// </summary>
        public float FieldOfView { get; set; }

        /// <summary>
        ///     Distance to the near view plane.
        /// </summary>
        public float NearPlane { get; set; }

        /// <summary>
        ///     Direction where the camera is looking.
        /// </summary>
        public Vector3 FrontDirection { get; set; }

        /// <summary>
        ///     The perspective projection matrix.
        /// </summary>
        public Matrix Projection { get; set; }

        /// <summary>
        ///     Position where the camera is located.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        ///     Represents the positive x-axis of the camera space.
        /// </summary>
        public Vector3 RightDirection { get; set; }

        /// <summary>
        ///     Vector up direction (may differ if the camera is reversed).
        /// </summary>
        public Vector3 UpDirection { get; set; }

        /// <summary>
        ///     The created view matrix.
        /// </summary>
        public Matrix View { get; set; }

        /// <summary>
        ///     Allows updating the internal state of the camera if this method is overwritten.
        ///     By default it does not perform any action.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        public abstract void Update(GameTime gameTime);
    }
}