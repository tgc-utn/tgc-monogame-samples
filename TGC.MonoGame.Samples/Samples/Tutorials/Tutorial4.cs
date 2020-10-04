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

        private Camera Camera { get; set; }
        private QuadPrimitive Quad { get; set; }
        private Matrix QuadWorld { get; set; }
        private BoxPrimitive Box { get; set; }
        private Matrix BoxWorld { get; set; }
        private float BoxRotation { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 10, 60), Vector3.Zero);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-3");
            Quad = new QuadPrimitive(GraphicsDevice, Vector3.Zero, Vector3.Backward, Vector3.Up, 22, 22, texture,
                4);
            QuadWorld = Matrix.CreateTranslation(Vector3.UnitX * 14);
            Box = new BoxPrimitive(GraphicsDevice, Vector3.One * 20, texture);
            BoxWorld = Matrix.CreateTranslation(Vector3.UnitX * -14);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
            BoxRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            AxisLines.Draw(Camera.View, Camera.Projection);

            Box.Draw(Matrix.CreateRotationY(BoxRotation) * BoxWorld, Camera.View, Camera.Projection);
            Quad.Draw(QuadWorld, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}