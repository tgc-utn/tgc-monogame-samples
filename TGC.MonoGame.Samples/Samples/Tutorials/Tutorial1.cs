using Microsoft.Xna.Framework;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    /// Tutorial 1:
    /// Shows how to create a triangle and display it on the screen.
    /// Author: René Juan Rico Mendoza.
    /// </summary>
    public class Tutorial1 : TGCSample
    {
        ///<inheritdoc/>
        public Tutorial1(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 1";
            Description =
                "Show how to create triangles with color vertices. You can see that there is a static camera pointing at the triangles and 2 triangles drawn with primitives.";
        }

        private Camera Camera { get; set; }
        private TrianglePrimitive Triangle { get; set; }
        private TrianglePrimitive Triangle2 { get; set; }

        ///<inheritdoc/>
        public override void Initialize()
        {
            Camera = new StaticCamera(new Vector3(0, 0, 55), Vector3.Zero);
            Triangle = new TrianglePrimitive(GraphicsDevice, new Vector3(-20f, 0f, 0f), new Vector3(-10f, 10f, 0f),
                new Vector3(0f, 0f, 0f), Color.Black, Color.Cyan, Color.Magenta);
            Triangle2 = new TrianglePrimitive(GraphicsDevice, new Vector3(0f, 0f, 0f), new Vector3(10f, 10f, 0f),
                new Vector3(20f, 0f, 0f), Color.Blue, Color.Red, Color.Green);

            base.Initialize();
        }

        ///<inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            AxisLines.Draw(Camera.ViewMatrix, Projection);

            Triangle.Draw(Matrix.Identity, Camera.ViewMatrix, Projection);
            Triangle2.Draw(Matrix.Identity, Camera.ViewMatrix, Projection);

            base.Draw(gameTime);
        }
    }
}