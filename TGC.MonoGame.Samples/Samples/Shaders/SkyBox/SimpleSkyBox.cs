using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders.SkyBox
{
    /// <summary>
    ///     Simple SkyBox:
    ///     Units Involved:
    ///     # Unit 4 - Textures and Lighting - SkyBox.
    ///     # Unit 8 - Video Adapters - Shaders.
    ///     Shows how to use a cube with a texture on each of its faces, which allows to achieve the effect of an enveloping
    ///     sky in the scene.
    ///     Author: Rene Juan Rico Mendoza
    /// </summary>
    public class SimpleSkyBox : TGCSample
    {
        /// <inheritdoc />
        public SimpleSkyBox(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Simple SkyBox";
            Description =
                "Shows how to use a cube with a texture on each of its faces, which allows to achieve the effect of an enveloping sky in the scene.";
        }

        private float _angle;
        private Vector3 _cameraPosition;
        private Vector3 _cameraTarget;
        private float _distance;
        private Matrix _view;
        private Vector3 _viewVector;
        private Matrix _projection;
        private SkyBox _skyBox;

        /// <inheritdoc />
        public override void Initialize()
        {
            _cameraTarget = Vector3.Zero;
            _view = Matrix.CreateLookAt(Vector3.UnitX * 20, _cameraTarget, Vector3.UnitY);
            _projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f,
                    100f);
            _distance = 20;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var skyBox = Game.Content.Load<Model>(ContentFolder3D + "skybox/cube");
            //var skyBoxTexture = Game.Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/sunset/sunset");
            //var skyBoxTexture = Game.Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/islands/islands");
            var skyBoxTexture = Game.Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/skybox/skybox");
            var skyBoxEffect = Game.Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            _skyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _cameraPosition = _distance * new Vector3((float) Math.Sin(_angle), 0, (float) Math.Cos(_angle));
            _viewVector = Vector3.Transform(_cameraTarget - _cameraPosition, Matrix.CreateRotationY(0));
            _viewVector.Normalize();

            _angle += 0.002f;
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.UnitY);

            Game.Gizmos.UpdateViewProjection(_view, _projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;

            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Game.Graphics.GraphicsDevice.RasterizerState = rasterizerState;

            //TODO why I have to set 1 in the alpha channel in the fx file?
            _skyBox.Draw(_view, _projection, _cameraPosition);

            GraphicsDevice.RasterizerState = originalRasterizerState;

            base.Draw(gameTime);
        }
    }
}