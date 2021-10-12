using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Models;
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

        private ModelDrawer ModelDrawer { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 100f), size);
            Camera.FarPlane = 100000.0f;
            Camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 1f, Camera.FarPlane, Camera.DefaultFieldOfViewDegrees);

            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            // We load the city meshes into a model
            Model = Game.Content.Load<Model>(ContentFolder3D + "sponza/Sponza");
            ModelDrawer = new ModelDrawer(Model, GraphicsDevice);

            // We load the effect in the .fx file
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");
            
            ModelDrawer.SetEffect(Effect);
            ModelDrawer.WorldViewProjectionMatrixParameter = Effect.Parameters["WorldViewProjection"];
            ModelDrawer.WorldMatrixParameter = Effect.Parameters["World"];
            ModelDrawer.NormalMatrixParameter = Effect.Parameters["InverseTransposeWorld"];
            ModelDrawer.TextureParameter = Effect.Parameters["baseTexture"];
            

            // Set uniforms
            Effect.Parameters["ambientColor"].SetValue(new Vector3(0.25f, 0.0f, 0.0f));
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            Effect.Parameters["KAmbient"].SetValue(0.1f);
            Effect.Parameters["KDiffuse"].SetValue(1.0f);
            Effect.Parameters["KSpecular"].SetValue(0.8f);
            Effect.Parameters["shininess"].SetValue(16.0f);

            lightBox = new CubePrimitive(GraphicsDevice, 25, Color.Blue);

            ModifierController.AddFloat("KA", Effect.Parameters["KAmbient"], 0.2f, 0f, 1f);
            ModifierController.AddFloat("KD", Effect.Parameters["KDiffuse"], 0.7f, 0f, 1f);
            ModifierController.AddFloat("KS", Effect.Parameters["KSpecular"], 0.4f, 0f, 1f);
            ModifierController.AddFloat("Shininess", Effect.Parameters["shininess"], 4.0f, 1f, 64f);
            ModifierController.AddColor("Ambient Color", Effect.Parameters["ambientColor"], new Color(0.25f, 0f, 0f));
            ModifierController.AddColor("Diffuse Color", Effect.Parameters["diffuseColor"], new Color(0.1f, 0.1f, 0.6f));
            ModifierController.AddColor("Specular Color", Effect.Parameters["specularColor"], Color.White);


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
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            ModelDrawer.Draw(Matrix.CreateScale(0.01f), Camera.View * Camera.Projection);
            //Model.Draw(Matrix.CreateScale(0.01f), Camera.View, Camera.Projection);

            lightBox.Draw(LightBoxWorld, Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }
    }
}