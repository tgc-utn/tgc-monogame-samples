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
    /// Shows the importance of using MipMapping for distant texture sampling.
    /// </summary>
    public class MipMapping : TGCSample
    {
        /// <summary>
        /// Options for different mip mapping visualizations.
        /// </summary>
        private enum MipMappingType
        {
            Trilinear,
            Bilinear,
            NoMipMapping,
            Debug,
        }
        
        private const string Texture = "Texture";

        /// <summary>
        /// A camera to draw geometry
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// A quad to draw the floor and transparent geometry
        /// </summary>
        private QuadPrimitive _quad;

        /// <summary>
        /// A world matrix for the floor quad
        /// </summary>
        private Matrix _floorWorld;

        /// <summary>
        /// A Tiling Texture Effect to draw the floor
        /// </summary>
        private Effect _tilingFloorEffect;

        /// <summary>
        /// A Texture with Mip-Mapping enabled
        /// </summary>
        private Texture2D _textureWithMipMapping;

        /// <summary>
        /// A Texture with Mip-Mapping disabled
        /// </summary>
        private Texture2D _textureWithoutMipMapping;
        
        /// <inheritdoc />
        public MipMapping(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.RenderPipeline;
            Name = "MipMapping";
            Description = "Shows the importance of using MipMapping for distant texture sampling.";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            _quad = new QuadPrimitive(GraphicsDevice);

            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 20f);
            _camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 0.1f, 100_000f, MathF.PI / 3f);

            _floorWorld = Matrix.CreateScale(500f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the texture for the floor
            _textureWithMipMapping = Game.Content.Load<Texture2D>(ContentFolderTextures + "stones");

            // Create a texture with no mip-mapping
            _textureWithoutMipMapping = CreateNoMipMappingTexture(_textureWithMipMapping);

            // Load the floor effect and set its parameters
            _tilingFloorEffect = Game.Content.Load<Effect>(ContentFolderEffects + "MipMapping");

            // Assign the main texture, then set the tiling value
            // Pass the TextureSize and MipLevelCount required for the debug technique
            _tilingFloorEffect.Parameters[Texture].SetValue(_textureWithMipMapping);
            _tilingFloorEffect.Parameters["Tiling"].SetValue(Vector2.One * 10f);
            _tilingFloorEffect.Parameters["TextureSize"].SetValue(new Vector2(_textureWithMipMapping.Width, _textureWithMipMapping.Height));
            _tilingFloorEffect.Parameters["MipLevelCount"].SetValue((float)_textureWithMipMapping.LevelCount);

            // Create a modifier to change the technique used
            ModifierController.AddOptions<MipMappingType>("Type", OnMipMappingTypeChange);

            base.LoadContent();
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> with no mip-mapping from another texture.
        /// </summary>
        /// <param name="mipMappingTexture">The original texture to get the texel data</param>
        /// <returns>A texture with no mip-mapping</returns>
        private Texture2D CreateNoMipMappingTexture(Texture2D mipMappingTexture)
        {
            var width = mipMappingTexture.Width;
            var height = mipMappingTexture.Height;

            // Create a texture with mip-mapping disabled
            var noMipMappingTexture = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);

            // Create an array of texels
            var colors = new Color[width * height];

            // Get texel colors from the base texture, using the mip level zero (original resolution)
            mipMappingTexture.GetData(0, null, colors, 0, colors.Length);

            // Write colors to the texture with no mip-mapping
            noMipMappingTexture.SetData(colors);

            return noMipMappingTexture;
        }

        /// <summary>
        /// Processes a change in the MipMapping type used.
        /// </summary>
        /// <param name="type">The new mip-mapping type to be used</param>
        private void OnMipMappingTypeChange(MipMappingType type)
        {
            switch (type)
            {
                // For Bilinear, use a technique with Trilinear Sampling disabled
                case MipMappingType.Bilinear:
                    _tilingFloorEffect.Parameters[Texture].SetValue(_textureWithMipMapping);
                    _tilingFloorEffect.CurrentTechnique = _tilingFloorEffect.Techniques["Bilinear"];
                    break;

                // For this option, just use a texture with no mip mapping
                // Ideally, this should be an option on the GraphicsDevice SamplerState
                case MipMappingType.NoMipMapping:
                    _tilingFloorEffect.Parameters[Texture].SetValue(_textureWithoutMipMapping);
                    _tilingFloorEffect.CurrentTechnique = _tilingFloorEffect.Techniques["Bilinear"];
                    break;

                // Use a technique to draw the mip level used
                // No texture is needed so there is no problem on using the previously set
                case MipMappingType.Debug:
                    _tilingFloorEffect.CurrentTechnique = _tilingFloorEffect.Techniques["Debug"];
                    break;

                // For anything else use Trilinear, using a technique with Trilinear Sampling enabled
                default:
                    _tilingFloorEffect.Parameters[Texture].SetValue(_textureWithMipMapping);
                    _tilingFloorEffect.CurrentTechnique = _tilingFloorEffect.Techniques["Trilinear"];
                    break;
            }
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _camera.Update(gameTime);

            base.Update(gameTime);
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
            _tilingFloorEffect.Parameters["WorldViewProjection"].SetValue(_floorWorld * viewProjection);
            _quad.Draw(_tilingFloorEffect);
            
            base.Draw(gameTime);
        }
    }
}
