using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class Bloom : TGCSample
    {
        private const int PassCount = 2;

        private BasicEffect _basicEffect;

        private bool _effectOn = true;

        private RenderTarget2D _firstPassBloomRenderTarget;

        private FullScreenQuad _fullScreenQuad;

        private RenderTarget2D _mainSceneRenderTarget;

        private RenderTarget2D _secondPassBloomRenderTarget;

        /// <inheritdoc />
        public Bloom(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Bloom";
            Description = "Applying a Bloom post-process to a scene";
        }

        private FreeCamera _camera;
        private Model _model;

        private Effect _effect;
        private Effect _blurEffect;

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-350, 50, 400), screenSize);
            
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            _model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");
            _basicEffect = (BasicEffect)_model.Meshes[0].Effects[0];

            // Load the base bloom pass effect
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "Bloom");

            // Load the blur effect to blur the bloom texture
            _blurEffect = Game.Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");
            _blurEffect.Parameters["screenSize"]
                .SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            // Create a full screen quad to post-process
            _fullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create render targets. 
            // MainRenderTarget is used to store the scene color
            // BloomRenderTarget is used to store the bloom color and switches with MultipassBloomRenderTarget
            // depending on the pass count, to blur the bloom color
            _mainSceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _firstPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            _secondPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            ModifierController.AddToggle("Effect Active", (toggle) => _effectOn = toggle, true);
            ModifierController.AddTexture("Scene Render Target", _mainSceneRenderTarget);
            ModifierController.AddTexture("Bloom Render Target", _secondPassBloomRenderTarget);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);
            
            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (_effectOn)
                DrawBloom();
            else
                DrawRegular();
            
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var modelMesh in _model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                {
                    part.Effect = _basicEffect;
                }
            }

            _model.Draw(Matrix.Identity, _camera.View, _camera.Projection);
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
            GraphicsDevice.SetRenderTarget(_mainSceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            // Assign the basic effect and draw
            foreach (var modelMesh in _model.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = _basicEffect;
            _model.Draw(Matrix.Identity, _camera.View, _camera.Projection);

            #endregion

            #region Pass 2

            // Set the render target as our bloomRenderTarget, we are drawing the bloom color into this texture
            GraphicsDevice.SetRenderTarget(_firstPassBloomRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            _effect.CurrentTechnique = _effect.Techniques["BloomPass"];
            _effect.Parameters["baseTexture"].SetValue(_basicEffect.Texture);

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var modelMesh in _model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = _effect;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                _effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * _camera.View * _camera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            #endregion

            #region Multipass Bloom

            // Now we apply a blur effect to the bloom texture
            // Note that we apply this a number of times and we switch
            // the render target with the source texture
            // Basically, this applies the blur effect N times
            _blurEffect.CurrentTechnique = _blurEffect.Techniques["Blur"];

            var bloomTexture = _firstPassBloomRenderTarget;
            var finalBloomRenderTarget = _secondPassBloomRenderTarget;

            for (var index = 0; index < PassCount; index++)
            {
                //Exchange(ref SecondaPassBloomRenderTarget, ref FirstPassBloomRenderTarget);

                // Set the render target as null, we are drawing into the screen now!
                GraphicsDevice.SetRenderTarget(finalBloomRenderTarget);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                _blurEffect.Parameters["baseTexture"].SetValue(bloomTexture);
                _fullScreenQuad.Draw(_blurEffect);

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
            _effect.CurrentTechnique = _effect.Techniques["Integrate"];
            _effect.Parameters["baseTexture"].SetValue(_mainSceneRenderTarget);
            _effect.Parameters["bloomTexture"].SetValue(finalBloomRenderTarget);
            _fullScreenQuad.Draw(_effect);

            #endregion
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            _fullScreenQuad.Dispose();
            _firstPassBloomRenderTarget.Dispose();
            _mainSceneRenderTarget.Dispose();
            _secondPassBloomRenderTarget.Dispose();
        }
    }
}