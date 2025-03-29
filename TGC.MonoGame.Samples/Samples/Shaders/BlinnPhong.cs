using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    public class BlinnPhong : TGCSample
    {
        private CubePrimitive _lightBox;

        /// <inheritdoc />
        public BlinnPhong(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Blinn Phong";
            Description = "Applying Blinn-Phong to a scene";
        }

        private Camera _camera;
        private Model _model;
        private Effect _effect;
        private Matrix _lightBoxWorld = Matrix.Identity;
        private float _timer;

        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);

            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            _model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // We get the mesh texture. All mesh parts use the same texture so we are fine
            var texture = ((BasicEffect) _model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // We load the effect in the .fx file
            _effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");

            // We assign the effect to each one of the models
            foreach (var modelMesh in _model.Meshes)
            {
                foreach (var meshPart in modelMesh.MeshParts)
                {
                    meshPart.Effect = _effect;
                }
            }

            // Set the texture. This won't change on this effect so we can assign it here
            _effect.Parameters["baseTexture"].SetValue(texture);

            // Set uniforms
            _effect.Parameters["ambientColor"].SetValue(new Vector3(0.25f, 0.0f, 0.0f));
            _effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            _effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            _effect.Parameters["KAmbient"].SetValue(0.1f);
            _effect.Parameters["KDiffuse"].SetValue(1.0f);
            _effect.Parameters["KSpecular"].SetValue(0.8f);
            _effect.Parameters["shininess"].SetValue(16.0f);

            _lightBox = new CubePrimitive(GraphicsDevice, 25, Color.Blue);

            ModifierController.AddFloat("KA", _effect.Parameters["KAmbient"], 0.2f, 0f, 1f);
            ModifierController.AddFloat("KD", _effect.Parameters["KDiffuse"], 0.7f, 0f, 1f);
            ModifierController.AddFloat("KS", _effect.Parameters["KSpecular"], 0.4f, 0f, 1f);
            ModifierController.AddFloat("Shininess", _effect.Parameters["shininess"], 4.0f, 1f, 64f);
            ModifierController.AddColor("Ambient Color", _effect.Parameters["ambientColor"], new Color(0.25f, 0f, 0f));
            ModifierController.AddColor("Diffuse Color", _effect.Parameters["diffuseColor"], new Color(0.1f, 0.1f, 0.6f));
            ModifierController.AddColor("Specular Color", _effect.Parameters["specularColor"], Color.White);

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            _camera.Update(gameTime);

            // Rotate our light position in a circle up in the sky
            var lightPosition = new Vector3((float) Math.Cos(_timer) * 700f, 800f, (float) Math.Sin(_timer) * 700f);
            _lightBoxWorld = Matrix.CreateTranslation(lightPosition);

            // Set the light position and camera position
            // These change every update so we need to set them on every update call
            _effect.Parameters["lightPosition"].SetValue(lightPosition);
            _effect.Parameters["eyePosition"].SetValue(_camera.Position);

            _timer += (float) gameTime.ElapsedGameTime.TotalSeconds;

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Set the background color to black, and use the default depth configuration
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var modelMesh in _model.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];
                // World is used to transform from model space to world space
                _effect.Parameters["World"].SetValue(worldMatrix);
                // InverseTransposeWorld is used to rotate normals
                _effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // WorldViewProjection is used to transform from model space to clip space
                _effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * _camera.View * _camera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            _lightBox.Draw(_lightBoxWorld, _camera.View, _camera.Projection);

            base.Draw(gameTime);
        }
    }
}