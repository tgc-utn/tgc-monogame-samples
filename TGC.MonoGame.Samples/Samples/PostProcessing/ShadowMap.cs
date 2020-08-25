using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class ShadowMap : TGCSample
    {

        private const int SHADOWMAP_SIZE = 2048;

        private FreeCamera Camera { get; set; }

        private StaticCamera StaticLightCamera { get; set; }

        private Model Model { get; set; }

        private Effect Effect { get; set; }

        private BasicEffect BasicEffect { get; set; }

        private Effect DebugTextureEffect { get; set; }

        private RenderTarget2D ShadowMapRenderTarget;

        private FullScreenQuad FullScreenQuad;

        private SpriteFont SpriteFont;

        private bool EffectOn = true;

        private Matrix QuadShadowsWorld;

        private bool PastKeyPressed = false;

        private Vector3 LightPosition = Vector3.One * 500f;

        private BoxPrimitive LightBox;

        private float Timer = 0f;

        private float LightCameraNearPlaneDistance = 5f;

        private float LightCameraFarPlaneDistance = 3000f;

        /// <inheritdoc />
        public ShadowMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcess;
            Name = "Shadow Map";
            Description = "Projecting shadows in a scene";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            Point screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(700, 700, 700), screenSize);
            Camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, MathHelper.PiOver4, 0.1f, 3000f);

            StaticLightCamera = new StaticCamera(1f, LightPosition, Vector3.Zero);
            StaticLightCamera.BuildProjection(1f, MathHelper.PiOver2, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // Load the shadowmap effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffect + "ShadowMap");

            BasicEffect = (BasicEffect)Model.Meshes.FirstOrDefault().Effects.FirstOrDefault();

            // Load the debug texture effect to visualize the shadow map
            DebugTextureEffect = Game.Content.Load<Effect>(ContentFolderEffect + "DebugTexture");
            // Assign the near and far plane distances of the light camera to debug depth
            DebugTextureEffect.Parameters["nearPlaneDistance"].SetValue(LightCameraNearPlaneDistance);
            DebugTextureEffect.Parameters["farPlaneDistance"].SetValue(LightCameraFarPlaneDistance);
            DebugTextureEffect.CurrentTechnique = DebugTextureEffect.Techniques["DebugDepth"];

            // Transform the quad to be in a smaller part of the screen
            QuadShadowsWorld = Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(new Vector3(-0.75f, -0.75f, 0f));

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a shadow map. It stores depth from the light position
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, SHADOWMAP_SIZE, SHADOWMAP_SIZE, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            LightBox = new BoxPrimitive(GraphicsDevice, Vector3.One * 5f, Vector3.Zero, Color.White);

            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");

            GraphicsDevice.BlendState = BlendState.Opaque;


            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            UpdateLightPosition((float)gameTime.ElapsedGameTime.TotalSeconds);

            StaticLightCamera.SetPosition(LightPosition);

            // Turn the effect on or off depending on the keyboard state
            var currentKeyPressed = Keyboard.GetState().IsKeyDown(Keys.J);
            if (!currentKeyPressed && PastKeyPressed)
                EffectOn = !EffectOn;
            PastKeyPressed = currentKeyPressed;

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


            Game.SpriteBatch.Begin();
            Game.SpriteBatch.DrawString(SpriteFont, "Con la tecla 'J' se prende y apaga el efecto", new Vector2(50, 50), Color.Black);
            Game.SpriteBatch.DrawString(SpriteFont, "Efecto " + (EffectOn ? "prendido" : "apagado"), new Vector2(50, 80), Color.Black);
            Game.SpriteBatch.End();

            AxisLines.Draw(Camera.View, Camera.Projection);
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var modelMesh in Model.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = BasicEffect;

            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);
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

            Effect.CurrentTechnique = Effect.Techniques["DepthPass"];

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var modelMesh in Model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = Effect;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * StaticLightCamera.View * StaticLightCamera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            #endregion

            #region Pass 2

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            Effect.CurrentTechnique = Effect.Techniques["DrawShadowedPCF"];
            Effect.Parameters["baseTexture"].SetValue(BasicEffect.Texture);
            Effect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            Effect.Parameters["lightPosition"].SetValue(LightPosition);
            Effect.Parameters["shadowMapSize"].SetValue(Vector2.One * SHADOWMAP_SIZE);
            Effect.Parameters["LightViewProjection"].SetValue(StaticLightCamera.View * StaticLightCamera.Projection);
            foreach (var modelMesh in Model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = Effect;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * Camera.View * Camera.Projection);
                Effect.Parameters["World"].SetValue(worldMatrix);
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            LightBox.Draw(Matrix.CreateTranslation(LightPosition), Camera.View, Camera.Projection);

            #endregion

            // Debug our shadowmap!
            // Show a simple quad with the texture
            DebugTextureEffect.Parameters["World"].SetValue(QuadShadowsWorld);
            DebugTextureEffect.Parameters["baseTexture"].SetValue(ShadowMapRenderTarget);
            FullScreenQuad.Draw(DebugTextureEffect);



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
