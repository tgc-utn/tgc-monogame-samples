using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.Transformations
{
    public class Spaces : TGCSample
    {
        private Camera MainCamera { get; set; }

        private Camera ViewerCamera { get; set; }

        private BoxPrimitive Box { get; set; }

        private Quaternion Quaternion { get; set; }
        
        private Matrix BoxWorld { get; set; }
        
        private float AspectRatio { get; set; }

        private Vector3 Position { get; set; } = Vector3.Zero;

        private Vector3 Scale { get; set; } = Vector3.One;

        private SpaceType Space { get; set; } = SpaceType.Local;

        private float Interpolator { get; set; } = 0f;

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

            ModifierController.AddOptions("Space", SpaceType.Local, OnChange);

            ModifierController.AddVector("Position", OnPositionChange, Vector3.Zero);

            ModifierController.AddVector("Rotation", OnRotationChange, Vector3.Zero);

            ModifierController.AddVector("Scale", OnScaleChange, Vector3.One);

            ModifierController.AddFloat("FOV", OnFOVChange, FOV, 0.01f, 180f - 0.01f);

            ModifierController.AddFloat("Interpolator", OnInterpolatorChange, 0f, 0f, 1f);

            base.Initialize();
        }


        private enum SpaceType
        {
            Local = 0,
            World = 1,
            View = 2,
            Projection = 3,
        }

        private void OnInterpolatorChange(float interpolator)
        {
            Interpolator = interpolator;
        }


        private void OnFOVChange(float fov)
        {
            // Prevent FOV to reach 0 or 180
            fov = Math.Clamp(fov, 0.01f, 180f - 0.01f);
            FOV = fov;
            ViewerCamera.BuildProjection(AspectRatio, 2f, 500f, ToRadians(FOV));
        }

        private void OnChange(SpaceType space)
        {
            Space = space;
            Interpolator = 0f;
        }

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

        private void OnRotationChange(Vector3 rotation)
        {
            var rotationInRadians = new Vector3(ToRadians(rotation.X), ToRadians(rotation.Y), ToRadians(rotation.Z));

            Quaternion = Quaternion.CreateFromAxisAngle(Vector3.Backward, rotationInRadians.Z) *
                Quaternion.CreateFromAxisAngle(Vector3.Up, rotationInRadians.Y) *
                Quaternion.CreateFromAxisAngle(Vector3.Right, rotationInRadians.X);
            UpdateWorld();
        }

        private void OnScaleChange(Vector3 scale)
        {
            Scale = scale;
            UpdateWorld();
        }

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



            Box.Draw(interpolated, MainCamera.View, MainCamera.Projection);


            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitX * 5000f, Color.Red);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitY * 5000f, Color.Green);
            Game.Gizmos.DrawLine(Vector3.Zero, Vector3.UnitZ * 5000f, Color.Blue);


            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {

            base.UnloadContent();
        }
    }
}

