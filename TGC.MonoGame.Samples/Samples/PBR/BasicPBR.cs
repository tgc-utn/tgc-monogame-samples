using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.PBR
{
    public class BasicPBR : TGCSample
    {
        private Material Current = Material.RustedMetal;

        private SpriteFont SpriteFont;
        private Model Sphere;
        private string TexturePath;
        private Effect SphereEffect;
        private List<Light> Lights;

        private CubePrimitive LightBox;
        private Texture2D albedo, ao, metalness, roughness, normals;

        private Matrix SphereWorld;

        private Camera Camera { get; set; }

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

        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 40, 200), size);

            ModifierController.AddOptions("Material", new string[]
            {
                "RustedMetal",
                "Grass",
                "Gold",
                "Marble",
                "Metal"
            }, Material.RustedMetal, OnMaterialChange);

            base.Initialize();
        }

        /// <summary>
        ///     Processes a change in the selected Material.
        /// </summary>
        /// <param name="index">The index of the new selected Material</param>
        /// <param name="name">The name of the new selected Material</param>
        private void OnMaterialChange(Material material)
        {
            Current = material;
            SwitchMaterial();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            InitializeLights();
            InitializeEffect();
            InitializeTextures();
            InitializeSphere();
            InitializeLightBox();

            SpriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");


            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            SphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;



            var worldView = SphereWorld * Camera.View;
            SphereEffect.Parameters["matWorld"].SetValue(SphereWorld);
            SphereEffect.Parameters["matWorldViewProj"].SetValue(worldView * Camera.Projection);
            SphereEffect.Parameters["matInverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(SphereWorld)));

            Sphere.Meshes.FirstOrDefault().Draw();

            for (int index = 0; index < Lights.Count; index++)
            {
                LightBox.Effect.DiffuseColor = Lights[index].ShowColor;
                LightBox.Draw(Matrix.CreateTranslation(Lights[index].Position), Camera.View, Camera.Projection);
            }


            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            SphereEffect.Dispose();
            LightBox.Dispose();

            base.UnloadContent();
        }
        
        private void InitializeSphere()
        {
            // Got to set a texture, else the translation to mesh does not map UV
            Sphere = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            SphereWorld = Matrix.CreateScale(30f) * Matrix.CreateRotationX(MathF.PI * 0.5f);

            // Apply the effect to all mesh parts
            Sphere.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = SphereEffect;
        }

        private void InitializeEffect()
        {
            SphereEffect = Game.Content.Load<Effect>(ContentFolderEffects + "PBR");
            SphereEffect.CurrentTechnique = SphereEffect.Techniques["PBR"];

            var positions = SphereEffect.Parameters["lightPositions"].Elements;
            var colors = SphereEffect.Parameters["lightColors"].Elements;


            for(int index = 0; index < Lights.Count; index++)
            {
                var light = Lights[index];
                positions[index].SetValue(light.Position);
                colors[index].SetValue(light.Color);
            }
        }

        private void InitializeLightBox()
        {
            LightBox = new CubePrimitive(GraphicsDevice, 10, Color.White);
            LightBox.Effect.LightingEnabled = false;
        }

        private void InitializeLights()
        {
            Lights = new List<Light>();
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

            Lights.Add(lightOne);
            Lights.Add(lightTwo);
            Lights.Add(lightThree);
            Lights.Add(lightFour);

            Lights = Lights.ConvertAll(light => light.GenerateShowColor());
        }

        private void InitializeTextures()
        {
            UpdateMaterialPath();
            LoadTextures();
        }

        private void LoadTextures()
        {
            normals = Game.Content.Load<Texture2D>(TexturePath + "normal");
            ao = Game.Content.Load<Texture2D>(TexturePath + "ao");
            metalness = Game.Content.Load<Texture2D>(TexturePath + "metalness");
            roughness = Game.Content.Load<Texture2D>(TexturePath + "roughness");
            albedo = Game.Content.Load<Texture2D>(TexturePath + "color");

            SphereEffect.Parameters["albedoTexture"]?.SetValue(albedo);
            SphereEffect.Parameters["normalTexture"]?.SetValue(normals);
            SphereEffect.Parameters["metallicTexture"]?.SetValue(metalness);
            SphereEffect.Parameters["roughnessTexture"]?.SetValue(roughness);
            SphereEffect.Parameters["aoTexture"]?.SetValue(ao);
        }
        
        private void UpdateMaterialPath()
        {
            TexturePath = ContentFolderTextures + "pbr/";
            switch (Current)
            {
                case Material.RustedMetal:
                    TexturePath += "harsh-metal";
                    break;

                case Material.Marble:
                    TexturePath += "marble";
                    break;

                case Material.Gold:
                    TexturePath += "gold";
                    break;

                case Material.Metal:
                    TexturePath += "metal";
                    break;

                case Material.Grass:
                    TexturePath += "ground";
                    break;
            }
            TexturePath += "/";
        }

        private void SwitchMaterial()
        {
            // We do not dispose textures, as they cannot be loaded again
            UpdateMaterialPath();
            LoadTextures();
        }

    }
}