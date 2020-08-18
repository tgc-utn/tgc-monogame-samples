using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    /// <summary>
    /// Basic Shader:
    /// Units Involved:
    /// # Unit 8 - Video Adapters - Shaders.
    ///
    /// It's the hello world of shaders.
    /// Author: Mariano Banquiero
    /// </summary>
    public class BasicShader : TGCSample
    {
        private float time;

        /// <inheritdoc />
        public BasicShader(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Basic Shader";
            Description = "Basic Shader Sample. Animation by vertex shader and coloring by pixel shader.";
        }

        private SimpleCamera Camera { get; set; }
        private Effect Effect { get; set; }
        private Model Model { get; set; }
        private Texture2D Texture { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, MathHelper.PiOver4, 1, 5000,
                new Vector3(0, 50, 400), 0.1f);
            time = 0;
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            // From the effect of the model I keep the texture.
            Texture = ((BasicEffect)Model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // Load a shader in runtime, outside the Content pipeline.
            // First you must run "mgfxc <SourceFile> <OutputFile> [/Debug] [/Profile:<DirectX_11,OpenGL>]"
            // https://docs.monogame.net/articles/tools/mgfxc.html
            //byte[] byteCode = File.ReadAllBytes(Game.Content.RootDirectory + "/" + ContentFolderEffect + "BasicShader.fx");
            //Effect = new Effect(Game.GraphicsDevice, byteCode);

            // Load a shader using Content pipeline.
            Effect = Game.Content.Load<Effect>(ContentFolderEffect + "BasicShader");

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime, Game.CurrentKeyboardState);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(GraphicsDevice, Camera);

            time += gameTime.ElapsedGameTime.Milliseconds;

            var mesh = Model.Meshes.FirstOrDefault();

            if (mesh != null)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Effect;
                    Effect.Parameters["World"].SetValue(Camera.WorldMatrix * mesh.ParentBone.Transform);
                    Effect.Parameters["View"].SetValue(Camera.ViewMatrix);
                    Effect.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
                    //Effect.Parameters["WorldViewProjection"].SetValue(Camera.WorldMatrix * Camera.ViewMatrix * Camera.ProjectionMatrix);
                    Effect.Parameters["ModelTexture"].SetValue(Texture);
                    Effect.Parameters["Time"].SetValue(time);
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}