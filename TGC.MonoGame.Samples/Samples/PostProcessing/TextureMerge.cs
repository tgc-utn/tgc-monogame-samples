using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class TextureMerge : TGCSample
    {
        /// <inheritdoc />
        public TextureMerge(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Texture Merge";
            Description = "Merging the scene render target with a texture";
        }

        private FreeCamera Camera { get; set; }

        private Model Model { get; set; }

        private Texture2D Overlay { get; set; }

        private Effect Effect { get; set; }

        private FullScreenQuad FullScreenQuad { get; set; }

        private RenderTarget2D SceneRenderTarget { get; set; }

        private float Time { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-300, 100, 0), screenSize);

            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // Load the texture overlay to merge
            Overlay = Game.Content.Load<Texture2D>(ContentFolderTextures + "overlay");

            // Load the shadowmap effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureMerge");
            //Effect.Parameters["overlayTexture"].SetValue(Overlay);

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a render target for the scene
            SceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0,
                RenderTargetUsage.DiscardContents);

            ModifierController.AddTexture("Scene Render Target", SceneRenderTarget);




            Effect.Parameters["Displacement"].SetValue(5f);


            Effect.Parameters["InverseScreenSize"].SetValue(new Vector2(1f / GraphicsDevice.Viewport.Width, 1f / GraphicsDevice.Viewport.Height));

            GraphicsDevice.BlendState = BlendState.Opaque;

            base.LoadContent();
        }
        
        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            Time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Effect.Parameters["time"].SetValue(Time);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }
        
        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            #region Pass 1

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(SceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            #endregion
            
            #region Pass 2

            // No depth needed
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            // Set the render target to null, we are drawing to the screen
            GraphicsDevice.SetRenderTarget(null);

            Effect.Parameters["baseTexture"].SetValue(SceneRenderTarget);
            FullScreenQuad.Draw(Effect);

            #endregion

            
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            FullScreenQuad.Dispose();
            SceneRenderTarget.Dispose();
        }
    }
}