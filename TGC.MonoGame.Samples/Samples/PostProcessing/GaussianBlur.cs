﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class GaussianBlur : TGCSample
    {
        private BlurType CurrentBlurType;

        private FullScreenQuad FullScreenQuad;

        private RenderTarget2D HorizontalRenderTarget;

        private RenderTarget2D MainRenderTarget;

        private FreeCamera Camera { get; set; }
        private Model Model { get; set; }

        private Effect Effect { get; set; }

        /// <inheritdoc />
        public GaussianBlur(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Gaussian Blur";
            Description = "Applying a Gaussian Blur post-process to a scene";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-400, 50, 400), screenSize);
                        
            CurrentBlurType = BlurType.SEPARATED_PASSES;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // Load the post-processing effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create render targets. One can be used for simple gaussian blur
            // mainRenderTarget is also used as a render target in the separated filter
            // horizontalRenderTarget is used as the horizontal render target in the separated filter
            MainRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            HorizontalRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            Effect.Parameters["screenSize"]
                .SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            CurrentBlurType = BlurType.SEPARATED_PASSES;
           
            ModifierController.AddOptions("Blur Type",new string[]
            {
                "None",
                "Single Pass Blur",
                "Two Pass Separated Blur",
            }, BlurType.SEPARATED_PASSES, OnBlurTypeChange);

            base.LoadContent();
        }


        /// <summary>
        ///     Processes a change in the Blur Type
        /// </summary>
        /// <param name="index">The index of the Blur selected option</param>
        /// <param name="value">The name of the Blur type</param>
        private void OnBlurTypeChange(BlurType type)
        {
            CurrentBlurType = type;
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
            if (CurrentBlurType.Equals(BlurType.NONE))
                DrawRegular();
            else if (CurrentBlurType.Equals(BlurType.SIMPLE))
                DrawSimpleBlur();
            else
                DrawSeparatedBlur();

            
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draws the scene.
        /// </summary>
        private void DrawRegular()
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);
        }


        /// <summary>
        ///     Draws the scene with a simple, single-pass blur.
        /// </summary>
        private void DrawSimpleBlur()
        {
            #region Pass 1

            // Use the default blend and depth configuration
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Set the main render target as our render target
            GraphicsDevice.SetRenderTarget(MainRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            #endregion

            #region Pass 2

            // Set the depth configuration as none, as we don't use depth in this pass
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set the render target as null, we are drawing into the screen now!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Set the technique to our blur technique
            // Then draw a texture into a full-screen quad
            // using our rendertarget as texture

            Effect.CurrentTechnique = Effect.Techniques["Blur"];
            Effect.Parameters["baseTexture"].SetValue(MainRenderTarget);
            FullScreenQuad.Draw(Effect);

            #endregion
        }


        /// <summary>
        ///     Draws the scene with a separated blur, in a total of three passes.
        /// </summary>
        private void DrawSeparatedBlur()
        {
            #region Pass 1

            // Use the default blend and depth configuration
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Set the main render target as our render target
            GraphicsDevice.SetRenderTarget(MainRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            Model.Draw(Matrix.Identity, Camera.View, Camera.Projection);

            #endregion

            #region Pass 2

            // Set the depth configuration as none, as we don't use depth in this pass
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set the render target as horizontalRenderTarget, 
            // we are drawing a horizontal blur into this texture
            GraphicsDevice.SetRenderTarget(HorizontalRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            // Set the technique to our blur technique
            // Then draw a texture into a full-screen quad
            // using our rendertarget as texture

            Effect.CurrentTechnique = Effect.Techniques["BlurHorizontalTechnique"];
            Effect.Parameters["baseTexture"].SetValue(MainRenderTarget);
            FullScreenQuad.Draw(Effect);

            #endregion

            #region Pass 3

            // Now we are drawing into the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Set the technique to our blur technique
            // Then draw a texture into a full-screen quad
            // using our rendertarget as texture

            Effect.CurrentTechnique = Effect.Techniques["BlurVerticalTechnique"];
            Effect.Parameters["baseTexture"].SetValue(HorizontalRenderTarget);
            FullScreenQuad.Draw(Effect);

            #endregion
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            base.UnloadContent();
            FullScreenQuad.Dispose();
            HorizontalRenderTarget.Dispose();
            MainRenderTarget.Dispose();
        }

        private enum BlurType
        {
            NONE,
            SIMPLE,
            SEPARATED_PASSES
        }
    }
}