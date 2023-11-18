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

        private float Angle { get; set; }
        private Vector3 CameraPosition { get; set; }
        private Vector3 CameraTarget { get; set; }
        private float Distance { get; set; }
        private Matrix View { get; set; }
        private Vector3 ViewVector { get; set; }
        private Matrix Projection { get; set; }
        private SkyBox SkyBox { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            CameraTarget = Vector3.Zero;
            View = Matrix.CreateLookAt(Vector3.UnitX * 20, CameraTarget, Vector3.UnitY);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f,
                    100f);
            Distance = 20;

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
            SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            CameraPosition = Distance * new Vector3((float) Math.Sin(Angle), 0, (float) Math.Cos(Angle));
            ViewVector = Vector3.Transform(CameraTarget - CameraPosition, Matrix.CreateRotationY(0));
            ViewVector.Normalize();

            Angle += 0.002f;
            View = Matrix.CreateLookAt(CameraPosition, CameraTarget, Vector3.UnitY);


            Game.Gizmos.UpdateViewProjection(View, Projection);

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
            SkyBox.Draw(View, Projection, CameraPosition);

            GraphicsDevice.RasterizerState = originalRasterizerState;

            base.Draw(gameTime);
        }
    }
}