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
    public class BlinnPhongTypes : TGCSample
    {

        private Camera Camera { get; set; }
        private Model Model { get; set; }

        private Model Floor { get; set; }
        private CubePrimitive LightBox { get; set; }

        private Effect Effect { get; set; }

        private Texture2D FloorTexture { get; set; }
        private Texture2D FloorNormalMap { get; set; }
        private Texture2D ModelTexture { get; set; }
        private Texture2D ModelNormal { get; set; }

        private Vector3 LightPosition { get; set; } = Vector3.Zero;
        private Matrix LightBoxWorld { get; set; } = Matrix.Identity;

        private Matrix ModelWorld { get; set; }

        private Matrix FloorWorld { get; set; }

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

            base.Initialize();
        }



        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the sphere mesh and floor, from runtime generated primitives
            Model = Game.Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            Floor = Game.Content.Load<Model>(ContentFolder3D + "geometries/plane");
            LightBox = new CubePrimitive(GraphicsDevice, 1f, Color.White);

            FloorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tiling-base");
            FloorNormalMap = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/tiling-normal");
            ModelTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/harsh-metal/color");
            ModelNormal = Game.Content.Load<Texture2D>(ContentFolderTextures + "pbr/harsh-metal/normal");

            // We load the effect in the .fx file
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhongTypes");


            foreach (var modelMesh in Model.Meshes)
                foreach (var meshPart in modelMesh.MeshParts)
                    meshPart.Effect = Effect;

            foreach (var modelMesh in Floor.Meshes)
                foreach (var meshPart in modelMesh.MeshParts)
                    meshPart.Effect = Effect;

            ModifierController.AddOptions("Blinn Phong Type", new[]
            {
                "Default",
                "Gouraud",
                "Normal Mapping"
            }, BlinnPhongType.DEFAULT, BlinnPhongTypeChange);

            ModifierController.AddVector("Light Position", SetLightPosition, Vector3.Up * 45f);
            ModifierController.AddColor("Ambient Color", Effect.Parameters["ambientColor"], new Color(0f, 0f, 1f));
            ModifierController.AddColor("Diffuse Color", Effect.Parameters["diffuseColor"], new Color(0.1f, 0.1f, 0.6f));
            ModifierController.AddColor("Specular Color", Effect.Parameters["specularColor"], Color.White);

            ModifierController.AddFloat("KA", Effect.Parameters["KAmbient"], 0.1f, 0f, 1f);
            ModifierController.AddFloat("KD", Effect.Parameters["KDiffuse"], 0.7f, 0f, 1f);
            ModifierController.AddFloat("KS", Effect.Parameters["KSpecular"], 0.4f, 0f, 1f);
            ModifierController.AddFloat("Shininess", Effect.Parameters["shininess"], 4.0f, 1f, 64f);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            ModelWorld = Matrix.CreateScale(15f) * Matrix.CreateRotationX(-MathF.PI * 0.5f);
            FloorWorld = Matrix.CreateScale(100f) * Matrix.CreateRotationX(-MathF.PI * 0.5f) * Matrix.CreateTranslation(0f, -20f, 0f);

            base.LoadContent();
        }

        private void SetLightPosition(Vector3 position)
        {
            LightBoxWorld = Matrix.CreateTranslation(position) * Matrix.CreateScale(3f);
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
            Effect.Parameters["ambientColor"].SetValue(Effect.Parameters["ambientColor"].GetValueVector3());
            Effect.Parameters["diffuseColor"].SetValue(Effect.Parameters["diffuseColor"].GetValueVector3());
            Effect.Parameters["specularColor"].SetValue(Effect.Parameters["specularColor"].GetValueVector3());
            // Set the camera position
            // This changes every update so we need to set it before rendering
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);

            Effect.Parameters["ModelTexture"].SetValue(ModelTexture);
            Effect.Parameters["NormalTexture"].SetValue(ModelNormal);
            Effect.Parameters["World"].SetValue(ModelWorld);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(ModelWorld)));
            Effect.Parameters["WorldViewProjection"].SetValue(ModelWorld * viewProjection);

            foreach (var modelMesh in Model.Meshes)
                modelMesh.Draw();

            Effect.Parameters["ModelTexture"].SetValue(FloorTexture);
            Effect.Parameters["NormalTexture"].SetValue(FloorNormalMap);
            Effect.Parameters["World"].SetValue(FloorWorld);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(FloorWorld)));
            Effect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);

            foreach (var modelMesh in Floor.Meshes)
                modelMesh.Draw();


            LightBox.Draw(LightBoxWorld, Camera.View, Camera.Projection);
          
            base.Draw(gameTime);
        }


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
                case BlinnPhongType.DEFAULT:
                    Effect.CurrentTechnique = Effect.Techniques["Default"];
                    break;

            }
        }

        private enum BlinnPhongType
        {
            DEFAULT,
            GOURAUD,
            NORMAL_MAPPING
        }
    }
}
