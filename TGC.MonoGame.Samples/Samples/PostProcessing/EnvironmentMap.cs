using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class EnvironmentMap : TGCSample
    {
        private const int EnvironmentMapSize = 2048;

        /// <inheritdoc />
        public EnvironmentMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Environment Map";
            Description = "Render an environment map from the scene and use it for reflections";
        }

        private FreeCamera _camera;

        private StaticCamera _cubeMapCamera;

        private Model _scene;

        private Model _robot;

        private SpherePrimitive _sphere;

        private Effect _effect;

        private BasicEffect _basicEffect;

        private RenderTargetCube _environmentMapRenderTarget;

        private bool _effectOn = true;

        private Vector3 _robotPosition = Vector3.UnitX * -500f;

        private Vector3 _spherePosition = Vector3.UnitX * -500f + Vector3.UnitZ * -500f;

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-250, 100, 700), screenSize);

            _cubeMapCamera = new StaticCamera(1f, _robotPosition, Vector3.UnitX, Vector3.Up);
            _cubeMapCamera.BuildProjection(1f, 1f, 3000f, MathHelper.PiOver2);
            
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            _scene = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            _sphere = new SpherePrimitive(GraphicsDevice, 100f, 16, Color.White);

            // Load the tank which will contain reflections
            _robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

            // Load the shadowmap effect
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "EnvironmentMap");

            _basicEffect = (BasicEffect) _robot.Meshes.FirstOrDefault().Effects[0];
            _basicEffect.LightingEnabled = false;
            _sphere.Effect.LightingEnabled = false;

            // Assign the Environment map effect to our robot
            foreach (var modelMesh in _robot.Meshes)
            foreach (var part in modelMesh.MeshParts)
                part.Effect = _effect;

            // Create a render target for the scene
            _environmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, EnvironmentMapSize, false,
                SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            GraphicsDevice.BlendState = BlendState.Opaque;

            ModifierController.AddToggle("Effect On", OnEffectEnable, true);

            base.LoadContent();
        }

        /// <summary>
        ///     Processes the toggling of the Effect
        /// </summary>
        /// <param name="enabled">A boolean indicating if the Effect is on</param>
        private void OnEffectEnable(bool enabled)
        {
            var effectToAssign = enabled ? _effect : _basicEffect;            
            foreach (var modelMesh in _robot.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = effectToAssign;
            _effectOn = enabled;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            _cubeMapCamera.Position = _robotPosition;

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (_effectOn)
                DrawEnvironmentMap();
            else
                DrawRegular();

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _scene.Draw(Matrix.Identity, _camera.View, _camera.Projection);

            _sphere.Draw(Matrix.CreateTranslation(_spherePosition), _camera.View, _camera.Projection);

            _robot.Draw(Matrix.CreateTranslation(_robotPosition), _camera.View, _camera.Projection);
        }

        /// <summary>
        ///     Draws the scene with an environment map.
        /// </summary>
        private void DrawEnvironmentMap()
        {
            #region Pass 1-6

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Draw to our cubemap from the robot position
            for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ; face++)
            {
                // Set the render target as our cubemap face, we are drawing the scene in this texture
                GraphicsDevice.SetRenderTarget(_environmentMapRenderTarget, face);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

                SetCubemapCameraForOrientation(face);
                _cubeMapCamera.BuildView();

                // Draw our scene. Do not draw our tank as it would be occluded by itself 
                // (if it has backface culling on)
                _scene.Draw(Matrix.Identity, _cubeMapCamera.View, _cubeMapCamera.Projection);
            }

            #endregion

            #region Pass 7

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);


            // Draw our scene with the default effect and default camera
            _scene.Draw(Matrix.Identity, _camera.View, _camera.Projection);

            // Draw our sphere

            #region Draw Sphere

            _effect.CurrentTechnique = _effect.Techniques["EnvironmentMapSphere"];
            _effect.Parameters["environmentMap"].SetValue(_environmentMapRenderTarget);
            _effect.Parameters["eyePosition"].SetValue(_camera.Position);

            var sphereWorld = Matrix.CreateTranslation(_spherePosition);

            // World is used to transform from model space to world space
            _effect.Parameters["World"].SetValue(sphereWorld);
            // InverseTransposeWorld is used to rotate normals
            _effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(sphereWorld)));
            // WorldViewProjection is used to transform from model space to clip space
            _effect.Parameters["WorldViewProjection"].SetValue(sphereWorld * _camera.View * _camera.Projection);

            _sphere.Draw(_effect);

            #endregion


            #region Draw Robot

            // Set up our Effect to draw the robot
            _effect.CurrentTechnique = _effect.Techniques["EnvironmentMap"];
            _effect.Parameters["baseTexture"].SetValue(_basicEffect.Texture);

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[_robot.Bones.Count];
            _robot.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            var worldMatrix = Matrix.CreateTranslation(_robotPosition);
            // World is used to transform from model space to world space
            _effect.Parameters["World"].SetValue(worldMatrix);
            // InverseTransposeWorld is used to rotate normals
            _effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));

            // WorldViewProjection is used to transform from model space to clip space
            _effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * _camera.View * _camera.Projection);

            _robot.Meshes.FirstOrDefault().Draw();

            #endregion

            #endregion
        }

        /// <summary>
        ///     Sets the camera orientation based on the cubemap face.
        /// </summary>
        private void SetCubemapCameraForOrientation(CubeMapFace face)
        {
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    _cubeMapCamera.FrontDirection = -Vector3.UnitX;
                    _cubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    _cubeMapCamera.FrontDirection = Vector3.UnitX;
                    _cubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    _cubeMapCamera.FrontDirection = Vector3.Down;
                    _cubeMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    _cubeMapCamera.FrontDirection = Vector3.Up;
                    _cubeMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    _cubeMapCamera.FrontDirection = -Vector3.UnitZ;
                    _cubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    _cubeMapCamera.FrontDirection = Vector3.UnitZ;
                    _cubeMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            base.UnloadContent();
            _environmentMapRenderTarget.Dispose();
        }
    }
}