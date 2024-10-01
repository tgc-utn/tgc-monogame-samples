using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.PBR
{
    public class BasicPBR : TGCSample
    {
        private Texture2D _albedo;
        private Texture2D _ao;
        private Texture2D _metalness;
        private Texture2D _roughness;
        private Texture2D _normals;
        private Material _current = Material.RustedMetal;

        private CubePrimitive _lightBox;
        private List<Light> _lights;
        private Model _sphere;
        private Effect _sphereEffect;

        private Matrix _sphereWorld;

        private string _texturePath;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="game">The game.</param>
        public BasicPBR(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.PBR;
            Name = "Basic PBR";
            Description = "Regular PBR sample.";
        }

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

        /// <summary>
        ///     Processes a change in the selected Material.
        /// </summary>
        private void OnMaterialChange(Material material)
        {
            _current = material;
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

            ModifierController.AddOptions("Material", new[]
            {
                "RustedMetal",
                "Grass",
                "Gold",
                "Marble",
                "Metal"
            }, Material.RustedMetal, OnMaterialChange);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            _sphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var worldView = _sphereWorld * Camera.View;
            _sphereEffect.Parameters["matWorld"].SetValue(_sphereWorld);
            _sphereEffect.Parameters["matWorldViewProj"].SetValue(worldView * Camera.Projection);
            _sphereEffect.Parameters["matInverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(_sphereWorld)));

            _sphere.Meshes.FirstOrDefault().Draw();

            for (var index = 0; index < _lights.Count; index++)
            {
                _lightBox.Effect.DiffuseColor = _lights[index].ShowColor;
                _lightBox.Draw(Matrix.CreateTranslation(_lights[index].Position), Camera.View, Camera.Projection);
            }

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            _sphereEffect.Dispose();
            _lightBox.Dispose();

            base.UnloadContent();
        }

        private void InitializeSphere()
        {
            // Got to set a texture, else the translation to mesh does not map UV
            _sphere = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            _sphereWorld = Matrix.CreateScale(30f) * Matrix.CreateRotationX(MathF.PI * 0.5f);

            // Apply the effect to all mesh parts
            _sphere.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = _sphereEffect;
        }

        private void InitializeEffect()
        {
            _sphereEffect = Game.Content.Load<Effect>(ContentFolderEffects + "PBR");
            _sphereEffect.CurrentTechnique = _sphereEffect.Techniques["PBR"];

            var positions = _sphereEffect.Parameters["lightPositions"].Elements;
            var colors = _sphereEffect.Parameters["lightColors"].Elements;

            for (var index = 0; index < _lights.Count; index++)
            {
                var light = _lights[index];
                positions[index].SetValue(light.Position);
                colors[index].SetValue(light.Color);
            }
        }

        private void InitializeLightBox()
        {
            _lightBox = new CubePrimitive(GraphicsDevice, 10, Color.White);
            _lightBox.Effect.LightingEnabled = false;
        }

        private void InitializeLights()
        {
            _lights = new List<Light>();
            var lightOne = new Light();

            var distance = 45f;

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

            _lights.Add(lightOne);
            _lights.Add(lightTwo);
            _lights.Add(lightThree);
            _lights.Add(lightFour);

            _lights = _lights.ConvertAll(light => light.GenerateShowColor());
        }

        private void InitializeTextures()
        {
            UpdateMaterialPath();
            LoadTextures();
        }

        private void LoadTextures()
        {
            _normals = Game.Content.Load<Texture2D>(_texturePath + "normal");
            _ao = Game.Content.Load<Texture2D>(_texturePath + "ao");
            _metalness = Game.Content.Load<Texture2D>(_texturePath + "metalness");
            _roughness = Game.Content.Load<Texture2D>(_texturePath + "roughness");
            _albedo = Game.Content.Load<Texture2D>(_texturePath + "color");

            _sphereEffect.Parameters["albedoTexture"]?.SetValue(_albedo);
            _sphereEffect.Parameters["normalTexture"]?.SetValue(_normals);
            _sphereEffect.Parameters["metallicTexture"]?.SetValue(_metalness);
            _sphereEffect.Parameters["roughnessTexture"]?.SetValue(_roughness);
            _sphereEffect.Parameters["aoTexture"]?.SetValue(_ao);
        }

        private void UpdateMaterialPath()
        {
            _texturePath = ContentFolderTextures + "pbr/";
            switch (_current)
            {
                case Material.RustedMetal:
                    _texturePath += "harsh-metal";
                    break;

                case Material.Marble:
                    _texturePath += "marble";
                    break;

                case Material.Gold:
                    _texturePath += "gold";
                    break;

                case Material.Metal:
                    _texturePath += "metal";
                    break;

                case Material.Grass:
                    _texturePath += "ground";
                    break;
            }

            _texturePath += "/";
        }

        private void SwitchMaterial()
        {
            // We do not dispose textures, as they cannot be loaded again
            UpdateMaterialPath();
            LoadTextures();
        }
    }
}