using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    /// <summary>
    /// Shows how to draw meshes with various types of Blinn-Phong techniques.
    /// Author: Ronan Vinitzca
    /// </summary>
    public class BlinnPhongTypes : TGCSample
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
        /// An effect to draw using blinn-phong techniques
        /// </summary>
        private Effect Effect { get; set; }

        /// <summary>
        /// Floor color texture
        /// </summary>
        private Texture2D FloorTexture { get; set; }

        /// <summary>
        /// Floor normal map
        /// </summary>
        private Texture2D FloorNormalMap { get; set; }

        /// <summary>
        /// Model color texture
        /// </summary>
        private Texture2D ModelTexture { get; set; }

        /// <summary>
        /// Model normal map
        /// </summary>
        private Texture2D ModelNormal { get; set; }

        /// <summary>
        /// The position of the light
        /// </summary>
        private Vector3 LightPosition { get; set; } = Vector3.Zero;

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

        /// <summary>
        /// The current blinn-phong type
        /// </summary>
        private BlinnPhongType EffectType { get; set; }

        /// <inheritdoc />
        public BlinnPhongTypes(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Blinn Phong Types";
            Description = "Applying Blinn-Phong Types to models";
        }


        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);

            ModelWorld = Matrix.CreateScale(15f) * Matrix.CreateRotationX(-MathF.PI * 0.5f);
            FloorWorld = Matrix.CreateScale(300f) * Matrix.CreateTranslation(0f, -20f, 0f);

            base.Initialize();
        }



        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere mesh and floor, from runtime generated primitives
            Model = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            Floor = new QuadPrimitive(GraphicsDevice);

            LightBox = new CubePrimitive(GraphicsDevice, 1f, Color.White);

            FloorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tiling-base");
            FloorNormalMap = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tiling-normal");
            ModelTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/harsh-metal/color");
            ModelNormal = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/harsh-metal/normal");

            // We load the effect with various techniques
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhongTypes");

            foreach (var modelMesh in Model.Meshes)
            {
                foreach (var meshPart in modelMesh.MeshParts)
                { 
                    meshPart.Effect = Effect; 
                }
            }


            // Add mappings for modifiers to control values
            ModifierController.AddVector("Light Position", SetLightPosition, Vector3.Up * 45f);
            ModifierController.AddColor("Ambient Color", Effect.Parameters["ambientColor"], new Color(0f, 0f, 1f));
            ModifierController.AddColor("Diffuse Color", Effect.Parameters["diffuseColor"], new Color(0.1f, 0.1f, 0.6f));
            ModifierController.AddColor("Specular Color", Effect.Parameters["specularColor"], Color.White);

            ModifierController.AddFloat("KA", Effect.Parameters["KAmbient"], 0.1f, 0f, 1f);
            ModifierController.AddFloat("KD", Effect.Parameters["KDiffuse"], 0.7f, 0f, 1f);
            ModifierController.AddFloat("KS", Effect.Parameters["KSpecular"], 0.4f, 0f, 1f);
            ModifierController.AddFloat("Shininess", Effect.Parameters["shininess"], 4.0f, 1f, 64f);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Add options to pick the blinn phong type
            ModifierController.AddOptions("Blinn Phong Type", new[]
            {
                "Default",
                "Gouraud",
                "Normal Mapping"
            }, BlinnPhongType.DEFAULT, BlinnPhongTypeChange);

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
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);

            Effect.Parameters["ModelTexture"].SetValue(ModelTexture);
            Effect.Parameters["NormalTexture"].SetValue(ModelNormal);
            Effect.Parameters["World"].SetValue(ModelWorld);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(ModelWorld)));
            Effect.Parameters["WorldViewProjection"].SetValue(ModelWorld * viewProjection);
            Effect.Parameters["Tiling"].SetValue(Vector2.One);

            foreach (var modelMesh in Model.Meshes)
            { 
                modelMesh.Draw(); 
            }

            Effect.Parameters["ModelTexture"].SetValue(FloorTexture);
            Effect.Parameters["NormalTexture"].SetValue(FloorNormalMap);
            Effect.Parameters["World"].SetValue(FloorWorld);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(FloorWorld)));
            Effect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
            Effect.Parameters["Tiling"].SetValue(Vector2.One * 5f);

            Floor.Draw(Effect);

            LightBox.Draw(LightBoxWorld, Camera.View, Camera.Projection);
          
            base.Draw(gameTime);
        }

        /// <summary>
        /// Processes a change in the blinn-phong type.
        /// </summary>
        /// <param name="type">The new blinn-phong type to use</param>
        private void BlinnPhongTypeChange(BlinnPhongType type)
        {
            switch(type)
            {
                case BlinnPhongType.GOURAUD:
                    Effect.CurrentTechnique = Effect.Techniques["Gouraud"];
                    break;
                case BlinnPhongType.NORMAL_MAPPING:
                    Effect.CurrentTechnique = Effect.Techniques["NormalMapping"];
                    break;
                default:
                    Effect.CurrentTechnique = Effect.Techniques["Default"];
                    break;
            }
        }

        /// <summary>
        /// The different types of blinn-phong illumination
        /// </summary>
        private enum BlinnPhongType
        {
            DEFAULT,
            GOURAUD,
            NORMAL_MAPPING
        }
    }
}
