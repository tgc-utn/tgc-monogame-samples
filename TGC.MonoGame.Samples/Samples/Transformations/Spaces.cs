using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Transformations
{
    /// <summary>
    ///     An example that shows the difference between spaces.
    ///     # Unit 2 - 3D Basics - Transformations
    ///     Transforms vertices across local space, world space, view space and NDC.
    ///     Authors: Ronan Vinitzca.
    /// </summary>
    public class Spaces : TGCSample
    {
        /// <inheritdoc />
        public Spaces(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Spaces";
            Description = "Shows how a model is transformed through different spaces.";
        }

        /// <summary>
        ///     A camera to draw geometry into the screen
        /// </summary>
        private Camera _mainCamera;

        /// <summary>
        ///     Simulates a camera to transform a model and show the different spaces a vertex can be in
        /// </summary>
        private Camera _viewerCamera;

        /// <summary>
        ///     A box to be drawn in different spaces
        /// </summary>
        private BoxPrimitive _box;

        /// <summary>
        ///     A quaternion that describes the box rotation
        /// </summary>
        private Quaternion _quaternion;

        /// <summary>
        ///     A vector containing the position of the box
        /// </summary>
        private Vector3 _position = Vector3.Zero;

        /// <summary>
        ///     A vector containing the scale of the box
        /// </summary>
        private Vector3 _scale = Vector3.One;

        /// <summary>
        ///     The world matrix of the box
        /// </summary>
        private Matrix _boxWorld;

        /// <summary>
        ///     The aspect ratio of the window, needed to recalculate the projection matrix
        /// </summary>
        private float _aspectRatio;

        /// <summary>
        ///     The current space we are drawing the box in
        /// </summary>
        private SpaceType _space = SpaceType.Local;

        /// <summary>
        ///     A value between zero and one to interpolate between spaces
        /// </summary>
        private float _interpolator;

        /// <summary>
        ///     The current field of view angle in degrees
        /// </summary>
        private float _fov = 60f;

        /// <inheritdoc />
        public override void Initialize()
        {
            _aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            var camera = new FreeCamera(_aspectRatio, Vector3.UnitZ);
            camera.MovementSpeed = 30f;
            _mainCamera = camera;

            _viewerCamera = new StaticCamera(_aspectRatio, Vector3.Left * 100f, Vector3.Backward, Vector3.Up);
            _viewerCamera.BuildProjection(_aspectRatio, 2f, 500f, MathHelper.PiOver2 * 2f / 3f);

            _boxWorld = Matrix.CreateScale(_scale) *
                       Matrix.CreateFromQuaternion(_quaternion) *
                       Matrix.CreateTranslation(_position);

            ModifierController.AddOptions("Space", SpaceType.Local, OnSpaceChange);

            ModifierController.AddVector("Position", OnPositionChange, Vector3.Zero);

            ModifierController.AddVector("Rotation", OnRotationChange, Vector3.Zero);

            ModifierController.AddVector("Scale", OnScaleChange, Vector3.One);

            ModifierController.AddFloat("FOV", OnFOVChange, _fov, 0.01f, 180f - 0.01f);

            ModifierController.AddFloat("Interpolator", OnInterpolatorChange, 0f, 0f, 1f);

            base.Initialize();
        }

        /// <summary>
        ///     Processes a change in the interpolation value used to blend spaces.
        /// </summary>
        /// <param name="interpolator">The new interpolator value, with range [0, 1]</param>
        private void OnInterpolatorChange(float interpolator)
        {
            _interpolator = interpolator;
        }

        /// <summary>
        ///     Processes a change in the FOV of the projection matrix.
        /// </summary>
        /// <param name="fieldOfView">The new FOV value in degrees</param>
        private void OnFOVChange(float fieldOfView)
        {
            // Prevent FOV to reach 0 or 180
            fieldOfView = Math.Clamp(fieldOfView, 0.01f, 180f - 0.01f);
            _fov = fieldOfView;

            _viewerCamera.BuildProjection(_aspectRatio, 2f, 500f, ToRadians(_fov));
        }

        /// <summary>
        ///     Processes a change in the space of the box.
        /// </summary>
        /// <param name="space">The new space to draw the box in</param>
        private void OnSpaceChange(SpaceType space)
        {
            _space = space;
            _interpolator = 0f;
        }

        /// <summary>
        ///     Processes a change in the position of the box.
        /// </summary>
        /// <param name="position">The position o</param>
        private void OnPositionChange(Vector3 position)
        {
            _position = position;
            UpdateWorld();
        }

        /// <summary>
        ///     Converts an angle in degrees to radians.
        /// </summary>
        /// <param name="angleInDegrees">The angle to convert to degrees.</param>
        /// <returns>The converted angle in radians</returns>
        private float ToRadians(float angleInDegrees)
        {
            return angleInDegrees * MathF.PI / 180f;
        }

        /// <summary>
        ///     Processes a change in the rotation of the box.
        /// </summary>
        /// <param name="rotation">The new rotation angle in degrees for each axis</param>
        private void OnRotationChange(Vector3 rotation)
        {
            var rotationInRadians = new Vector3(ToRadians(rotation.X), ToRadians(rotation.Y), ToRadians(rotation.Z));

            _quaternion = Quaternion.CreateFromAxisAngle(Vector3.Backward, rotationInRadians.Z) *
                         Quaternion.CreateFromAxisAngle(Vector3.Up, rotationInRadians.Y) *
                         Quaternion.CreateFromAxisAngle(Vector3.Right, rotationInRadians.X);
            UpdateWorld();
        }

        /// <summary>
        ///     Processes a change in the scale of the box.
        /// </summary>
        /// <param name="scale">The new scale for each axis</param>
        private void OnScaleChange(Vector3 scale)
        {
            _scale = scale;
            UpdateWorld();
        }

        /// <summary>
        ///     Updates the world matrix, using a scale and position matrices and a rotation quaternion.
        /// </summary>
        private void UpdateWorld()
        {
            _boxWorld = Matrix.CreateScale(_scale) *
                       Matrix.CreateFromQuaternion(_quaternion) *
                       Matrix.CreateTranslation(_position);
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-1");
            _box = new BoxPrimitive(GraphicsDevice, Vector3.One, texture);
            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _mainCamera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_mainCamera.View, _mainCamera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;

            // Save the past RasterizerState
            var oldRasterizerState = GraphicsDevice.RasterizerState;
            // Use a RasterizerState which has Back-Face Culling disabled
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Interpolate across spaces.
            // Note that we are linearly interpolating between matrices
            // Also draw frustum in world space, view space and the canonical view volume
            var interpolated = Matrix.Identity;
            switch (_space)
            {
                case SpaceType.Local:
                    interpolated = Matrix.Lerp(Matrix.Identity, _boxWorld, _interpolator);
                    Game.Gizmos.DrawFrustum(_viewerCamera.View * _viewerCamera.Projection, Color.Yellow * _interpolator);
                    break;

                case SpaceType.World:
                    interpolated = Matrix.Lerp(_boxWorld, _boxWorld * _viewerCamera.View, _interpolator);
                    Game.Gizmos.DrawFrustum(_viewerCamera.View * _viewerCamera.Projection,
                        Color.Yellow * (1f - _interpolator));
                    Game.Gizmos.DrawFrustum(_viewerCamera.Projection, Color.Green * _interpolator);
                    break;

                case SpaceType.View:
                    interpolated = Matrix.Lerp(_boxWorld * _viewerCamera.View,
                        _boxWorld * _viewerCamera.View * _viewerCamera.Projection, _interpolator);
                    Game.Gizmos.DrawFrustum(_viewerCamera.Projection, Color.Green * (1f - _interpolator));
                    Game.Gizmos.DrawCube(new Vector3(0f, 0f, 0.5f), new Vector3(1f, 1f, 1f),
                        Color.Purple * _interpolator);
                    break;

                case SpaceType.Projection:
                    interpolated = _boxWorld * _viewerCamera.View * _viewerCamera.Projection;
                    Game.Gizmos.DrawCube(new Vector3(0f, 0f, 0.5f), new Vector3(1f, 1f, 1f), Color.Purple);
                    break;
            }

            // Note that the MainCamera (the one we use in the example) is still used
            // to render the Box, as we still want to visualize it.
            // The final transformation for vertices would be vertex * interpolated * MainCamera.View * MainCamera.Projection
            _box.Draw(interpolated, _mainCamera.View, _mainCamera.Projection);

            // Draw axis lines
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitX * 5000f, Color.Red);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitY * 5000f, Color.Green);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitZ * 5000f, Color.Blue);

            // Restore the old RasterizerState
            GraphicsDevice.RasterizerState = oldRasterizerState;

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Describes a space to transform models.
        /// </summary>
        private enum SpaceType
        {
            Local = 0,
            World = 1,
            View = 2,
            Projection = 3
        }
    }
}