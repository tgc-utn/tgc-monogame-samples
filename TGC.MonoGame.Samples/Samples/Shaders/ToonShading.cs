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
        /// An effect to draw using toon shading techniques
        /// </summary>
        private Effect _effect;

        /// <summary>
        /// Texture to use as Look Up Table
        /// </summary>
        private Texture2D _lookUpTable;

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
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);

            _floorWorld = Matrix.CreateScale(300f) * Matrix.CreateTranslation(0f, -20f, 0f);

            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere mesh and floor, from runtime generated primitives

            _model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            
            var aabb = BoundingVolumesExtensions.CreateAABBFrom(_model);
            var height = (aabb.Max.Y - aabb.Min.Y) / 2f;

            _modelWorld = Matrix.CreateTranslation(0f, height, 0f);

            _floor = new QuadPrimitive(GraphicsDevice);
            
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "ToonShading");

            foreach (var modelMesh in _model.Meshes)
            {
                foreach (var meshPart in modelMesh.MeshParts)
                {
                    meshPart.Effect.Dispose();
                    meshPart.Effect = _effect;
                }
            }
            
            _lightBox = new CubePrimitive(GraphicsDevice, 1f, Color.White);
            
            _lookUpTable = Game.Content.Load<Texture2D>(ContentFolderTextures + "toon/lut");
            _effect.Parameters["LookUpTableTexture"].SetValue(_lookUpTable);

            // Add options to pick the blinn phong type
            ModifierController.AddOptions("Toon Shading Type", new[]
            {
                "Default",
                "Look Up Table",
            }, ToonShadingType.DEFAULT, ToonShadingTypeChange);
            

            // Add mappings for modifiers to control values
            ModifierController.AddVector("Light Position", SetLightPosition, new Vector3(70f, 160f, 40f));
            
            ModifierController.AddColor("Color A", _effect.Parameters["colorA"], new Color(0.152f, 0f, 0.072f));
            ModifierController.AddColor("Color B", _effect.Parameters["colorB"], new Color(0.373f, 0.105f, 0.009f));
            ModifierController.AddColor("Color C", _effect.Parameters["colorC"], new Color(1f, 0.529f, 0f));
            ModifierController.AddColor("Color D", _effect.Parameters["colorD"], new Color(1f, 0.794f, 0f));

            ModifierController.AddVector("Color Range", _effect.Parameters["colorRange"], new Vector3(0.2f, 0.4f, 0.75f));
            ModifierController.AddFloat("Ambient", _effect.Parameters["KAmbient"], 0.05f, 0f, 1f);

            ModifierController.AddTexture("LUT", _lookUpTable);

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
            // Set the background color to black
            Game.Background = Color.Black;
            
            var viewProjection = _camera.View * _camera.Projection;
            
            // Draw the model, pass the World, WorldViewProjection and InverseTransposeWorld matrices
            _effect.Parameters["World"].SetValue(_modelWorld);
            _effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(_modelWorld)));
            _effect.Parameters["WorldViewProjection"].SetValue(_modelWorld * viewProjection);

            foreach (var modelMesh in _model.Meshes)
            { 
                modelMesh.Draw(); 
            }

            // Draw the floor, pass the World, WorldViewProjection and InverseTransposeWorld matrices
            _effect.Parameters["World"].SetValue(_floorWorld);
            _effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(_floorWorld)));
            _effect.Parameters["WorldViewProjection"].SetValue(_floorWorld * viewProjection);

            _floor.Draw(_effect);

            // Draw a box to show where the light is
            _lightBox.Draw(_lightBoxWorld, _camera.View, _camera.Projection);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Processes a change in the toon shader type.
        /// </summary>
        /// <param name="type">The new toon shader type to use</param>
        private void ToonShadingTypeChange(ToonShadingType type)
        {
            if (type.Equals(ToonShadingType.LOOKUPTABLE))
            {
                _effect.CurrentTechnique = _effect.Techniques["LookUpTable"];
            }
            else
            {
                _effect.CurrentTechnique = _effect.Techniques["Default"];
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
