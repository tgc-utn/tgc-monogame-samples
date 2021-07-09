using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class ChromaticAberration : TGCSample
    {
        private FreeCamera Camera { get; set; }

        private Model Model { get; set; }

        private Effect Effect { get; set; }

        private FullScreenQuad FullScreenQuad { get; set; }

        private RenderTarget2D SceneRenderTarget { get; set; }


        /// <inheritdoc />
        public ChromaticAberration(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Chromatic Aberration";
            Description = "Chromatic Aberration post-processing using a Render Target";
        }


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

            // Load the Chromatic Aberration effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "ChromaticAberration");
            Effect.Parameters["ScreenDelta"].SetValue(new Vector2(1f / GraphicsDevice.Viewport.Width, 1f / GraphicsDevice.Viewport.Height));

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a render target for the scene
            SceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0,
                RenderTargetUsage.DiscardContents);

            GraphicsDevice.BlendState = BlendState.Opaque;

            Modifiers = new IModifier[]
            {
                new OptionsModifier("Chromatic Aberration Type", new string[]{"Basic", "Centered", "Centered Directional" }, 0, OnChangeChromaticType),
                new FloatModifier("Displacement", Effect.Parameters["Displacement"], 1f, 0f, 30f),
            };
            
            base.LoadContent();
        }

        /// <summary>
        ///     Processes a change in the Chromatic Aberration Type, setting the right Technique in the Shader
        /// </summary>
        /// <param name="index">The index of the option</param>
        /// <param name="name">The name of the option</param>
        private void OnChangeChromaticType(int index, string name)
        {
            switch(index)
            {
                case 0:
                    Effect.CurrentTechnique = Effect.Techniques["BasicChromaticAberration"];
                    break;
                case 1:
                    Effect.CurrentTechnique = Effect.Techniques["CenteredChromaticAberration"];
                    break;
                case 2:
                    Effect.CurrentTechnique = Effect.Techniques["CenteredDirectionalChromaticAberration"];
                    break;
            }
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            #region Pass 1

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target, we are drawing into this texture
            GraphicsDevice.SetRenderTarget(SceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            // Draw the scene to the Render Target
            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            #endregion

            #region Pass 2

            // No depth needed
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            // Set the render target to null, we are drawing to the screen
            GraphicsDevice.SetRenderTarget(null);

            // Draw the Full Screen Quad with our Post-Process Effect
            Effect.Parameters["BaseTexture"].SetValue(SceneRenderTarget);
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