using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
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
        private float _time;

        /// <inheritdoc />
        public BasicShader(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Basic Shader";
            Description = "Basic Shader Sample. Animation by vertex shader and coloring by pixel shader.";
        }

        private Camera _camera;
        private Effect _effect;
        private Model _model;
        private Texture2D _texture;

        /// <inheritdoc />
        public override void Initialize()
        {
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f, 50f, 400f));
            _time = 0;
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            _model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            // From the effect of the model I keep the texture.
            _texture = ((BasicEffect) _model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // Load a shader in runtime, outside the Content pipeline.
            // First you must run "mgfxc <SourceFile> <OutputFile> [/Debug] [/Profile:<DirectX_11,OpenGL>]"
            // https://docs.monogame.net/articles/tools/mgfxc.html
            //byte[] byteCode = File.ReadAllBytes(Game.Content.RootDirectory + "/" + ContentFolderEffects + "BasicShader.fx");
            //_effect = new Effect(GraphicsDevice, byteCode);

            // Load a shader using Content pipeline.
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "BasicShader");

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

            _time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            var mesh = _model.Meshes.FirstOrDefault();

            if (mesh != null)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = _effect;
                    _effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                    _effect.Parameters["View"].SetValue(_camera.View);
                    _effect.Parameters["Projection"].SetValue(_camera.Projection);
                    //_effect.Parameters["WorldViewProjection"].SetValue(Camera.WorldMatrix * Camera.View * Camera.Projection);
                    _effect.Parameters["ModelTexture"].SetValue(_texture);
                    _effect.Parameters["Time"].SetValue(_time);
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}