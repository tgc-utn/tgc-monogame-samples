using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 4:
    ///     Units Involved:
    ///     # Unit 4 - Textures and lighting - Textures
    ///     Shows how to create a Quad with a 2D image as a texture to give it color.
    ///     Author: Matías Leone.
    /// </summary>
    public class Tutorial4 : TGCSample
    {
        /// <inheritdoc />
        public Tutorial4(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 4";
            Description = "Shows how to create a Quad with a 2D image as a texture to give it color.";
        }

        private Camera Camera { get; set; }
        private QuadPrimitive Quad { get; set; }
        private float Rotation { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            Camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);

            var texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "Wood/cajaMadera3");
            Quad = new QuadPrimitive(Game.GraphicsDevice, Vector3.Zero, Vector3.Backward, Vector3.Up, 22, 22, texture,
                2);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Rotation += gameTime.ElapsedGameTime.Milliseconds * 0.001f;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(Camera.View, Camera.Projection);

            Quad.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}