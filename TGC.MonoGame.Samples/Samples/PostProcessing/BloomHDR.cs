using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Models;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{

    public class BloomHDR : TGCSample
    {
        private RenderTarget2D SceneRenderTarget;

        private FullScreenQuad FullScreenQuad;

        private const int DownSampleCount = 7;

        private RenderTarget2D[] DownSampleRenderTargets;

        private RenderTarget2D[] UpSampleRenderTargets;

        private Camera Camera { get; set; }
        private Model Scene { get; set; }
        private ModelDrawer ModelDrawer { get; set; }

        private CubePrimitive CubePrimitive { get; set; }

        private Effect BloomEffect { get; set; }

        private Effect BlinnPhongEffect { get; set; }

        private bool EffectOn { get; set; }

        /// <inheritdoc />
        public BloomHDR(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Bloom HDR";
            Description = "Applying a Bloom HDR post-process to a scene";
        }


        /// <inheritdoc />
        public override void Initialize()
        {
            var size = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 100f), size);
            Camera.FarPlane = 100000.0f;
            Camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 1f, Camera.FarPlane, Camera.DefaultFieldOfViewDegrees);

            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Scene = Game.Content.Load<Model>(ContentFolder3D + "sponza/Sponza");

            // Load the base bloom pass effect
            BlinnPhongEffect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");
            BloomEffect = Game.Content.Load<Effect>(ContentFolderEffects + "BloomHDR");
            ModelDrawer = new ModelDrawer(Scene, GraphicsDevice);


            ModelDrawer.SetEffect(BlinnPhongEffect);
            ModelDrawer.WorldViewProjectionMatrixParameter = BlinnPhongEffect.Parameters["WorldViewProjection"];
            ModelDrawer.WorldMatrixParameter = BlinnPhongEffect.Parameters["World"];
            ModelDrawer.NormalMatrixParameter = BlinnPhongEffect.Parameters["InverseTransposeWorld"];
            ModelDrawer.TextureParameter = BlinnPhongEffect.Parameters["baseTexture"];

            ModifierController.AddVector("Light Position X", (v) => BlinnPhongEffect.Parameters["lightPosition"].SetValue(v), Vector3.Up * 100f);

            ModifierController.AddFloat("KA", BlinnPhongEffect.Parameters["KAmbient"], 0.2f, 0f, 1f);
            ModifierController.AddFloat("KD", BlinnPhongEffect.Parameters["KDiffuse"], 0.7f, 0f, 1f);
            ModifierController.AddFloat("KS", BlinnPhongEffect.Parameters["KSpecular"], 0.4f, 0f, 1f);
            ModifierController.AddFloat("Shininess", BlinnPhongEffect.Parameters["shininess"], 16.0f, 1f, 64f);
            ModifierController.AddColor("Ambient Color", BlinnPhongEffect.Parameters["ambientColor"], Color.LightGoldenrodYellow);
            ModifierController.AddColor("Diffuse Color", BlinnPhongEffect.Parameters["diffuseColor"], Color.LightGoldenrodYellow);
            ModifierController.AddColor("Specular Color", BlinnPhongEffect.Parameters["specularColor"], Color.White);

            ModifierController.AddColor("Color", BloomEffect.Parameters["Color"], Color.White);
            ModifierController.AddFloat("Threshold", BloomEffect.Parameters["Threshold"], 1f, 0f, 10f);
            ModifierController.AddFloat("Intensity", BloomEffect.Parameters["Intensity"], 1f, 0f, 10f);
            
            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            CubePrimitive = new CubePrimitive(GraphicsDevice, 1f, Color.White);

            // Create render targets. 
            // MainRenderTarget is used to store the scene color, allowing for colors to go beyond 0-1
            SceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);


            // BloomRenderTarget is used to store the bloom color and switches with MultipassBloomRenderTarget
            // depending on the pass count, to blur the bloom color


            var resolutionWidth = GraphicsDevice.Viewport.Width / 2;
            var resolutionHeight = GraphicsDevice.Viewport.Height / 2;

            DownSampleRenderTargets = new RenderTarget2D[DownSampleCount];
            UpSampleRenderTargets = new RenderTarget2D[DownSampleCount - 1];

            for (var index = 0; index < DownSampleCount; index++)
            {
                DownSampleRenderTargets[index] = new RenderTarget2D(GraphicsDevice, resolutionWidth,
                      resolutionHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0,
                      RenderTargetUsage.DiscardContents);


                resolutionWidth /= 2;
                resolutionHeight /= 2;
            }

            resolutionWidth *= 4;
            resolutionHeight *= 4;

            for (var index = 0; index < DownSampleCount - 1; index++)
            {
                UpSampleRenderTargets[index] = new RenderTarget2D(GraphicsDevice, resolutionWidth,
                      resolutionHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None, 0,
                      RenderTargetUsage.DiscardContents);

                resolutionWidth *= 2;
                resolutionHeight *= 2;
            }


            ModifierController.AddToggle("Effect Active", (toggle) => EffectOn = toggle, true);
            ModifierController.AddTexture("Scene Render Target", SceneRenderTarget);
            for(var index = 0; index < DownSampleCount; index++)
                ModifierController.AddTexture("Downsample Render Target " + index, DownSampleRenderTargets[index]);
            for (var index = 0; index < DownSampleCount - 1; index++)
                ModifierController.AddTexture("Upsample Render Target " + index, UpSampleRenderTargets[index]);


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            base.LoadContent();
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);


            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }


        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (EffectOn)
                DrawBloom(gameTime);
            else
                DrawRegular();

            base.Draw(gameTime);
        }


        private void DrawBloom(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            var viewProjection = Camera.View * Camera.Projection;

            // Set the main render target, here we'll draw the base scene
            GraphicsDevice.SetRenderTarget(SceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);


            ModelDrawer.Draw(Matrix.CreateScale(0.01f), viewProjection);

            var cubeWorld = Matrix.CreateScale(30f) * Matrix.CreateTranslation(Vector3.Up * 100f);
            BloomEffect.CurrentTechnique = BloomEffect.Techniques["RandomEffect"];
            BloomEffect.Parameters["Time"]?.SetValue(time);
            BloomEffect.Parameters["WorldViewProjection"].SetValue(cubeWorld * viewProjection);
            BloomEffect.Parameters["World"].SetValue(cubeWorld);
            CubePrimitive.Draw(BloomEffect);






            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SetRenderTarget(DownSampleRenderTargets[0]);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            var currentResolutionWidth = GraphicsDevice.Viewport.Width;
            var currentResolutionHeight = GraphicsDevice.Viewport.Height;


            BloomEffect.CurrentTechnique = BloomEffect.Techniques["ExtractDownsample"];
            BloomEffect.Parameters["baseTexture"].SetValue(SceneRenderTarget);
            BloomEffect.Parameters["TexelSize"].SetValue(new Vector2(1f / currentResolutionWidth, 1f / currentResolutionHeight));

            FullScreenQuad.Draw(BloomEffect);
            

            BloomEffect.CurrentTechnique = BloomEffect.Techniques["Downsample"];
            for (var index = 1; index < DownSampleCount; index++)
            {
                currentResolutionWidth /= 2;
                currentResolutionHeight /= 2;

                GraphicsDevice.SetRenderTarget(DownSampleRenderTargets[index]);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                BloomEffect.Parameters["baseTexture"].SetValue(DownSampleRenderTargets[index - 1]);
                BloomEffect.Parameters["TexelSize"].SetValue(new Vector2(1f / currentResolutionWidth, 1f / currentResolutionHeight));

                FullScreenQuad.Draw(BloomEffect);
            }



            BloomEffect.CurrentTechnique = BloomEffect.Techniques["UpsampleCombine"];
            BloomEffect.Parameters["baseTexture"].SetValue(DownSampleRenderTargets[DownSampleCount - 1]);
            BloomEffect.Parameters["bloomTexture"].SetValue(DownSampleRenderTargets[DownSampleCount - 2]);
            BloomEffect.Parameters["TexelSize"].SetValue(new Vector2(1f / currentResolutionWidth, 1f / currentResolutionHeight));

            GraphicsDevice.SetRenderTarget(UpSampleRenderTargets[0]);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            FullScreenQuad.Draw(BloomEffect);

            for (var index = 1; index < DownSampleCount - 1; index++)
            {
                GraphicsDevice.SetRenderTarget(UpSampleRenderTargets[index]);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                FullScreenQuad.Draw(BloomEffect);

                currentResolutionWidth *= 2;
                currentResolutionHeight *= 2;

                BloomEffect.Parameters["baseTexture"].SetValue(UpSampleRenderTargets[index - 1]);
                BloomEffect.Parameters["bloomTexture"].SetValue(DownSampleRenderTargets[DownSampleCount - index - 2]);
                BloomEffect.Parameters["TexelSize"].SetValue(new Vector2(1f / currentResolutionWidth, 1f / currentResolutionHeight));
            }


            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.Black);
            //GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0f, 0);

            BloomEffect.CurrentTechnique = BloomEffect.Techniques["UpsampleCombineTonemapping"];

            BloomEffect.Parameters["baseTexture"].SetValue(UpSampleRenderTargets[DownSampleCount - 2]);
            BloomEffect.Parameters["bloomTexture"].SetValue(SceneRenderTarget);
            BloomEffect.Parameters["TexelSize"].SetValue(new Vector2(1f / currentResolutionWidth, 1f / currentResolutionHeight));

            FullScreenQuad.Draw(BloomEffect);


        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var viewProjection = Camera.View * Camera.Projection;

            ModelDrawer.Draw(Matrix.CreateScale(0.01f), viewProjection);

            BlinnPhongEffect.Parameters["WorldViewProjection"].SetValue(viewProjection);
            BlinnPhongEffect.Parameters["World"].SetValue(Matrix.Identity);
            BlinnPhongEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(Matrix.Identity)));
            CubePrimitive.Draw(BlinnPhongEffect);
        }
    }
}
