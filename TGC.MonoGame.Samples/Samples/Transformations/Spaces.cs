using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;


namespace TGC.MonoGame.Samples.Samples.Transformations
{
    /// <summary>
    /// An example that shows the difference between spaces.
    /// # Unit 2 - 3D Basics - Transformations
    /// Transforms vertices across local space, world space, view space and NDC.
    /// Authors: Ronan Vinitzca.
    /// </summary>
    public class Spaces : TGCSample
    {
        /// <summary>
        /// A camera to draw geometry into the screen
        /// </summary>
        private Camera MainCamera { get; set; }

        /// <summary>
        /// Simulates a camera to transform a model and show the different spaces a vertex can be in
        /// </summary>
        private Camera ViewerCamera { get; set; }

        /// <summary>
        /// A box to be drawn in different spaces
        /// </summary>
        private BoxPrimitive Box { get; set; }

        /// <summary>
        /// A quaternion that describes the box rotation
        /// </summary>
        private Quaternion Quaternion { get; set; }

        /// <summary>
        /// A vector containing the position of the box
        /// </summary>
        private Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>
        /// A vector containing the scale of the box
        /// </summary>
        private Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// The world matrix of the box
        /// </summary>
        private Matrix BoxWorld { get; set; }
        
        /// <summary>
        /// The aspect ratio of the window, needed to recalculate the projection matrix
        /// </summary>
        private float AspectRatio { get; set; }

        /// <summary>
        /// The current space we are drawing the box in
        /// </summary>
        private SpaceType Space { get; set; } = SpaceType.Local;

        /// <summary>
        /// A value between zero and one to interpolate between spaces
        /// </summary>
        private float Interpolator { get; set; } = 0f;

        /// <summary>
        /// The current field of view angle in degrees
        /// </summary>
        private float FOV { get; set; } = 60f;


        /// <inheritdoc />
        public Spaces(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Spaces";
            Description = "Shows how a model is transformed through different spaces.";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            AspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            var camera = new FreeCamera(AspectRatio, Vector3.UnitZ);
            camera.MovementSpeed = 30f;
            MainCamera = camera;


            ViewerCamera = new StaticCamera(AspectRatio, Vector3.Left * 100f, Vector3.Backward, Vector3.Up);
            ViewerCamera.BuildProjection(AspectRatio, 2f, 500f, MathHelper.PiOver2 * 2f / 3f);

            BoxWorld = Matrix.CreateScale(Scale) *
                Matrix.CreateFromQuaternion(Quaternion) *
                Matrix.CreateTranslation(Position);

            ModifierController.AddOptions("Space", SpaceType.Local, OnSpaceChange);

            ModifierController.AddVector("Position", OnPositionChange, Vector3.Zero);

            ModifierController.AddVector("Rotation", OnRotationChange, Vector3.Zero);

            ModifierController.AddVector("Scale", OnScaleChange, Vector3.One);

            ModifierController.AddFloat("FOV", OnFOVChange, FOV, 0.01f, 180f - 0.01f);

            ModifierController.AddFloat("Interpolator", OnInterpolatorChange, 0f, 0f, 1f);

            base.Initialize();
        }

        /// <summary>
        /// Processes a change in the interpolation value used to blend spaces.
        /// </summary>
        /// <param name="interpolator">The new interpolator value, with range [0, 1]</param>
        private void OnInterpolatorChange(float interpolator)
        {
            Interpolator = interpolator;
        }


        /// <summary>
        /// Processes a change in the FOV of the projection matrix.
        /// </summary>
        /// <param name="fieldOfView">The new FOV value in degrees</param>
        private void OnFOVChange(float fieldOfView)
        {
            // Prevent FOV to reach 0 or 180
            fieldOfView = Math.Clamp(fieldOfView, 0.01f, 180f - 0.01f);
            FOV = fieldOfView;

            ViewerCamera.BuildProjection(AspectRatio, 2f, 500f, ToRadians(FOV));
        }

        /// <summary>
        /// Processes a change in the space of the box.
        /// </summary>
        /// <param name="space">The new space to draw the box in</param>
        private void OnSpaceChange(SpaceType space)
        {
            Space = space;
            Interpolator = 0f;
        }

        /// <summary>
        /// Processes a change in the position of the box.
        /// </summary>
        /// <param name="position">The position o</param>
        private void OnPositionChange(Vector3 position)
        {
            Position = position;
            UpdateWorld();
        }

