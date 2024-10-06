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

        private FreeCamera _camera;

        private Model _model;

        private Texture2D _overlay;

        private Effect _effect;

        private FullScreenQuad _fullScreenQuad;

        private RenderTarget2D _sceneRenderTarget;

        private float _time;

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-300, 100, 0), screenSize);

            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            _model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // Load the texture overlay to merge
            _overlay = Game.Content.Load<Texture2D>(ContentFolderTextures + "overlay");

            // Load the shadowmap effect
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureMerge");
            _effect.Parameters["overlayTexture"].SetValue(_overlay);

            // Create a full screen quad to post-process
            _fullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a render target for the scene
            _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0,
                RenderTargetUsage.DiscardContents);

            GraphicsDevice.BlendState = BlendState.Opaque;

            base.LoadContent();
        }
        
        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _effect.Parameters["time"].SetValue(_time);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }
        
        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            #region Pass 1

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            _model.Draw(Matrix.Identity, _camera.View, _camera.Projection);

            #endregion
            
            #region Pass 2

            // No depth needed
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            // Set the render target to null, we are drawing to the screen
            GraphicsDevice.SetRenderTarget(null);

            _effect.Parameters["baseTexture"].SetValue(_sceneRenderTarget);
            _fullScreenQuad.Draw(_effect);

            #endregion
            
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            _fullScreenQuad.Dispose();
            _sceneRenderTarget.Dispose();
        }
    }
}