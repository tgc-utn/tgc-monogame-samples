using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Models.Drawers;
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

        private Effect Effect { get; set; }
        private ModelDrawer ModelDrawerOne { get; set; }
        private ModelDrawer ModelDrawerTwo { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 55, 15, 0.5f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var modelOne = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            var modelTwo = Game.Content.Load<Model>(ContentFolder3D + "tank/tank");

            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "DiffuseTexture");

            ModelDrawerOne = ModelInspector.CreateDrawerFrom(modelOne, Effect, EffectInspectionType.ALL);
            ModelDrawerTwo = ModelInspector.CreateDrawerFrom(modelTwo, Effect, EffectInspectionType.ALL);


            base.LoadContent();
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var viewProjection = Camera.View * Camera.Projection;

            ModelDrawerOne.World = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(Vector3.UnitX * -8);
            ModelDrawerOne.ViewProjection = viewProjection;
            ModelDrawerOne.Draw();

            ModelDrawerTwo.World = Matrix.CreateScale(2.8f) * Matrix.CreateTranslation(new Vector3(8, -5, 0));
            ModelDrawerTwo.ViewProjection = viewProjection;
            ModelDrawerTwo.Draw();

            base.Draw(gameTime);
        }
    }
}