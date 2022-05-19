using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    /// <summary>
    /// Shows how to draw meshes with various types of Blinn-Phong techniques.
    /// Author: Ronan Vinitzca
    /// </summary>
    public class ToonShading : TGCSample
    {
        /// <summary>
        /// A Camera to draw models in space
        /// </summary>
        private Camera Camera { get; set; }

        /// <summary>
        /// A model to draw in the center of the scene
        /// </summary>
        private Model Model { get; set; }

        /// <summary>
        /// Geometry to draw a floor
        /// </summary>
        private QuadPrimitive Floor { get; set; }

        /// <summary>
        /// A box to draw where the light is
        /// </summary>
        private CubePrimitive LightBox { get; set; }

        /// <summary>
        /// An effect to draw using toon shading techniques
        /// </summary>
        private Effect Effect { get; set; }


        /// <summary>
        /// Texture to use as Look Up Table
        /// </summary>
        private Texture2D LookUpTable { get; set; }

        /// <summary>
        /// The world matrix for the light box
        /// </summary>
        private Matrix LightBoxWorld { get; set; } = Matrix.Identity;

        /// <summary>
        /// The world matrix for the model
        /// </summary>
        private Matrix ModelWorld { get; set; }

        /// <summary>
        /// The world matrix for the floor
        /// </summary>
        private Matrix FloorWorld { get; set; }


        /// <inheritdoc />
        public ToonShading(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Toon Shading Types";
            Description = "Applying Toon Shading to models";
        }


        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);

            FloorWorld = Matrix.CreateScale(300f) * Matrix.CreateTranslation(0f, -20f, 0f);

            base.Initialize();
        }



        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere mesh and floor, from runtime generated primitives

            Model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            
            var aabb = BoundingVolumesExtensions.CreateAABBFrom(Model);
            var height = (aabb.Max.Y - aabb.Min.Y) / 2f;

            ModelWorld = Matrix.CreateTranslation(0f, height, 0f);

            Floor = new QuadPrimitive(GraphicsDevice);
            
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "ToonShading");

            foreach (var modelMesh in Model.Meshes)
            {
                foreach (var meshPart in modelMesh.MeshParts)
                {
                    meshPart.Effect.Dispose();
                    meshPart.Effect = Effect;
                }
            }
            
            LightBox = new CubePrimitive(GraphicsDevice, 1f, Color.White);
            
            LookUpTable = Game.Content.Load<Texture2D>(ContentFolderTextures + "toon/lut");
            Effect.Parameters["LookUpTableTexture"].SetValue(LookUpTable);

            // Add options to pick the blinn phong type
            ModifierController.AddOptions("Toon Shading Type", new[]
            {
                "Default",
                "Look Up Table",
            }, ToonShadingType.DEFAULT, ToonShadingTypeChange);
            

            // Add mappings for modifiers to control values
            ModifierController.AddVector("Light Position", SetLightPosition, new Vector3(70f, 160f, 40f));
            

            ModifierController.AddColor("Color A", Effect.Parameters["colorA"], new Color(0.152f, 0f, 0.072f));
            ModifierController.AddColor("Color B", Effect.Parameters["colorB"], new Color(0.373f, 0.105f, 0.009f));
            ModifierController.AddColor("Color C", Effect.Parameters["colorC"], new Color(1f, 0.529f, 0f));
            ModifierController.AddColor("Color D", Effect.Parameters["colorD"], new Color(1f, 0.794f, 0f));

            ModifierController.AddVector("Color Range", Effect.Parameters["colorRange"], new Vector3(0.2f, 0.4f, 0.75f));
            ModifierController.AddFloat("Ambient", Effect.Parameters["KAmbient"], 0.05f, 0f, 1f);

            ModifierController.AddTexture("LUT", LookUpTable);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.LoadContent();
        }

        /// <summary>
        /// Sets the light position from the modifier.
        /// </summary>
        /// <param name="position">The new light position in world space</param>
        private void SetLightPosition(Vector3 position)
        {
            LightBoxWorld = Matrix.CreateScale(3f) *  Matrix.CreateTranslation(position);
            Effect.Parameters["lightPosition"].SetValue(position);
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
            // Set the background color to black, and use the default depth configuration
            Game.Background = Color.Black;

            var viewProjection = Camera.View * Camera.Projection;

            
            // Set the camera position
            // This changes every update so we need to set it before rendering
            Effect.Parameters["World"].SetValue(ModelWorld);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(ModelWorld)));
            Effect.Parameters["WorldViewProjection"].SetValue(ModelWorld * viewProjection);

            foreach (var modelMesh in Model.Meshes)
            { 
                modelMesh.Draw(); 
            }

            Effect.Parameters["World"].SetValue(FloorWorld);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(FloorWorld)));
            Effect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);

            Floor.Draw(Effect);

            LightBox.Draw(LightBoxWorld, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Processes a change in the blinn-phong type.
        /// </summary>
        /// <param name="type">The new blinn-phong type to use</param>
        private void ToonShadingTypeChange(ToonShadingType type)
        {
            switch(type)
            {
                case ToonShadingType.LOOKUPTABLE:
                    Effect.CurrentTechnique = Effect.Techniques["LookUpTable"];
                    break;
                default:
                    Effect.CurrentTechnique = Effect.Techniques["Default"];
                    break;
            }
        }


        /// <summary>
        /// The different types of toon-shading
        /// </summary>
        private enum ToonShadingType
        {
            DEFAULT,
            LOOKUPTABLE
        }
    }
}
