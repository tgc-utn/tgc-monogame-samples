using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.RenderPipeline
{
    /// <summary>
    /// Shows why back-to-front sorting needs to be performed when drawing alpha blended geometry
    /// </summary>
    public class AlphaBlendingSorting : TGCSample
    {
        /// <summary>
        /// A camera to draw geometry
        /// </summary>
        private Camera Camera { get; set; }

        /// <summary>
        /// A quad to draw the floor and transparent geometry
        /// </summary>
        private QuadPrimitive Quad { get; set; }

        /// <summary>
        /// An Alpha Blending Effect to draw transparent geometry
        /// </summary>
        private Effect AlphaBlendingEffect { get; set; }

        /// <summary>
        /// A Tiling Texture Effect to draw the floor
        /// </summary>
        private Effect TilingFloorEffect { get; set; }

        /// <summary>
        /// A list containing quad world matrices that should be drawn from further to nearest
        /// </summary>
        private List<Matrix> BackToFrontQuadWorlds { get; set; }

        /// <summary>
        /// A list containing quad world matrices that should be drawn from nearest to furthest
        /// </summary>
        private List<Matrix> FrontToBackQuadWorlds { get; set; }

        /// <summary>
        /// A list containing quad world matrices that should be drawn from further to nearest but without depth testing
        /// </summary>
        private List<Matrix> DepthNoneQuadWorlds { get; set; }

        /// <summary>
        /// Colors per quad
        /// </summary>
        private List<Color> QuadColors { get; set; }

        /// <summary>
        /// A list of the screen positions for the texts, modified on each update
        /// </summary>
        private List<Vector2> TextScreenPositions { get; set; }

        /// <summary>
        /// A list of the positions for the texts
        /// </summary>
        private List<Vector3> TextWorldPositions { get; set; }

        /// <summary>
        /// A list of texts to display
        /// </summary>
        private List<string> Texts { get; set; }

        /// <summary>
        /// A font to draw text into the screen
        /// </summary>
        private SpriteFont SpriteFont { get; set; }


        /// <inheritdoc />
        public AlphaBlendingSorting(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.RenderPipeline;
            Name = "Alpha Blending Sorting";
            Description = "Shows why back-to-front sorting needs to be performed when drawing alpha blended geometry";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            var baseScaleRotation = Matrix.CreateScale(10f) * Matrix.CreateRotationX(MathF.PI / 2f);
            BackToFrontQuadWorlds = new List<Matrix>()
            {
                baseScaleRotation * Matrix.CreateTranslation(-30f, 5f, 0f),
                baseScaleRotation * Matrix.CreateTranslation(-30f, 5f, 10f),
                baseScaleRotation * Matrix.CreateTranslation(-30f, 5f, 20f),
            };

            FrontToBackQuadWorlds = new List<Matrix>()
            {
                baseScaleRotation * Matrix.CreateTranslation(0f, 5f, 20f),
                baseScaleRotation * Matrix.CreateTranslation(0f, 5f, 10f),
                baseScaleRotation * Matrix.CreateTranslation(0f, 5f, 0f),
            };

            DepthNoneQuadWorlds = new List<Matrix>()
            {
                baseScaleRotation * Matrix.CreateTranslation(30f, 5f, 0f),
                baseScaleRotation * Matrix.CreateTranslation(30f, 5f, 10f),
                baseScaleRotation * Matrix.CreateTranslation(30f, 5f, 20f),
            };


            QuadColors = new List<Color>()
            {
                Color.Yellow,
                Color.Red,
                Color.Blue,
            };


            var offset = Vector3.Up * 15f;
            TextWorldPositions = new List<Vector3>
            {
                BackToFrontQuadWorlds[1].Translation + offset,
                FrontToBackQuadWorlds[1].Translation + offset,
                DepthNoneQuadWorlds[1].Translation + offset,
            };

            TextScreenPositions = new List<Vector2>
            {
                Vector2.Zero, 
                Vector2.Zero, 
                Vector2.Zero,
            };

            Quad = new QuadPrimitive(GraphicsDevice);

            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 20f);

            Texts = new List<string>()
            {
                "Back to Front\nDepth Test\nDepth Write OFF",
                "Front to Back\nDepth Test\nDepth Write OFF",
                "Back to Front\nDepth Read OFF\nDepth Write OFF",
            };

            base.Initialize();
        }

        /// <summary>
        /// Returns a <see cref="Vector2"/> containing the XY components of a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3"/> to obtain its XY components</param>
        /// <returns>A <see cref="Vector2"/> containing the XY components of the given vector</returns>
        private Vector2 ToVector2(Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the texture for the floor
            var floorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "grass");

            // Load the floor effect and set its parameters
            TilingFloorEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");

            TilingFloorEffect.Parameters["Texture"].SetValue(floorTexture);
            TilingFloorEffect.Parameters["Tiling"].SetValue(Vector2.One * 10f);


            // Load the texture for the quads
            var quadsTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tierra");

            // Load the alpha blending effect and set its parameters
            AlphaBlendingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "AlphaBlending");
            AlphaBlendingEffect.Parameters["Texture"].SetValue(quadsTexture);
            AlphaBlendingEffect.Parameters["AlphaFactor"].SetValue(0.5f);

            // Load the Sprite Font to draw text
            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            // Update text positions in screen space to face the camera
            for (var index = 0; index < TextScreenPositions.Count; index++)
                UpdateTextPosition(index);

            base.Update(gameTime);
        }

        /// <summary>
        /// Updates a text position in screen space to face the camera, 
        /// based on the world space position and the camera values.
        /// </summary>
        /// <param name="index">The index of the text to update</param>
        private void UpdateTextPosition(int index)
        {
            var size = SpriteFont.MeasureString(Texts[index]) / 2f;
            TextScreenPositions[index] = ToVector2(GraphicsDevice.Viewport.Project(
                    TextWorldPositions[index], Camera.Projection, Camera.View, Matrix.Identity)) - size;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            var viewProjection = Camera.View * Camera.Projection;

            // Enable back-face culling
            // Enable depth testing and writing
            // Set the blend state as opaque, we are drawing our opaque geometry
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            
            // Draw the floor
            TilingFloorEffect.Parameters["WorldViewProjection"].SetValue(Matrix.CreateScale(100f) * viewProjection);
            Quad.Draw(TilingFloorEffect);

            // Set the blend state as alpha blend, 
            // we are doing this operation when writing on the ColorBuffer:
            // ColorBuffer.rgb = lerp(ColorBuffer.rgb, FragmentColor.rgb, Alpha)

            // Set the depth testing ON but depth writing OFF
            // We don't need to write transparent geometry to the Depth Buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Draw back-to-front transparent geometry
            for (var index = 0; index < BackToFrontQuadWorlds.Count; index++)
            {
                AlphaBlendingEffect.Parameters["Tint"].SetValue(QuadColors[index].ToVector3());
                AlphaBlendingEffect.Parameters["WorldViewProjection"].SetValue(BackToFrontQuadWorlds[index] * viewProjection);
                Quad.Draw(AlphaBlendingEffect);
            }

            // Draw front-to-back transparent geometry
            for (var index = 0; index < FrontToBackQuadWorlds.Count; index++)
            {
                var colorIndex = FrontToBackQuadWorlds.Count - index - 1;
                AlphaBlendingEffect.Parameters["Tint"].SetValue(QuadColors[colorIndex].ToVector3());
                AlphaBlendingEffect.Parameters["WorldViewProjection"].SetValue(FrontToBackQuadWorlds[index] * viewProjection);
                Quad.Draw(AlphaBlendingEffect);
            }

            // Set the depth testing OFF and writing OFF
            // Draw back-to-front geometry
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            for (var index = 0; index < DepthNoneQuadWorlds.Count; index++)
            {
                AlphaBlendingEffect.Parameters["Tint"].SetValue(QuadColors[index].ToVector3());
                AlphaBlendingEffect.Parameters["WorldViewProjection"].SetValue(DepthNoneQuadWorlds[index] * viewProjection);
                Quad.Draw(AlphaBlendingEffect);
            }


            // Draw labels
            Game.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone);

            for (var index = 0; index < TextScreenPositions.Count; index++)
                Game.SpriteBatch.DrawString(SpriteFont, Texts[index], TextScreenPositions[index], Color.White);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
