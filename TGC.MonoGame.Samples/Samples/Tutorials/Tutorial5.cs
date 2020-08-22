using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 5:
    ///     Units Involved:
    ///     # Unit 3 - 3D Basics - Mesh
    ///     Shows how to load a 3D model. You can move around the scene with a simple camera that handles asdw and arrows keys.
    ///     Author: Matías Leone.
    /// </summary>
    public class Tutorial5 : TGCSample
    {
        public Tutorial5(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 5";
            Description =
                "Shows how to load a 3D model. You can move around the scene with a simple camera that handles asdw and arrows keys.";
        }

        private Camera Camera { get; set; }
        private Model Model1 { get; set; }
        private Model Model2 { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 0, 55), 0.01f);

            Model1 = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            Model2 = Game.Content.Load<Model>(ContentFolder3D + "tank/tank");

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(Camera.View, Camera.Projection);

            foreach (var mesh in Model1.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    var castEffect = (BasicEffect) effect;
                    castEffect.World = Matrix.Identity * Matrix.CreateScale(0.1f) *
                                       Matrix.CreateTranslation(Vector3.UnitX * -8);
                    castEffect.View = Camera.View;
                    castEffect.Projection = Camera.Projection;
                    castEffect.EnableDefaultLighting();
                }

                mesh.Draw();
            }

            // Look up the bone transform matrices.
            var transforms = new Matrix[Model2.Bones.Count];
            Model2.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (var mesh in Model2.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    var castEffect = (BasicEffect) effect;
                    castEffect.World = transforms[mesh.ParentBone.Index] * Matrix.Identity *
                                       Matrix.CreateScale(2.8f) * Matrix.CreateTranslation(new Vector3(8, -5, 0));
                    castEffect.View = Camera.View;
                    castEffect.Projection = Camera.Projection;
                    castEffect.EnableDefaultLighting();
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}