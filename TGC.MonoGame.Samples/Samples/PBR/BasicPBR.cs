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
        //private TGCEnumModifier materials;

        private Model sphereMesh;
        private Texture2D sphereTexture;
        private Effect sphereEffect;
        private List<Light> lights;
        private List<CubePrimitive> lightBoxes;
        private List<Texture2D> textures;
        private Texture2D albedo, ao, metalness, roughness, normals;
        
        private Camera Camera { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            //Camera = new TgcRotationalCamera(TGCVector3.Empty, 80f, 1f, Input);
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 150, Vector3.UnitZ);
            
            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            InitializeLights();
            InitializeEffect();
            InitializeTextures();
            InitializeSphere();
            InitializeLightBoxes();

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            //UpdateMaterial();

            sphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);
            //textures.ForEach(texture => texture.Update());

            base.Update(gameTime);
        }
        
        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            sphereMesh.Draw(Matrix.Identity, Camera.View, Camera.Projection);
            lightBoxes.ForEach(lightBox => lightBox.Draw(Matrix.Identity, Camera.View, Camera.Projection));

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            sphereEffect.Dispose();
            textures.ForEach(t => t.Dispose());
            lightBoxes.ForEach(l => l.Dispose());
            //TODO que hago con el mesh?
            //sphereMesh.Dispose();

            base.UnloadContent();
        }
        
        private void InitializeSphere()
        {
            // Got to set a texture, else the translation to mesh does not map UV
            sphereTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "white");

            //var sphere = new TGCSphere();
            //sphere.Radius = 40.0f;
            //sphere.LevelOfDetail = 3;
            //sphere.setTexture(texture);
            //sphere.updateValues();

            sphereMesh = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            //sphereMesh.Transform = Matrix.CreateScale(Vector3.One * 30f);

            //sphere.Dispose();
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

        private void InitializeLightBoxes()
        {
            lightBoxes = new List<CubePrimitive>();
            
            var lightBoxOne = new CubePrimitive(GraphicsDevice, 10, Color.White);
            //lightBoxOne.Transform = Matrix.CreateTranslation(lights[0].Position);

            var lightBoxTwo = new CubePrimitive(GraphicsDevice, 10, Color.Purple);
            //lightBoxTwo.Transform = Matrix.CreateTranslation(lights[1].Position);

            var lightBoxThree = new CubePrimitive(GraphicsDevice, 10, Color.Yellow);
            //lightBoxThree.Transform = Matrix.CreateTranslation(lights[2].Position);

            var lightBoxFour = new CubePrimitive(GraphicsDevice, 10, Color.Cyan);;
            //lightBoxFour.Transform = Matrix.CreateTranslation(lights[3].Position);

            lightBoxes.Add(lightBoxOne);
            lightBoxes.Add(lightBoxTwo);
            lightBoxes.Add(lightBoxThree);
            lightBoxes.Add(lightBoxFour);
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
            var defaultTexturePath = ContentFolderTextures + "pbr/harsh-metal/";

            textures = new List<Texture2D>();

            normals = Game.Content.Load<Texture2D>(defaultTexturePath + "normal"); 
            //new TGCTextureAutoUpdateModifier("normalTexture");

            ao = Game.Content.Load<Texture2D>(ContentFolderTextures + "white");
            //new TGCTextureAutoUpdateModifier("aoTexture");

            metalness = Game.Content.Load<Texture2D>(defaultTexturePath + "metalness");
            //new TGCTextureAutoUpdateModifier("metallicTexture");

            roughness = Game.Content.Load<Texture2D>(defaultTexturePath + "roughness"); 
            //new TGCTextureAutoUpdateModifier("roughnessTexture");

            albedo = Game.Content.Load<Texture2D>(defaultTexturePath + "color");
            //new TGCTextureAutoUpdateModifier("albedoTexture");

            textures.Add(normals);
            textures.Add(ao);
            textures.Add(metalness);
            textures.Add(roughness);
            textures.Add(albedo);
        }

        /*
        private void UpdateMaterial()
        {
            var value = (Material)materials.Value;
            if (!value.Equals(current))
            {
                var defaultTexturePath = MediaDir + "Texturas\\PBR\\";
                current = value;
                switch (current)
                {
                    case Material.RustedMetal:
                        defaultTexturePath += "Harsh-Metal";
                        break;

                    case Material.Marble:
                        defaultTexturePath += "Marble";
                        break;

                    case Material.Gold:
                        defaultTexturePath += "Gold";
                        break;

                    case Material.Metal:
                        defaultTexturePath += "Metal";
                        break;

                    case Material.Grass:
                        defaultTexturePath += "Ground";
                        break;
                }
                defaultTexturePath += "\\";

                albedo.SetValue(defaultTexturePath + "Color.jpg");

                if (File.Exists(defaultTexturePath + "AmbientOcclusion.jpg"))
                    ao.SetValue(defaultTexturePath + "AmbientOcclusion.jpg");
                else
                    ao.SetValue(MediaDir + "Texturas\\white.bmp");

                if (File.Exists(defaultTexturePath + "Metalness.jpg"))
                    metalness.SetValue(defaultTexturePath + "Metalness.jpg");
                else
                    metalness.SetValue(MediaDir + "Texturas\\green.bmp");

                normals.SetValue(defaultTexturePath + "Normal.jpg");
                roughness.SetValue(defaultTexturePath + "Roughness.jpg");
            }
        }*/
    }
}