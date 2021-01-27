using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PBR
{
    public class BasicPBR : TGCSample
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public BasicPBR(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PBR;
            Name = "Basic PBR";
            Description = "Ejemplo de PBR regular.";
        }

        private Material current;
        private List<Material> materials;

        private Model sphere;
        private Texture2D sphereTexture;
        private string texturePath;
        private Effect sphereEffect;
        private List<Light> lights;
        private CubePrimitive lightBox;
        private List<Texture2D> textures;
        private Texture2D albedo, ao, metalness, roughness, normals;

        private Matrix sphereWorld; 

        private Camera Camera { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 40, 200), size);
            
            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            InitializeLights();
            InitializeEffect();
            InitializeTextures();
            InitializeSphere();
            InitializeLightBox();

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            //UpdateMaterial();
            Camera.Update(gameTime);

            sphereEffect.Parameters["eyePosition"]?.SetValue(Camera.Position);
            //textures.ForEach(texture => texture.Update());

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            var worldView = sphereWorld * Camera.View;
            sphereEffect.Parameters["matWorld"].SetValue(sphereWorld);
            sphereEffect.Parameters["matWorldViewProj"].SetValue(worldView * Camera.Projection);
            sphereEffect.Parameters["matInverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(worldView)));

            sphere.Meshes[0].Draw();

            for (int index = 0; index < lights.Count; index++)
            {
                lightBox.Effect.DiffuseColor = lights[index].Color;
                lightBox.Draw(Matrix.CreateTranslation(lights[index].Position), Camera.View, Camera.Projection);
            }

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            sphereEffect.Dispose();
            lightBox.Dispose();
            //TODO que hago con el mesh?

            DisposeTextures();

            base.UnloadContent();
        }
        
        private void InitializeSphere()
        {
            // Got to set a texture, else the translation to mesh does not map UV
            //sphereTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "white");

            sphere = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            sphereWorld = Matrix.CreateScale(30f) * Matrix.CreateRotationX(MathF.PI * 0.5f);
            sphere.Meshes[0].MeshParts[0].Effect = sphereEffect;
        }

        private void InitializeEffect()
        {
            sphereEffect = Game.Content.Load<Effect>(ContentFolderEffects + "PBR");
            sphereEffect.CurrentTechnique = sphereEffect.Techniques["PBR"];


            int index = 0;
            lights.ForEach(light =>
            {
                light.SetLight(index, sphereEffect);
                index++;
            });
        }

        private void InitializeLightBox()
        {
            lightBox = new CubePrimitive(GraphicsDevice, 10, Color.White);
        }

        private void InitializeLights()
        {
            lights = new List<Light>();
            var lightOne = new Light();

            float distance = 45f;

            lightOne.Position = Vector3.One * distance;
            lightOne.Color = new Vector3(200f, 200f, 200f);

            var lightTwo = new Light();
            lightTwo.Position = Vector3.One * -distance;
            lightTwo.Color = new Vector3(100f, 30f, 100f);

            var lightThree = new Light();
            lightThree.Position = Vector3.One * distance - new Vector3(2f * distance, 0f, 0f);
            lightThree.Color = new Vector3(100f, 100f, 0f);

            var lightFour = new Light();
            lightFour.Position = Vector3.One * -distance + new Vector3(2f * distance, 0f, 0f);
            lightFour.Color = new Vector3(0f, 100f, 100f);

            lights.Add(lightOne);
            lights.Add(lightTwo);
            lights.Add(lightThree);
            lights.Add(lightFour);
        }

        private void InitializeTextures()
        {
            current = Material.RustedMetal;
            UpdateMaterialPath();
            textures = new List<Texture2D>();
            LoadTextures();
        }

        private void LoadTextures()
        {
            normals = Game.Content.Load<Texture2D>(texturePath + "normal");
            ao = Game.Content.Load<Texture2D>(texturePath + "ao");
            metalness = Game.Content.Load<Texture2D>(texturePath + "metalness");
            roughness = Game.Content.Load<Texture2D>(texturePath + "roughness");
            albedo = Game.Content.Load<Texture2D>(texturePath + "color");

            textures.Add(normals);
            textures.Add(ao);
            textures.Add(metalness);
            textures.Add(roughness);
            textures.Add(albedo);

            sphereEffect.Parameters["albedoTexture"]?.SetValue(albedo);
            sphereEffect.Parameters["normalTexture"]?.SetValue(normals);
            sphereEffect.Parameters["metallicTexture"]?.SetValue(metalness);
            sphereEffect.Parameters["roughnessTexture"]?.SetValue(roughness);
            sphereEffect.Parameters["aoTexture"]?.SetValue(ao);
        }
        
        private void UpdateMaterialPath()
        {
            texturePath = ContentFolderTextures + "pbr/";
            switch (current)
            {
                case Material.RustedMetal:
                    texturePath += "harsh-metal";
                    break;

                case Material.Marble:
                    texturePath += "marble";
                    break;

                case Material.Gold:
                    texturePath += "gold";
                    break;

                case Material.Metal:
                    texturePath += "metal";
                    break;

                case Material.Grass:
                    texturePath += "ground";
                    break;
            }
            texturePath += "/";
        }

        private void SwitchMaterial()
        {
            DisposeTextures();
            UpdateMaterialPath();
            LoadTextures();
        }

        private void DisposeTextures()
        {
            textures.ForEach(t => t.Dispose());
            textures.Clear();
        }
    }
}