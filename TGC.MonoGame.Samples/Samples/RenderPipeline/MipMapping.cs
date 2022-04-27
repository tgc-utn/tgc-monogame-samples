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

        /// <summary>
        /// A camera to draw geometry
        /// </summary>
        private Camera Camera { get; set; }

        /// <summary>
        /// A quad to draw the floor and transparent geometry
        /// </summary>
        private QuadPrimitive Quad { get; set; }

        /// <summary>
        /// A world matrix for the floor quad
        /// </summary>
        private Matrix FloorWorld { get; set; }

        /// <summary>
        /// A Tiling Texture Effect to draw the floor
        /// </summary>
        private Effect TilingFloorEffect { get; set; }

        /// <summary>
        /// A Texture with Mip-Mapping enabled
        /// </summary>
        private Texture2D TextureWithMipMapping {get; set;}

        /// <summary>
        /// A Texture with Mip-Mapping disabled
        /// </summary>
        private Texture2D TextureWithoutMipMapping { get; set; }


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
            Quad = new QuadPrimitive(GraphicsDevice);

            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 20f);
            Camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 0.1f, 100000f, MathF.PI / 3f);

            FloorWorld = Matrix.CreateScale(500f);

            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the texture for the floor
            TextureWithMipMapping = Game.Content.Load<Texture2D>(ContentFolderTextures + "stones");

            // Create a texture with no mip-mapping
            TextureWithoutMipMapping = CreateNoMipMappingTexture(TextureWithMipMapping);

            // Load the floor effect and set its parameters
            TilingFloorEffect = Game.Content.Load<Effect>(ContentFolderEffects + "MipMapping");

            // Assign the main texture, then set the tiling value
            // Pass the TextureSize and MipLevelCount required for the debug technique
            TilingFloorEffect.Parameters["Texture"].SetValue(TextureWithMipMapping);
            TilingFloorEffect.Parameters["Tiling"].SetValue(Vector2.One * 10f);
            TilingFloorEffect.Parameters["TextureSize"].SetValue(new Vector2(TextureWithMipMapping.Width, TextureWithMipMapping.Height));
            TilingFloorEffect.Parameters["MipLevelCount"].SetValue((float)TextureWithMipMapping.LevelCount);

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
                    TilingFloorEffect.Parameters["Texture"].SetValue(TextureWithMipMapping);
                    TilingFloorEffect.CurrentTechnique = TilingFloorEffect.Techniques["Bilinear"];
                    break;

                // For this option, just use a texture with no mip mapping
                // Ideally, this should be an option on the GraphicsDevice SamplerState
                case MipMappingType.NoMipMapping:
                    TilingFloorEffect.Parameters["Texture"].SetValue(TextureWithoutMipMapping);
                    TilingFloorEffect.CurrentTechnique = TilingFloorEffect.Techniques["Bilinear"];
                    break;

                // Use a technique to draw the mip level used
                // No texture is needed so there is no problem on using the previously set
                case MipMappingType.Debug:
                    TilingFloorEffect.CurrentTechnique = TilingFloorEffect.Techniques["Debug"];
                    break;

                // For anything else use Trilinear, using a technique with Trilinear Sampling enabled
                default:
                    TilingFloorEffect.Parameters["Texture"].SetValue(TextureWithMipMapping);
                    TilingFloorEffect.CurrentTechnique = TilingFloorEffect.Techniques["Trilinear"];
                    break;
            }
        }


        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            base.Update(gameTime);
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
            TilingFloorEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
            Quad.Draw(TilingFloorEffect);


            base.Draw(gameTime);
        }

    }
}
