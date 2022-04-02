using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    /// <summary>
    ///     Basic Shader:
    ///     Units Involved:
    ///     # Unit 8 - Video Adapters - Shaders.
    ///     It's the hello world of shaders.
    ///     Author: Mariano Banquiero
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

        private Camera Camera { get; set; }
        private Effect Effect { get; set; }
        private ModelDrawer ModelDrawer { get; set; }
        private Texture2D Texture { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 50f, 400f));
            time = 0;
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

            // Load a shader in runtime, outside the Content pipeline.
            // First you must run "mgfxc <SourceFile> <OutputFile> [/Debug] [/Profile:<DirectX_11,OpenGL>]"
            // https://docs.monogame.net/articles/tools/mgfxc.html
            //byte[] byteCode = File.ReadAllBytes(Game.Content.RootDirectory + "/" + ContentFolderEffects + "BasicShader.fx");
            //Effect = new Effect(GraphicsDevice, byteCode);

            // Load a shader using Content pipeline.
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BasicShader");

            ModelDrawer = ModelInspector.CreateDrawerFrom(model, Effect, EffectInspectionType.ALL);

            var viewParameter = Effect.Parameters["View"];
            var projectionParameter = Effect.Parameters["Projection"];

            ModelDrawer.ModelActionCollection.Add(data => viewParameter.SetValue(Camera.View));
            ModelDrawer.ModelActionCollection.Add(data => projectionParameter.SetValue(Camera.Projection));

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

            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            Effect.Parameters["Time"].SetValue(time);
            ModelDrawer.Draw();

            base.Draw(gameTime);
        }
    }
}