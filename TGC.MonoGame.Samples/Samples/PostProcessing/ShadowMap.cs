using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class ShadowMap : TGCSample
    {
        private const int ShadowmapSize = 2048;

        private readonly float LightCameraFarPlaneDistance = 3000f;

        private readonly float LightCameraNearPlaneDistance = 5f;

        private bool EffectOn = true;

        private FullScreenQuad FullScreenQuad;

        private CubePrimitive LightBox;

        private Vector3 LightPosition = Vector3.One * 500f;

        private RenderTarget2D ShadowMapRenderTarget;

        private float Timer;

        private FreeCamera Camera { get; set; }

        private TargetCamera TargetLightCamera { get; set; }

        private ModelDrawer ModelDrawer { get; set; }
        private ModelDrawer ModelDrawerDepth { get; set; }

        private ModelDrawer ModelDrawerReceiveShadows { get; set; }

        private Effect Effect { get; set; }
        private Effect DiffuseEffect { get; set; }
        private Effect VertexColorEffect { get; set; }


        /// <inheritdoc />
        public ShadowMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Shadow Map";
            Description = "Projecting shadows in a scene";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-600, 250, 1500), screenSize);
            Camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 0.1f, 3000f, MathHelper.PiOver4);

            TargetLightCamera = new TargetCamera(1f, LightPosition, Vector3.Zero);
            TargetLightCamera.BuildProjection(1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance,
                MathHelper.Pi * 2f / 3f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            var model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");


            // Load the shadowmap effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "ShadowMap");

            DiffuseEffect = Game.Content.Load<Effect>(ContentFolderEffects + "DiffuseTexture");

            VertexColorEffect = Game.Content.Load<Effect>(ContentFolderEffects + "VertexColor");





            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a shadow map. It stores depth from the light position
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            ModelDrawer = ModelInspector.CreateDrawerFrom(model, DiffuseEffect, EffectInspectionType.ALL);

            ModelDrawerDepth = ModelDrawer.Clone(false, true);
            ModelDrawerDepth.SetEffect(Effect, Effect.Techniques["DepthPass"], EffectInspectionType.MATRICES);

            ModelDrawerReceiveShadows = ModelDrawer.Clone(false, true);
            ModelDrawerReceiveShadows.SetEffect(Effect, Effect.Techniques["DrawShadowedPCF"], EffectInspectionType.ALL);

            LightBox = new CubePrimitive(GraphicsDevice, 5, Color.White);
            LightBox.SetEffect(VertexColorEffect, EffectInspectionType.MATRICES);

            GraphicsDevice.BlendState = BlendState.Opaque;


            ModifierController.AddToggle("Effect Active", (toggle) => EffectOn = toggle, true);
            ModifierController.AddTexture("Shadow Map", ShadowMapRenderTarget);



            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            UpdateLightPosition((float)gameTime.ElapsedGameTime.TotalSeconds);

            TargetLightCamera.Position = LightPosition;
            TargetLightCamera.BuildView();

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        private void UpdateLightPosition(float elapsedTime)
        {
            LightPosition = new Vector3(MathF.Cos(Timer) * 1000f, 500f, MathF.Sin(Timer) * 1000f);
            Timer += elapsedTime * 0.5f;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (EffectOn)
                DrawShadows();
            else
                DrawRegular();

            
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var viewProjection = Camera.View * Camera.Projection;
            ModelDrawer.ViewProjection = viewProjection;
            ModelDrawer.Draw();
        }


        /// <summary>
        ///     Draws the scene with shadows.
        /// </summary>
        private void DrawShadows()
        {
            #region Pass 1

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            var lightViewProjection = TargetLightCamera.View * TargetLightCamera.Projection;

            ModelDrawerDepth.ViewProjection = lightViewProjection;
            ModelDrawerDepth.Draw();

            #endregion

            #region Pass 2
            
            var viewProjection = Camera.View * Camera.Projection;

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            Effect.CurrentTechnique = Effect.Techniques["DrawShadowedPCF"];
            Effect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            Effect.Parameters["lightPosition"].SetValue(LightPosition);
            Effect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            Effect.Parameters["LightViewProjection"].SetValue(lightViewProjection);

            ModelDrawerReceiveShadows.ViewProjection = viewProjection;
            ModelDrawerReceiveShadows.Draw();


            LightBox.ViewProjection = Camera.View * Camera.Projection;
            LightBox.World = Matrix.CreateTranslation(LightPosition);
            LightBox.Draw();

            #endregion
        }


        /// <inheritdoc />
        protected override void UnloadContent()
        {
            base.UnloadContent();
            FullScreenQuad.Dispose();
            ShadowMapRenderTarget.Dispose();
        }
    }
}