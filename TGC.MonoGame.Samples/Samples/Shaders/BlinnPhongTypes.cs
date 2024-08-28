using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        private Camera _camera;

        /// <summary>
        /// A model to draw in the center of the scene
        /// </summary>
        private Model _model;

        /// <summary>
        /// Geometry to draw a floor
        /// </summary>
        private QuadPrimitive _floor;

        /// <summary>
        /// A box to draw where the light is
        /// </summary>
        private CubePrimitive _lightBox;

        /// <summary>
        /// An effect to draw using blinn-phong techniques
        /// </summary>
        private Effect _effect;

        /// <summary>
        /// Floor color texture
        /// </summary>
        private Texture2D _floorTexture;

        /// <summary>
        /// Floor normal map
        /// </summary>
        private Texture2D _floorNormalMap;

        /// <summary>
        /// Model color texture
        /// </summary>
        private Texture2D _modelTexture;

        /// <summary>
        /// Model normal map
        /// </summary>
        private Texture2D _modelNormal;

        /// <summary>
        /// The position of the light
        /// </summary>
        private Vector3 _lightPosition = Vector3.Zero;

        /// <summary>
        /// The world matrix for the light box
        /// </summary>
        private Matrix _lightBoxWorld = Matrix.Identity;

        /// <summary>
        /// The world matrix for the model
        /// </summary>
        private Matrix _modelWorld;

        /// <summary>
        /// The world matrix for the floor
        /// </summary>
        private Matrix _floorWorld;
        
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
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);

            _modelWorld = Matrix.CreateScale(15f) * Matrix.CreateRotationX(-MathF.PI * 0.5f);
            _floorWorld = Matrix.CreateScale(300f) * Matrix.CreateTranslation(0f, -20f, 0f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere mesh and floor, from runtime generated primitives
            _model = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            _floor = new QuadPrimitive(GraphicsDevice);

            _lightBox = new CubePrimitive(GraphicsDevice, 1f, Color.White);

            _floorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tiling-base");
            _floorNormalMap = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tiling-normal");
            _modelTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/harsh-metal/color");
            _modelNormal = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/harsh-metal/normal");

            // We load the effect with various techniques
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhongTypes");

            foreach (var modelMesh in _model.Meshes)
            {
                foreach (var meshPart in modelMesh.MeshParts)
                { 
                    meshPart.Effect = _effect; 
                }
            }

            // Add options to pick the blinn phong type
            ModifierController.AddOptions("Blinn Phong Type", new[]
            {
                "Default",
                "Gouraud",
                "Normal Mapping"
            }, BlinnPhongType.DEFAULT, BlinnPhongTypeChange);

            // Add mappings for modifiers to control values
            ModifierController.AddVector("Light Position", SetLightPosition, Vector3.Up * 45f);
            ModifierController.AddColor("Ambient Color", _effect.Parameters["ambientColor"], new Color(0f, 0f, 1f));
            ModifierController.AddColor("Diffuse Color", _effect.Parameters["diffuseColor"], new Color(0.1f, 0.1f, 0.6f));
            ModifierController.AddColor("Specular Color", _effect.Parameters["specularColor"], Color.White);

            ModifierController.AddFloat("KA", _effect.Parameters["KAmbient"], 0.1f, 0f, 1f);
            ModifierController.AddFloat("KD", _effect.Parameters["KDiffuse"], 0.7f, 0f, 1f);
            ModifierController.AddFloat("KS", _effect.Parameters["KSpecular"], 0.4f, 0f, 1f);
            ModifierController.AddFloat("Shininess", _effect.Parameters["shininess"], 4.0f, 1f, 64f);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.LoadContent();
        }

        /// <summary>
        /// Sets the light position from the modifier.
        /// </summary>
        /// <param name="position">The new light position in world space</param>
        private void SetLightPosition(Vector3 position)
        {
            _lightBoxWorld = Matrix.CreateScale(3f) *  Matrix.CreateTranslation(position);
            _effect.Parameters["lightPosition"].SetValue(position);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Set the background color to black, and use the default depth configuration
            Game.Background = Color.Black;

            var viewProjection = _camera.View * _camera.Projection;

            // Set the camera position
            // This changes every update so we need to set it before rendering
            _effect.Parameters["eyePosition"].SetValue(_camera.Position);

            _effect.Parameters["ModelTexture"].SetValue(_modelTexture);
            _effect.Parameters["NormalTexture"].SetValue(_modelNormal);
            _effect.Parameters["World"].SetValue(_modelWorld);
            _effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(_modelWorld)));
            _effect.Parameters["WorldViewProjection"].SetValue(_modelWorld * viewProjection);
            _effect.Parameters["Tiling"].SetValue(Vector2.One);

            foreach (var modelMesh in _model.Meshes)
            { 
                modelMesh.Draw(); 
            }

            _effect.Parameters["ModelTexture"].SetValue(_floorTexture);
            _effect.Parameters["NormalTexture"].SetValue(_floorNormalMap);
            _effect.Parameters["World"].SetValue(_floorWorld);
            _effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(_floorWorld)));
            _effect.Parameters["WorldViewProjection"].SetValue(_floorWorld * viewProjection);
            _effect.Parameters["Tiling"].SetValue(Vector2.One * 5f);

            _floor.Draw(_effect);

            _lightBox.Draw(_lightBoxWorld, _camera.View, _camera.Projection);
          
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
                    _effect.CurrentTechnique = _effect.Techniques["Gouraud"];
                    break;
                case BlinnPhongType.NORMAL_MAPPING:
                    _effect.CurrentTechnique = _effect.Techniques["NormalMapping"];
                    break;
                default:
                    _effect.CurrentTechnique = _effect.Techniques["Default"];
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
