using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples
{
    /// <summary>
    ///     Default example with TGC logo.
    /// </summary>
    public class TGCLogoSample : TGCSample
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public TGCLogoSample(TGCViewer game) : base(game)
        {
            Name = GetType().Name;
            Description = Description = "Time to explore the samples :)";
        }

        private Camera Camera { get; set; }
        private ModelDrawer ModelDrawer { get; set; }

        private Effect Effect { get; set; }

        private Matrix ModelWorld { get; set; }
        private float ModelRotation { get; set; }

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
            var model = Game.Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");

            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");
            ModelDrawer = ModelInspector.CreateDrawerFrom(model, Effect, Effect.Techniques["BaseColor"], EffectInspectionType.MATRICES);

            Effect.Parameters["baseColor"].SetValue(Color.DarkBlue.ToVector3()); 
            Effect.Parameters["ambientColor"].SetValue(Vector3.One);
            Effect.Parameters["diffuseColor"].SetValue(Vector3.One);
            Effect.Parameters["specularColor"].SetValue(Vector3.One);

            Effect.Parameters["KAmbient"].SetValue(0.3f);
            Effect.Parameters["KDiffuse"].SetValue(0.8f);
            Effect.Parameters["KSpecular"].SetValue(1.0f);
            Effect.Parameters["shininess"].SetValue(16f);
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);
            Effect.Parameters["lightPosition"].SetValue(Vector3.One * 10f);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            ModelRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            ModelDrawer.World = Matrix.CreateRotationY(ModelRotation);
            ModelDrawer.ViewProjection = Camera.View * Camera.Projection;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            ModelDrawer.Draw();

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            ModelDrawer.Dispose();
            base.UnloadContent();
        }
    }
}