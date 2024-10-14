using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 4:
    ///     Units Involved:
    ///     # Unit 4 - Textures and lighting - Textures
    ///     Shows how to create a Quad and a Box with a 2D image as a texture to give it color.
    ///     Author: Matías Leone.
    /// </summary>
    public class Tutorial4 : TGCSample
    {
        /// <inheritdoc />
        public Tutorial4(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 4";
            Description = "Shows how to create a Quad and a Box with a 2D image as a texture to give it color.";
        }

        private Camera _camera;
        private QuadPrimitive _quad;
        private Matrix _quadWorld;
        private BoxPrimitive _box;
        private Matrix _boxWorld;
        private float _boxRotation;

        /// <inheritdoc />
        public override void Initialize()
        {
            _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 10, 60), Vector3.Zero);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-3");
            _quad = new QuadPrimitive(GraphicsDevice);
            _quad.Effect.Texture = texture;

            _quadWorld = Matrix.CreateScale(10f) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitX * 14);
             
            _box = new BoxPrimitive(GraphicsDevice, Vector3.One * 20, texture);
            _boxWorld = Matrix.CreateTranslation(Vector3.UnitX * -14);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime);
            _boxRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            _box.Draw(Matrix.CreateRotationY(_boxRotation) * _boxWorld, _camera.View, _camera.Projection);
            _quad.Draw(_quadWorld, _camera.View, _camera.Projection);

            base.Draw(gameTime);
        }
    }
}