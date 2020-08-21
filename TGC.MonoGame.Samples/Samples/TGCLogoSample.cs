using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples
{
    /// <summary>
    /// Default example with TGC logo.
    /// </summary>
    public class TGCLogoSample : TGCSample
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public TGCLogoSample(TGCViewer game) : base(game)
        {
            Name = GetType().Name;
            Description = Description = "Time to explore the samples :)";
        }

        private Camera Camera { get; set; }
        private Model Model { get; set; }
        private float Rotation { get; set; }

        ///<inheritdoc/>
        public override void Initialize()
        {
            Camera = new StaticCamera(Vector3.UnitZ * 150, Vector3.Zero);
            base.Initialize();
        }

        ///<inheritdoc/>
        protected override void LoadContent()
        {
            // Load mesh.
            Model = Game.Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
            base.LoadContent();
        }

        ///<inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            Rotation += gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            base.Update(gameTime);
        }

        ///<inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var mesh in Model.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    var castEffect = (BasicEffect)effect;
                    castEffect.World = Matrix.CreateRotationY(Rotation) *
                                       Matrix.CreateTranslation(new Vector3(0, -40, 0));
                    castEffect.View = Camera.ViewMatrix;
                    castEffect.Projection = Projection;
                    castEffect.EnableDefaultLighting();
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}