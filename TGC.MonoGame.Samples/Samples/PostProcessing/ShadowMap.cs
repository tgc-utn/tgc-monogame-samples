using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class ShadowMap : TGCSample
    {
        private const int ShadowmapSize = 2048;

        private const float LightCameraFarPlaneDistance = 3000f;

        private const float LightCameraNearPlaneDistance = 5f;

        private bool _effectOn = true;

        private FullScreenQuad _fullScreenQuad;

        private CubePrimitive _lightBox;

        private Vector3 _lightPosition = Vector3.One * 500f;

        private RenderTarget2D _shadowMapRenderTarget;

        private float _timer;

        private FreeCamera _camera;

        private TargetCamera _targetLightCamera;

        private Model _model;

        private Effect _effect;

        private BasicEffect _basicEffect;

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
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-600, 250, 1500), screenSize);
            _camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 0.1f, 3000f, MathHelper.PiOver4);

            _targetLightCamera = new TargetCamera(1f, _lightPosition, Vector3.Zero);
            _targetLightCamera.BuildProjection(1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance,
                MathHelper.PiOver2);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            _model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // Load the shadowmap effect
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "ShadowMap");

            _basicEffect = (BasicEffect)_model.Meshes.FirstOrDefault()?.Effects.FirstOrDefault();

            // Create a full screen quad to post-process
            _fullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a shadow map. It stores depth from the light position
            _shadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            _lightBox = new CubePrimitive(GraphicsDevice, 5, Color.White);

            GraphicsDevice.BlendState = BlendState.Opaque;

            ModifierController.AddToggle("Effect Active", (toggle) => _effectOn = toggle, true);
            ModifierController.AddTexture("Shadow Map", _shadowMapRenderTarget);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            UpdateLightPosition((float)gameTime.ElapsedGameTime.TotalSeconds);

            _targetLightCamera.Position = _lightPosition;
            _targetLightCamera.BuildView();

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        private void UpdateLightPosition(float elapsedTime)
        {
            _lightPosition = new Vector3(MathF.Cos(_timer) * 1000f, 500f, MathF.Sin(_timer) * 1000f);
            _timer += elapsedTime * 0.5f;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (_effectOn)
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

            foreach (var modelMesh in _model.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = _basicEffect;

            _model.Draw(Matrix.Identity, _camera.View, _camera.Projection);
        }

        /// <summary>
        ///     Draws the scene with shadows.
        /// </summary>
        private void DrawShadows()
        {
            #region Pass 1

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(_shadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            _effect.CurrentTechnique = _effect.Techniques["DepthPass"];

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
                _effect.Parameters["WorldViewProjection"]
                    .SetValue(worldMatrix * _targetLightCamera.View * _targetLightCamera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            #endregion

            #region Pass 2

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            _effect.CurrentTechnique = _effect.Techniques["DrawShadowedPCF"];
            _effect.Parameters["baseTexture"].SetValue(_basicEffect.Texture);
            _effect.Parameters["shadowMap"].SetValue(_shadowMapRenderTarget);
            _effect.Parameters["lightPosition"].SetValue(_lightPosition);
            _effect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            _effect.Parameters["LightViewProjection"].SetValue(_targetLightCamera.View * _targetLightCamera.Projection);
            foreach (var modelMesh in _model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = _effect;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                _effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * _camera.View * _camera.Projection);
                _effect.Parameters["World"].SetValue(worldMatrix);
                _effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            _lightBox.Draw(Matrix.CreateTranslation(_lightPosition), _camera.View, _camera.Projection);

            #endregion
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            base.UnloadContent();
            _fullScreenQuad.Dispose();
            _shadowMapRenderTarget.Dispose();
        }
    }
}