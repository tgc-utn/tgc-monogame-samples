using System.Linq;
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

        private Camera _camera;
        private Model _model1;
        private Model _model2;

        /// <inheritdoc />
        public override void Initialize()
        {
            _camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 55, 15, 0.5f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            _model1 = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            ((BasicEffect) _model1.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();

            _model2 = Game.Content.Load<Model>(ContentFolder3D + "tank/tank");

            foreach (var mesh in _model2.Meshes) ((BasicEffect) mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _model1.Draw(Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(Vector3.UnitX * -8), _camera.View,
                _camera.Projection);
            _model2.Draw(Matrix.CreateScale(2.8f) * Matrix.CreateTranslation(new Vector3(8, -5, 0)), _camera.View,
                _camera.Projection);

            base.Draw(gameTime);
        }
    }
}