        /// <summary>
        /// Converts an angle in degrees to radians.
        /// </summary>
        /// <param name="angleInDegrees">The angle to convert to degrees.</param>
        /// <returns>The converted angle in radians</returns>
        float ToRadians(float angleInDegrees)
        {
            return angleInDegrees * MathF.PI / 180f;
        }

        /// <summary>
        /// Processes a change in the rotation of the box.
        /// </summary>
        /// <param name="rotation">The new rotation angle in degrees for each axis</param>
        private void OnRotationChange(Vector3 rotation)
        {
            var rotationInRadians = new Vector3(ToRadians(rotation.X), ToRadians(rotation.Y), ToRadians(rotation.Z));

            Quaternion = Quaternion.CreateFromAxisAngle(Vector3.Backward, rotationInRadians.Z) *
                Quaternion.CreateFromAxisAngle(Vector3.Up, rotationInRadians.Y) *
                Quaternion.CreateFromAxisAngle(Vector3.Right, rotationInRadians.X);
            UpdateWorld();
        }

        /// <summary>
        /// Processes a change in the scale of the box.
        /// </summary>
        /// <param name="scale">The new scale for each axis</param>
        private void OnScaleChange(Vector3 scale)
        {
            Scale = scale;
            UpdateWorld();
        }

        /// <summary>
        /// Updates the world matrix, using a scale and position matrices and a rotation quaternion.
        /// </summary>
        private void UpdateWorld()
        {
            BoxWorld = Matrix.CreateScale(Scale) *
                Matrix.CreateFromQuaternion(Quaternion) *
                Matrix.CreateTranslation(Position);
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Texture2D texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-1");
            Box = new BoxPrimitive(GraphicsDevice, Vector3.One, texture);
            base.LoadContent();
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            MainCamera.Update(gameTime);
            
            Game.Gizmos.UpdateViewProjection(MainCamera.View, MainCamera.Projection);

            base.Update(gameTime);
        }


        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Interpolate across spaces.
            // Note that we are linearly interpolating between matrices
            // Also draw frustum in world space, view space and the canonical view volume
            Matrix interpolated = Matrix.Identity;
            switch(Space)
            {
                case SpaceType.Local:
                    interpolated = Matrix.Lerp(Matrix.Identity, BoxWorld, Interpolator);
                    Game.Gizmos.DrawFrustum(ViewerCamera.View * ViewerCamera.Projection, Color.Yellow * Interpolator);
                    break;

                case SpaceType.World:
                    interpolated = Matrix.Lerp(BoxWorld, BoxWorld * ViewerCamera.View, Interpolator);
                    Game.Gizmos.DrawFrustum(ViewerCamera.View * ViewerCamera.Projection, Color.Yellow * (1f - Interpolator));
                    Game.Gizmos.DrawFrustum(ViewerCamera.Projection, Color.Green * Interpolator);
                    break;

                case SpaceType.View:
                    interpolated = Matrix.Lerp(BoxWorld * ViewerCamera.View, BoxWorld * ViewerCamera.View * ViewerCamera.Projection, Interpolator);
                    Game.Gizmos.DrawFrustum(ViewerCamera.Projection, Color.Green * (1f - Interpolator));
                    Game.Gizmos.DrawCube(new Vector3(0f, 0f, 0.5f), new Vector3(1f, 1f, 1f), Color.Purple * Interpolator);
                    break;

                case SpaceType.Projection:
                    interpolated = BoxWorld * ViewerCamera.View * ViewerCamera.Projection;
                    Game.Gizmos.DrawCube(new Vector3(0f, 0f, 0.5f), new Vector3(1f, 1f, 1f), Color.Purple);
                    break;
            }


            // Note that the MainCamera (the one we use in the example) is still used
            // to render the Box, as we still want to visualize it.
            // The final transformation for vertices would be vertex * interpolated * MainCamera.View * MainCamera.Projection
            Box.Draw(interpolated, MainCamera.View, MainCamera.Projection);

            // Draw axis lines
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitX * 5000f, Color.Red);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitY * 5000f, Color.Green);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitZ * 5000f, Color.Blue);


            base.Draw(gameTime);
        }


        /// <summary>
        /// Describes a space to transform models.
        /// </summary>
        private enum SpaceType
        {
            Local = 0,
            World = 1,
            View = 2,
            Projection = 3,
        }

    }
}

