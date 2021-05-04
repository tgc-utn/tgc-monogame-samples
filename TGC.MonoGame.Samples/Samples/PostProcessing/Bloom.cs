using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class Bloom : TGCSample
    {
        private const int PassCount = 2;

        private BasicEffect BasicEffect;

        private bool EffectOn = true;

        private RenderTarget2D FirstPassBloomRenderTarget;

        private FullScreenQuad FullScreenQuad;

        private RenderTarget2D MainSceneRenderTarget;

        private bool PastKeyPressed;

        private Matrix QuadBloomWorld;

        private Matrix QuadSceneWorld;

        private RenderTarget2D SecondPassBloomRenderTarget;

        private SpriteFont SpriteFont;

        /// <inheritdoc />
        public Bloom(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Bloom";
            Description = "Applying a Bloom post-process to a scene";
        }

        private FreeCamera Camera { get; set; }
        private Model Model { get; set; }

        private Effect Effect { get; set; }
        private Effect BlurEffect { get; set; }
        private Effect DebugTextureEffect { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-350, 50, 400), screenSize);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");
            BasicEffect = (BasicEffect)Model.Meshes[0].Effects[0];

            // Load the base bloom pass effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "Bloom");

            // Load the blur effect to blur the bloom texture
            BlurEffect = Game.Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");
            BlurEffect.Parameters["screenSize"]
                .SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            // Load the debug texture effect to visualize the bloom
            DebugTextureEffect = Game.Content.Load<Effect>(ContentFolderEffects + "DebugTexture");
            // Transform the quad to be in a smaller part of the screen
            QuadSceneWorld = Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(new Vector3(-0.75f, -0.75f, 0f));
            // Transform the quad to be in a smaller part of the screen
            QuadBloomWorld = Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(new Vector3(-0.3f, -0.75f, 0f));

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create render targets. 
            // MainRenderTarget is used to store the scene color
            // BloomRenderTarget is used to store the bloom color and switches with MultipassBloomRenderTarget
            // depending on the pass count, to blur the bloom color
            MainSceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            FirstPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            SecondPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            // Turn the effect on or off depending on the keyboard state
            var currentKeyPressed = Keyboard.GetState().IsKeyDown(Keys.J);
            if (!currentKeyPressed && PastKeyPressed)
                EffectOn = !EffectOn;
            PastKeyPressed = currentKeyPressed;

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (EffectOn)
                DrawBloom();
            else
                DrawRegular();

            Game.SpriteBatch.Begin();
            Game.SpriteBatch.DrawString(SpriteFont, "Con la tecla 'J' se prende y apaga el efecto", new Vector2(50, 50),
                Color.Black);
            Game.SpriteBatch.DrawString(SpriteFont, "Efecto " + (EffectOn ? "prendido" : "apagado"),
                new Vector2(50, 80), Color.Black);
            Game.SpriteBatch.End();

            
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var modelMesh in Model.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = BasicEffect;

            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);
        }

        /// <summary>
        ///     Draws the scene with a multiple-light bloom.
        /// </summary>
        private void DrawBloom()
        {
            #region Pass 1

            // Use the default blend and depth configuration
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Set the main render target, here we'll draw the base scene
            GraphicsDevice.SetRenderTarget(MainSceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            // Assign the basic effect and draw
            foreach (var modelMesh in Model.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = BasicEffect;
            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            #endregion

            #region Pass 2

            // Set the render target as our bloomRenderTarget, we are drawing the bloom color into this texture
            GraphicsDevice.SetRenderTarget(FirstPassBloomRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            Effect.CurrentTechnique = Effect.Techniques["BloomPass"];
            Effect.Parameters["baseTexture"].SetValue(BasicEffect.Texture);

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
                Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * Camera.View * Camera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            #endregion

            #region Multipass Bloom

            // Now we apply a blur effect to the bloom texture
            // Note that we apply this a number of times and we switch
            // the render target with the source texture
            // Basically, this applies the blur effect N times
            BlurEffect.CurrentTechnique = BlurEffect.Techniques["Blur"];

            var bloomTexture = FirstPassBloomRenderTarget;
            var finalBloomRenderTarget = SecondPassBloomRenderTarget;

            for (var index = 0; index < PassCount; index++)
            {
                //Exchange(ref SecondaPassBloomRenderTarget, ref FirstPassBloomRenderTarget);

                // Set the render target as null, we are drawing into the screen now!
                GraphicsDevice.SetRenderTarget(finalBloomRenderTarget);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                BlurEffect.Parameters["baseTexture"].SetValue(bloomTexture);
                FullScreenQuad.Draw(BlurEffect);

                if (index != PassCount - 1)
                {
                    var auxiliar = bloomTexture;
                    bloomTexture = finalBloomRenderTarget;
                    finalBloomRenderTarget = auxiliar;
                }
            }

            #endregion

            #region Final Pass

            // Set the depth configuration as none, as we don't use depth in this pass
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set the render target as null, we are drawing into the screen now!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Set the technique to our blur technique
            // Then draw a texture into a full-screen quad
            // using our rendertarget as texture
            Effect.CurrentTechnique = Effect.Techniques["Integrate"];
            Effect.Parameters["baseTexture"].SetValue(MainSceneRenderTarget);
            Effect.Parameters["bloomTexture"].SetValue(finalBloomRenderTarget);
            FullScreenQuad.Draw(Effect);

            #endregion

            // Debug our scene texture!
            // Show a simple quad with the texture
            DebugTextureEffect.Parameters["World"].SetValue(QuadSceneWorld);
            DebugTextureEffect.Parameters["baseTexture"].SetValue(MainSceneRenderTarget);
            FullScreenQuad.Draw(DebugTextureEffect);

            // Debug our bloom texture!
            // Show a simple quad with the texture
            DebugTextureEffect.Parameters["World"].SetValue(QuadBloomWorld);
            DebugTextureEffect.Parameters["baseTexture"].SetValue(finalBloomRenderTarget);
            FullScreenQuad.Draw(DebugTextureEffect);

            // Set the render targets as they were
            // Exchange(ref SecondPassBloomRenderTarget, ref FirstPassBloomRenderTarget);
        }

        private void Exchange(ref RenderTarget2D first, ref RenderTarget2D second)
        {
            var auxiliar = second;
            second = first;
            first = auxiliar;
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            FullScreenQuad.Dispose();
            FirstPassBloomRenderTarget.Dispose();
            MainSceneRenderTarget.Dispose();
            SecondPassBloomRenderTarget.Dispose();
        }
    }
}