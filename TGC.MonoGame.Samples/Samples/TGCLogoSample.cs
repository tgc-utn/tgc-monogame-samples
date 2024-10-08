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
        private Camera Camera { get; set; }
        
        private Model Model { get; set; }
        
        private Matrix World { get; set; }
        
        private float Angle { get; set; }

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
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 150, Vector3.UnitZ);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load mesh.
            Model = Game.Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
            var modelEffect = (BasicEffect) Model.Meshes[0].Effects[0];
            modelEffect.DiffuseColor = Color.DarkBlue.ToVector3();
            modelEffect.EnableDefaultLighting();
            World = Matrix.Identity;

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Angle += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            World = Matrix.CreateRotationY(Angle);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Model.Draw(World, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}
