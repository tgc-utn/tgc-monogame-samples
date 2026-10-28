using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    ///     The minimum behavior that a camera should have.
    /// </summary>
    public abstract class Camera
    {
        /// <summary>
        ///     Default field of view in the y direction, in radians.
        /// </summary>
        public const float DefaultFieldOfView = MathHelper.PiOver4;

        /// <summary>
        ///     Default distance to the near view plane.
        /// </summary>
        public const float DefaultNearPlaneDistance = 0.1f;

        /// <summary>
        ///     Default distance to the far view plane.
        /// </summary>
        public const float DefaultFarPlaneDistance = 2000;

        /// <summary>
        ///     Creates a camera with the default near plane, far plane, and field of view.
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        protected Camera(float aspectRatio)
            : this(aspectRatio, DefaultNearPlaneDistance, DefaultFarPlaneDistance)
        {
        }

        /// <summary>
        ///     Creates a camera with the default field of view.
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="nearPlaneDistance">Distance to the near view plane.</param>
        /// <param name="farPlaneDistance">Distance to the far view plane.</param>
        protected Camera(float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
            : this(aspectRatio, nearPlaneDistance, farPlaneDistance, DefaultFieldOfView)
        {
        }

        /// <summary>
        ///     Creates a camera and builds its perspective projection matrix.
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="nearPlaneDistance">Distance to the near view plane.</param>
        /// <param name="farPlaneDistance">Distance to the far view plane.</param>
        /// <param name="fieldOfView">Field of view in the y direction, in radians.</param>
        protected Camera(float aspectRatio, float nearPlaneDistance, float farPlaneDistance, float fieldOfView)
        {
            BuildProjection(aspectRatio, nearPlaneDistance, farPlaneDistance, fieldOfView);
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
        ///     The direction that is "up" in the world, used as reference to orient the camera.
        /// </summary>
        public Vector3 DefaultWorldUpVector { get; } = Vector3.Up;

        /// <summary>
        ///     The created view matrix.
        /// </summary>
        public Matrix View { get; set; }

        /// <summary>
        ///     Build a perspective projection matrix based on a field of view, aspect ratio, and near and far view plane
        ///     distances.
        /// </summary>
        /// <param name="aspectRatio">The aspect ratio, defined as view space width divided by height.</param>
        /// <param name="nearPlaneDistance">The distance to the near view plane.</param>
        /// <param name="farPlaneDistance">The distance to the far view plane.</param>
        /// <param name="fieldOfView">The field of view in the y direction, in radians.</param>
        public void BuildProjection(
            float aspectRatio,
            float nearPlaneDistance,
            float farPlaneDistance,
            float fieldOfView)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView,
                aspectRatio,
                nearPlaneDistance,
                farPlaneDistance);
        }

        /// <summary>
        ///     Allows updating the internal state of the camera if this method is overwritten.
        ///     By default it does not perform any action.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        public abstract void Update(GameTime gameTime);
    }
}
