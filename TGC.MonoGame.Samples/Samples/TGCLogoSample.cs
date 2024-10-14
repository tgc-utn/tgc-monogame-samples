using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples
{
    /// <summary>
    ///     Default example with TGC logo.
    /// </summary>
    public class TGCLogoSample : TGCSample
    {
        private Camera _camera;

        private Model _model;

        private Matrix _world;

        private float _angle;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public TGCLogoSample(TGCViewer game) : base(game)
        {
            Name = GetType().Name;
            Description = Description = "Time to explore the samples :)";
        }
        
        /// <inheritdoc />
        public override void Initialize()
        {
            _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 150, Vector3.UnitZ);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load mesh.
            _model = Game.Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
            var modelEffect = (BasicEffect) _model.Meshes[0].Effects[0];
            modelEffect.DiffuseColor = Color.DarkBlue.ToVector3();
            modelEffect.EnableDefaultLighting();
            _world = Matrix.Identity;

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _angle += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            _world = Matrix.CreateRotationY(_angle);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _model.Draw(_world, _camera.View, _camera.Projection);

            base.Draw(gameTime);
        }
    }
}
