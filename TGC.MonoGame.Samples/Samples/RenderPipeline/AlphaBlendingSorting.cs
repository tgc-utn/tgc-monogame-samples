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
        private Camera _camera;

        /// <summary>
        /// A quad to draw the floor and transparent geometry
        /// </summary>
        private QuadPrimitive _quad;

        /// <summary>
        /// An Alpha Blending Effect to draw transparent geometry
        /// </summary>
        private Effect _alphaBlendingEffect;

        /// <summary>
        /// A Tiling Texture Effect to draw the floor
        /// </summary>
        private Effect _tilingFloorEffect;

        /// <summary>
        /// A list containing quad world matrices that should be drawn from further to nearest
        /// </summary>
        private List<Matrix> _backToFrontQuadWorlds;

        /// <summary>
        /// A list containing quad world matrices that should be drawn from nearest to furthest
        /// </summary>
        private List<Matrix> _frontToBackQuadWorlds;

        /// <summary>
        /// A list containing quad world matrices that should be drawn from further to nearest but without depth testing
        /// </summary>
        private List<Matrix> _depthNoneQuadWorlds;

        /// <summary>
        /// Colors per quad
        /// </summary>
        private List<Color> _quadColors;

        /// <summary>
        /// A list of the screen positions for the texts, modified on each update
        /// </summary>
        private List<Vector2> _textScreenPositions;

        /// <summary>
        /// A list of the positions for the texts
        /// </summary>
        private List<Vector3> _textWorldPositions;

        /// <summary>
        /// A list of texts to display
        /// </summary>
        private List<string> _texts;

        /// <summary>
        /// A font to draw text into the screen
        /// </summary>
        private SpriteFont _spriteFont;

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
            _backToFrontQuadWorlds = new List<Matrix>()
            {
                baseScaleRotation * Matrix.CreateTranslation(-30f, 5f, 0f),
                baseScaleRotation * Matrix.CreateTranslation(-30f, 5f, 10f),
                baseScaleRotation * Matrix.CreateTranslation(-30f, 5f, 20f),
            };

            _frontToBackQuadWorlds = new List<Matrix>()
            {
                baseScaleRotation * Matrix.CreateTranslation(0f, 5f, 20f),
                baseScaleRotation * Matrix.CreateTranslation(0f, 5f, 10f),
                baseScaleRotation * Matrix.CreateTranslation(0f, 5f, 0f),
            };

            _depthNoneQuadWorlds = new List<Matrix>()
            {
                baseScaleRotation * Matrix.CreateTranslation(30f, 5f, 0f),
                baseScaleRotation * Matrix.CreateTranslation(30f, 5f, 10f),
                baseScaleRotation * Matrix.CreateTranslation(30f, 5f, 20f),
            };

            _quadColors = new List<Color>()
            {
                Color.Yellow,
                Color.Red,
                Color.Blue,
            };

            var offset = Vector3.Up * 15f;
            _textWorldPositions = new List<Vector3>
            {
                _backToFrontQuadWorlds[1].Translation + offset,
                _frontToBackQuadWorlds[1].Translation + offset,
                _depthNoneQuadWorlds[1].Translation + offset,
            };

            _textScreenPositions = new List<Vector2>
            {
                Vector2.Zero, 
                Vector2.Zero, 
                Vector2.Zero,
            };

            _quad = new QuadPrimitive(GraphicsDevice);

            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 20f);

            _texts = new List<string>()
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
            _tilingFloorEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            _tilingFloorEffect.Parameters["Texture"].SetValue(floorTexture);
            _tilingFloorEffect.Parameters["Tiling"].SetValue(Vector2.One * 10f);
            
            // Load the texture for the quads
            var quadsTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tierra");

            // Load the alpha blending effect and set its parameters
            _alphaBlendingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "AlphaBlending");
            _alphaBlendingEffect.Parameters["Texture"].SetValue(quadsTexture);
            _alphaBlendingEffect.Parameters["AlphaFactor"].SetValue(0.5f);

            // Load the Sprite Font to draw text
            _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime);

            // Update text positions in screen space to face the camera
            for (var index = 0; index < _textScreenPositions.Count; index++)
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
            var size = _spriteFont.MeasureString(_texts[index]) / 2f;
            _textScreenPositions[index] = ToVector2(GraphicsDevice.Viewport.Project(
                    _textWorldPositions[index], _camera.Projection, _camera.View, Matrix.Identity)) - size;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            var viewProjection = _camera.View * _camera.Projection;

            // Enable back-face culling
            // Enable depth testing and writing
            // Set the blend state as opaque, we are drawing our opaque geometry
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            
            // Draw the floor
            _tilingFloorEffect.Parameters["WorldViewProjection"].SetValue(Matrix.CreateScale(100f) * viewProjection);
            _quad.Draw(_tilingFloorEffect);

            // Set the blend state as alpha blend, 
            // we are doing this operation when writing on the ColorBuffer:
            // ColorBuffer.rgb = lerp(ColorBuffer.rgb, FragmentColor.rgb, Alpha)

            // Set the depth testing ON but depth writing OFF
            // We don't need to write transparent geometry to the Depth Buffer
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Draw back-to-front transparent geometry
            for (var index = 0; index < _backToFrontQuadWorlds.Count; index++)
            {
                _alphaBlendingEffect.Parameters["Tint"].SetValue(_quadColors[index].ToVector3());
                _alphaBlendingEffect.Parameters["WorldViewProjection"].SetValue(_backToFrontQuadWorlds[index] * viewProjection);
                _quad.Draw(_alphaBlendingEffect);
            }

            // Draw front-to-back transparent geometry
            for (var index = 0; index < _frontToBackQuadWorlds.Count; index++)
            {
                var colorIndex = _frontToBackQuadWorlds.Count - index - 1;
                _alphaBlendingEffect.Parameters["Tint"].SetValue(_quadColors[colorIndex].ToVector3());
                _alphaBlendingEffect.Parameters["WorldViewProjection"].SetValue(_frontToBackQuadWorlds[index] * viewProjection);
                _quad.Draw(_alphaBlendingEffect);
            }

            // Set the depth testing OFF and writing OFF
            // Draw back-to-front geometry
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            for (var index = 0; index < _depthNoneQuadWorlds.Count; index++)
            {
                _alphaBlendingEffect.Parameters["Tint"].SetValue(_quadColors[index].ToVector3());
                _alphaBlendingEffect.Parameters["WorldViewProjection"].SetValue(_depthNoneQuadWorlds[index] * viewProjection);
                _quad.Draw(_alphaBlendingEffect);
            }
            
            // Draw labels
            Game.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone);

            for (var index = 0; index < _textScreenPositions.Count; index++)
                Game.SpriteBatch.DrawString(_spriteFont, _texts[index], _textScreenPositions[index], Color.White);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
