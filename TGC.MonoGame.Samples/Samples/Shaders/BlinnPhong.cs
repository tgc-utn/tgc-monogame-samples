using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;
using TGC.MonoGame.Samples.Viewer.GUI.Modifiers;

namespace TGC.MonoGame.Samples.Samples.Shaders
{
    public class BlinnPhong : TGCSample
    {
        private CubePrimitive lightBox;

        /// <inheritdoc />
        public BlinnPhong(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Shaders;
            Name = "Blinn Phong";
            Description = "Applying Blinn-Phong to a scene";
        }

        private Camera Camera { get; set; }
        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private Matrix LightBoxWorld { get; set; } = Matrix.Identity;
        private float Timer { get; set; }


        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);

            base.Initialize();
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Model = Game.Content.Load<Model>(ContentFolder3D + "scene/city");

            // We get the mesh texture. All mesh parts use the same texture so we are fine
            var texture = ((BasicEffect) Model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            // We load the effect in the .fx file
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");

            // We assign the effect to each one of the models
            foreach (var modelMesh in Model.Meshes)
            foreach (var meshPart in modelMesh.MeshParts)
                meshPart.Effect = Effect;

            // Set the texture. This won't change on this effect so we can assign it here
            Effect.Parameters["baseTexture"].SetValue(texture);

            // Set uniforms
            Effect.Parameters["ambientColor"].SetValue(new Vector3(0.25f, 0.0f, 0.0f));
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            Effect.Parameters["KAmbient"].SetValue(0.1f);
            Effect.Parameters["KDiffuse"].SetValue(1.0f);
            Effect.Parameters["KSpecular"].SetValue(0.8f);
            Effect.Parameters["shininess"].SetValue(16.0f);

            lightBox = new CubePrimitive(GraphicsDevice, 25, Color.Blue);

            Modifiers = new IModifier[]
            {
                new FloatModifier("KA", Effect.Parameters["KAmbient"], 0.2f, 0f, 1f),
                new FloatModifier("KD", Effect.Parameters["KDiffuse"], 0.7f, 0f, 1f),
                new FloatModifier("KS", Effect.Parameters["KSpecular"], 0.4f, 0f, 1f),
                new FloatModifier("Shininess", Effect.Parameters["shininess"], 4.0f, 1f, 64f),
                new ColorModifier("Ambient Color", Effect.Parameters["ambientColor"], new Color(0.25f, 0f, 0f)),
                new ColorModifier("Diffuse Color", Effect.Parameters["diffuseColor"], new Color(0.1f, 0.1f, 0.6f)),
                new ColorModifier("Specular Color", Effect.Parameters["specularColor"], Color.White)
            };

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera
            Camera.Update(gameTime);

            // Rotate our light position in a circle up in the sky
            var lightPosition = new Vector3((float) Math.Cos(Timer) * 700f, 800f, (float) Math.Sin(Timer) * 700f);
            LightBoxWorld = Matrix.CreateTranslation(lightPosition);

            // Set the light position and camera position
            // These change every update so we need to set them on every update call
            Effect.Parameters["lightPosition"].SetValue(lightPosition);
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);

            Timer += (float) gameTime.ElapsedGameTime.TotalSeconds;

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Set the background color to black, and use the default depth configuration
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var modelMesh in Model.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];
                // World is used to transform from model space to world space
                Effect.Parameters["World"].SetValue(worldMatrix);
                // InverseTransposeWorld is used to rotate normals
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // WorldViewProjection is used to transform from model space to clip space
                Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * Camera.View * Camera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            lightBox.Draw(LightBoxWorld, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}