using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class EnvironmentMap : TGCSample
    {
        private const int EnvironmentmapSize = 2048;

        /// <inheritdoc />
        public EnvironmentMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Environment Map";
            Description = "Render an environment map from the scene and use it for reflections";
        }

        private FreeCamera Camera { get; set; }

        private StaticCamera CubeMapCamera { get; set; }

        private ModelDrawer Scene { get; set; }

        private ModelDrawer Robot { get; set; }

        private SpherePrimitive Sphere { get; set; }

        private Effect Effect { get; set; }
        private Effect DiffuseEffect { get; set; }

        private RenderTargetCube EnvironmentMapRenderTarget { get; set; }

        private bool EffectOn { get; set; } = true;

        private Vector3 RobotPosition { get; } = Vector3.UnitX * -500f;

        private Vector3 SpherePosition { get; } = Vector3.UnitX * -500f + Vector3.UnitZ * -500f;

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-250, 100, 700), screenSize);

            CubeMapCamera = new StaticCamera(1f, RobotPosition, Vector3.UnitX, Vector3.Up);
            CubeMapCamera.BuildProjection(1f, 0.1f, 30000f, MathHelper.PiOver2);


            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            var sceneModel = Game.Content.Load<Model>(ContentFolder3D + "scene/city");
            var robotModel = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            

            // Load the Environment Map effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "EnvironmentMap");

            // Load the diffuse effect
            DiffuseEffect = Game.Content.Load<Effect>(ContentFolderEffects + "DiffuseTexture");

            Scene = ModelInspector.CreateDrawerFrom(sceneModel, DiffuseEffect, EffectInspectionType.ALL);
            Robot = ModelInspector.CreateDrawerFrom(robotModel, Effect, Effect.Techniques["EnvironmentMap"], EffectInspectionType.ALL);

            Sphere = new SpherePrimitive(GraphicsDevice, 100f, 16, Color.White);
            Sphere.SetEffect(Effect, Effect.Techniques["EnvironmentMapSphere"], EffectInspectionType.MATRICES);
            Sphere.World = Matrix.CreateTranslation(SpherePosition);

            // Create a render target for the scene
            EnvironmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, EnvironmentmapSize, false,
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
            var effectToAssign = enabled ? Effect : DiffuseEffect;
            Robot.SetEffect(effectToAssign, EffectInspectionType.ALL);
            EffectOn = enabled;
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            CubeMapCamera.Position = RobotPosition;

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            if (EffectOn)
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

            var viewProjection = Camera.View * Camera.Projection;
            Scene.ViewProjection = viewProjection;
            Scene.Draw();

            Robot.World = Matrix.CreateTranslation(RobotPosition);
            Robot.ViewProjection = viewProjection;
            Robot.Draw();
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
                GraphicsDevice.SetRenderTarget(EnvironmentMapRenderTarget, face);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

                SetCubemapCameraForOrientation(face);
                CubeMapCamera.BuildView();

                // Draw our scene
                // (if it has backface culling on)
                Scene.ViewProjection = CubeMapCamera.View * CubeMapCamera.Projection;
                Scene.Draw();
            }

            #endregion

            #region Pass 7

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            Scene.ViewProjection = Camera.View * Camera.Projection;

            // Draw our scene with the default effect and default camera
            Scene.Draw();

            // Draw our sphere

            #region Draw Sphere

            Effect.Parameters["environmentMap"].SetValue(EnvironmentMapRenderTarget);
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);

            Sphere.ViewProjection = Camera.View * Camera.Projection;
            Sphere.Draw();

            #endregion


            #region Draw Robot

            Robot.World = Matrix.CreateTranslation(RobotPosition);
            Robot.ViewProjection = Camera.View * Camera.Projection;
            Robot.Draw();

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
                    CubeMapCamera.FrontDirection = -Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    CubeMapCamera.FrontDirection = Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    CubeMapCamera.FrontDirection = Vector3.Down;
                    CubeMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    CubeMapCamera.FrontDirection = Vector3.Up;
                    CubeMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    CubeMapCamera.FrontDirection = -Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    CubeMapCamera.FrontDirection = Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            base.UnloadContent();
            EnvironmentMapRenderTarget.Dispose();
        }
    }
}