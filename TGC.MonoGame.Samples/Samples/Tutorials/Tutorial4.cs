using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 4:
    ///     Units Involved:
    ///     # Unit 4 - Textures and lighting - Textures
    ///     Shows how to create a Quad and a Box with a 2D image as a texture to give it color.
    ///     Author: Matías Leone.
    /// </summary>
    public class Tutorial4 : TGCSample
    {
        /// <inheritdoc />
        public Tutorial4(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 4";
            Description = "Shows how to create a Quad and a Box with a 2D image as a texture to give it color.";
        }

        private Camera Camera { get; set; }
        private QuadPrimitive Quad { get; set; }
        private CubePrimitive Cube { get; set; }
        private Matrix CubeWorld { get; set; }
        private float BoxRotation { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 10, 60), Vector3.Zero);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            base.LoadContent();

            var texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-3");

            var effect = Game.Content.Load<Effect>(ContentFolderEffects + "DiffuseTexture");

            Quad = new QuadPrimitive(GraphicsDevice);
            Quad.Textures.Add(texture);
            Quad.World = Matrix.CreateScale(10f) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitX * 14);
            Quad.SetEffect(effect, EffectInspectionType.ALL); 

            Cube = new CubePrimitive(GraphicsDevice, 10f, Color.White);
            Cube.Textures.Add(texture);
            CubeWorld = Matrix.CreateTranslation(Vector3.UnitX * -14);
            Cube.SetEffect(effect, EffectInspectionType.ALL);

        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
            BoxRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var viewProjection = Camera.View * Camera.Projection;

            Cube.World = Matrix.CreateRotationY(BoxRotation) * CubeWorld;
            Cube.ViewProjection = viewProjection;
            Cube.Draw();

            Quad.ViewProjection = viewProjection;
            Quad.Draw();

            base.Draw(gameTime);
        }
    }
}