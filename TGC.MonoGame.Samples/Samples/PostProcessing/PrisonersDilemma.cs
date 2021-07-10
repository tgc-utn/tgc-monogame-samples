using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PostProcessing
{
    public class PrisonersDilemma : TGCSample
    {

        /// <inheritdoc />
        public PrisonersDilemma(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PostProcessing;
            Name = "Prisoner's Dilemma";
            Description = "Using two render targets to draw with a given technique";
        }

        private Effect Effect { get; set; }

        private FullScreenQuad FullScreenQuad { get; set; }

        private RenderTarget2D RenderTargetA { get; set; }

        private RenderTarget2D RenderTargetB { get; set; }


        private RenderTarget2D DestinationRenderTarget { get; set; }

        private RenderTarget2D SourceRenderTarget { get; set; }



        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the shadowmap effect
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "PrisonersDilemma");

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create a render target for the scene
            RenderTargetA = new RenderTarget2D(GraphicsDevice, 512, 512, 
                false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.PreserveContents);

            RenderTargetB = new RenderTarget2D(GraphicsDevice, 512, 512, 
                false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.PreserveContents);


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            SourceRenderTarget = RenderTargetA;
            DestinationRenderTarget = RenderTargetB;

            Effect.Parameters["ScreenSize"].SetValue(new Vector2(1f / RenderTargetA.Width, 1f / RenderTargetA.Height));

            base.LoadContent();
        }

        private bool FirstTime = true;

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            float totalTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            Effect.Parameters["Time"]?.SetValue(totalTime);

            if(FirstTime)
            {
                // Clear both RenderTargets
                GraphicsDevice.SetRenderTarget(RenderTargetA);
                GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
                
                GraphicsDevice.SetRenderTarget(RenderTargetB);
                GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);


                // Set the render target as the Destination and draw 
                GraphicsDevice.SetRenderTarget(DestinationRenderTarget);
                Effect.CurrentTechnique = Effect.Techniques["InitialConditions"];
                FullScreenQuad.Draw(Effect);

                FirstTime = false;
            }
            else
            {
                // Set the render target as our Destination, use our past RenderTarget and draw
                GraphicsDevice.SetRenderTarget(DestinationRenderTarget);
                Effect.CurrentTechnique = Effect.Techniques["Process"];
                Effect.Parameters["MainTexture"].SetValue(SourceRenderTarget);
                FullScreenQuad.Draw(Effect);
            }



            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
            Effect.CurrentTechnique = Effect.Techniques["Debug"];
            Effect.Parameters["MainTexture"].SetValue(DestinationRenderTarget);
            FullScreenQuad.Draw(Effect);



            var temp = DestinationRenderTarget;
            DestinationRenderTarget = SourceRenderTarget;
            SourceRenderTarget = temp;


            base.Draw(gameTime);
        }

    }
}
