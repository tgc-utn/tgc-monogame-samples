using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class EnvironmentMap : TGCSample
    {
        private const int ENVIRONMENTMAP_SIZE = 2048;

        private FreeCamera Camera { get; set; }

        private UnrestrictedCamera CubeMapCamera { get; set; }

        private Model Scene { get; set; }

        private Model Robot { get; set; }


        private SpherePrimitive Sphere { get; set; }

        private Effect Effect { get; set; }

        private BasicEffect BasicEffect { get; set; }

        private Effect DebugTextureEffect { get; set; }

        private FullScreenQuad FullScreenQuad { get; set; }

        private RenderTargetCube EnvironmentMapRenderTarget { get; set; }

        private Matrix QuadWorld;

        private bool EffectOn { get; set; } = true;

        private Vector3 RobotPosition { get; set; } = Vector3.UnitX * -500f;

        private Vector3 SpherePosition { get; set; } = Vector3.UnitX * -500f + Vector3.UnitZ * -500f;

        private bool PastKeyPressed { get; set; } = false;

        /// <inheritdoc />
        public EnvironmentMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcess;
            Name = "Environment Map";
            Description = "Render an environment map from the scene and use it for reflections";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            Point screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(700, 700, 700), screenSize);
            Camera.MovementSpeed *= 2f;

            CubeMapCamera = new UnrestrictedCamera(1f);
            CubeMapCamera.Position = RobotPosition;
            CubeMapCamera.BuildProjection(1f, MathHelper.PiOver2, 1f, 3000f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Scene = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            Sphere = new SpherePrimitive(GraphicsDevice, 100f, 16, Color.White);

            // Load the tank which will contain reflections
            Robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");


            // Load the shadowmap effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffect + "EnvironmentMap");

            BasicEffect = (BasicEffect)Robot.Meshes.FirstOrDefault().Effects[0];

            // Assign the Environment map effect to our robot
            foreach (var modelMesh in Robot.Meshes)
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = Effect;

            DebugTextureEffect = Game.Content.Load<Effect>(ContentFolderEffect + "DebugTexture");
            DebugTextureEffect.CurrentTechnique = DebugTextureEffect.Techniques["DebugCubeMap"];

            // Create a full screen quad to debug the environment map
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            QuadWorld = Matrix.CreateScale(new Vector3(0.9f, 0.2f, 0f)) * Matrix.CreateTranslation(Vector3.Down * 0.7f);

            // Create a render target for the scene
            EnvironmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, ENVIRONMENTMAP_SIZE, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            GraphicsDevice.BlendState = BlendState.Opaque;

            base.LoadContent();
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            CubeMapCamera.Position = RobotPosition;

            // Turn the effect on or off depending on the keyboard state
            var currentKeyPressed = Keyboard.GetState().IsKeyDown(Keys.J);
            if (!currentKeyPressed && PastKeyPressed)
            {
                EffectOn = !EffectOn;
                if(EffectOn)
                    foreach (var modelMesh in Robot.Meshes)
                        foreach (var part in modelMesh.MeshParts)
                            part.Effect = Effect;
                else
                    foreach (var modelMesh in Robot.Meshes)
                        foreach (var part in modelMesh.MeshParts)
                            part.Effect = BasicEffect;
            }
            PastKeyPressed = currentKeyPressed;

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


            Scene.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            Robot.Draw(Matrix.CreateTranslation(RobotPosition), Camera.View, Camera.Projection);
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
                CubeMapCamera.Update();

                // Draw our scene. Do not draw our tank as it would be occluded by itself 
                // (if it has backface culling on)
                Scene.Draw(Matrix.Identity, CubeMapCamera.View, CubeMapCamera.Projection);
            }

            #endregion

            #region Pass 7

            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);


            // Draw our scene with the default effect and default camera
            Scene.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            // Draw our sphere

            #region Draw Sphere

            Effect.CurrentTechnique = Effect.Techniques["EnvironmentMapSphere"];
            Effect.Parameters["environmentMap"].SetValue(EnvironmentMapRenderTarget);
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);

            var sphereWorld = Matrix.CreateTranslation(SpherePosition);

            // World is used to transform from model space to world space
            Effect.Parameters["World"].SetValue(sphereWorld);
            // InverseTransposeWorld is used to rotate normals
            Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(sphereWorld)));
            // WorldViewProjection is used to transform from model space to clip space
            Effect.Parameters["WorldViewProjection"].SetValue(sphereWorld * Camera.View * Camera.Projection);

            Sphere.Draw(Effect);


            #endregion



            #region Draw Robot

            // Set up our Effect to draw the robot
            Effect.CurrentTechnique = Effect.Techniques["EnvironmentMap"];
            Effect.Parameters["baseTexture"].SetValue(BasicEffect.Texture);

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[Robot.Bones.Count];
            Robot.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            var worldMatrix = Matrix.CreateTranslation(RobotPosition);
            // World is used to transform from model space to world space
            Effect.Parameters["World"].SetValue(worldMatrix);
            // InverseTransposeWorld is used to rotate normals
            Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));

            // WorldViewProjection is used to transform from model space to clip space
            Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * Camera.View * Camera.Projection);

            Robot.Meshes.FirstOrDefault().Draw();

            #endregion


            // Debug our cubemap!
            // Show a quad
            DebugTextureEffect.Parameters["World"].SetValue(QuadWorld);
            DebugTextureEffect.Parameters["cubeMapTexture"]?.SetValue(EnvironmentMapRenderTarget);
            FullScreenQuad.Draw(DebugTextureEffect);


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
    }
}
