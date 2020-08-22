using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    /// Tutorial 2:
    /// Shows how to create a colored 3D box and display it on the screen.
    /// Author: René Juan Rico Mendoza.
    /// </summary>
    public class Tutorial2 : TGCSample
    {
        ///<inheritdoc/>
        public Tutorial2(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 2";
            Description = "Shows the Creation of a 3D Box with one color per vertex and can be moved using the keyboard arrows.";
        }

        private Camera Camera { get; set; }
        private BoxPrimitive Box { get; set; }

        private Matrix BoxWorld { get; set; } = Matrix.Identity;

        ///<inheritdoc/>
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            Point size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 20f, 60f), size);
            Box = new BoxPrimitive(GraphicsDevice, new Vector3(25, 25, 25), Vector3.Zero, Color.Black, Color.Red, Color.Yellow,
                Color.Green, Color.Blue, Color.Magenta, Color.White, Color.Cyan);

            base.Initialize();
        }

        ///<inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            // Press Directional Keys to rotate cube
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.I)) BoxWorld *= Matrix.CreateRotationX(-(float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.K)) BoxWorld *= Matrix.CreateRotationX((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.J)) BoxWorld *= Matrix.CreateRotationY(-(float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.L)) BoxWorld *= Matrix.CreateRotationY((float)gameTime.ElapsedGameTime.TotalSeconds);

            Camera.Update(gameTime);

            base.Update(gameTime);
        }

        ///<inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(Camera.View, Camera.Projection);

            Box.Draw(BoxWorld, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}