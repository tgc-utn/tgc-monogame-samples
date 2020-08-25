using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Geometries;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    public enum demoState {
        Normal,
        View,
        ViewProj,
        ViewProjGiant
    }
    /// <summary>
    /// Default example with TGC logo.
    /// </summary>
    public class TGCTransformsSample : TGCSample
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public TGCTransformsSample(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = GetType().Name;
            Description = Description = "View and Projection transforms example";
        }

        private Camera Camera { get; set; }

        private Camera DummyCamera { get; set; }
        private Model Model { get; set; }
        private float Rotation { get; set; }

        private FrustumPrimitive frustum { get; set; }

        private demoState currentState = demoState.Normal;

        private Matrix currentStateTransform = Matrix.Identity;
        private KeyboardState oldState;



        ///<inheritdoc/>
        public override void Initialize()
        {
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 50f, 400f),100f);
            DummyCamera = new DummyCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitX * 150, Vector3.Zero);
            frustum =  new FrustumPrimitive(GraphicsDevice, DummyCamera);
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
            var keyboardState = Keyboard.GetState();
            if(keyboardState.IsKeyUp(Keys.X) && oldState.IsKeyDown(Keys.X)){
                switch (currentState)
                {
                    case demoState.Normal:
                        currentState = demoState.View;
                        currentStateTransform = DummyCamera.View;
                        break;
                    case demoState.View:
                        currentState = demoState.ViewProj;
                        currentStateTransform = DummyCamera.View * DummyCamera.Projection;
                        break;
                    case demoState.ViewProj:
                        currentState = demoState.ViewProjGiant;
                        currentStateTransform = DummyCamera.View * DummyCamera.Projection * Matrix.CreateScale(20);
                        break;
                    case demoState.ViewProjGiant:
                        currentState = demoState.Normal;
                        currentStateTransform = Matrix.Identity;
                        break;
                    default:
                        break;
                }
            }
            oldState = keyboardState;
            Camera.Update(gameTime);
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
                                       Matrix.CreateTranslation(new Vector3(0, -50f, 0)) * currentStateTransform;
                    castEffect.View = Camera.View;
                    castEffect.Projection = Camera.Projection;
                    castEffect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            frustum.Draw(Camera.View,Camera.Projection,currentStateTransform);


            base.Draw(gameTime);
        }
    }
}