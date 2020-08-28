using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 3:
    ///     This sample shows how to draw 3D geometric primitives such as cubes, spheres and cylinders.
    ///     Author: René Juan Rico Mendoza.
    /// </summary>
    public class Tutorial3 : TGCSample
    {
        /// <inheritdoc />
        public Tutorial3(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 3";
            Description = "This sample shows how to draw 3D geometric primitives such as cubes, spheres and cylinders.";
        }

        private Camera Camera { get; set; }
        private BoxPrimitive Box { get; set; }
        private Vector3 BoxPosition { get; set; }
        private CylinderPrimitive Cylinder { get; set; }
        private Vector3 CylinderPosition { get; set; }
        private SpherePrimitive Sphere { get; set; }
        private Vector3 SpherePosition { get; set; }
        private TeapotPrimitive Teapot { get; set; }
        private Vector3 TeapotPosition { get; set; }
        private TorusPrimitive Torus { get; set; }
        private Vector3 TorusPosition { get; set; }
        private TrianglePrimitive Triangle { get; set; }
        private float Yaw { get; set; }
        private float Pitch { get; set; }
        private float Roll { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);

            Box = new BoxPrimitive(GraphicsDevice, new Vector3(10, 10, 10), Vector3.Zero, Color.Cyan, Color.Black,
                Color.Magenta, Color.Yellow, Color.Green, Color.Blue, Color.Red, Color.White);
            BoxPosition = Vector3.Zero;
            Cylinder = new CylinderPrimitive(GraphicsDevice, 20, 10, 16);
            CylinderPosition = new Vector3(-20, 0, 0);
            Sphere = new SpherePrimitive(GraphicsDevice, 10, 16);
            SpherePosition = new Vector3(0, -15, 0);
            Teapot = new TeapotPrimitive(GraphicsDevice, 10, 8);
            TeapotPosition = new Vector3(20, 0, 0);
            Torus = new TorusPrimitive(GraphicsDevice, 10, 1, 16);
            TorusPosition = new Vector3(-20, 15, 0);
            Triangle = new TrianglePrimitive(GraphicsDevice, new Vector3(-10f, 10f, 0f), new Vector3(0f, 20f, 0f),
                new Vector3(10f, 10f, 0f), Color.Black, Color.Cyan, Color.Magenta);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var time = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            Yaw += time * 0.4f;
            Pitch += time * 0.8f;
            Roll += time * 0.9f;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(Camera.View, Camera.Projection);

            DrawGeometry(Box, BoxPosition, Yaw, Pitch, Roll);
            DrawGeometry(Cylinder, CylinderPosition, Yaw, Pitch, Roll);
            DrawGeometry(Sphere, SpherePosition, -Yaw, Pitch, Roll);
            DrawGeometry(Teapot, TeapotPosition, Yaw, -Pitch, Roll);
            DrawGeometry(Torus, TorusPosition, Yaw, Pitch, -Roll);

            var triangleEffect = Triangle.Effect;
            triangleEffect.World = Matrix.Identity;
            triangleEffect.View = Camera.View;
            triangleEffect.Projection = Camera.Projection;
            Triangle.Draw(triangleEffect);

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Draw the geometry applying a rotation and translation.
        /// </summary>
        /// <param name="geometry">The geometry to draw.</param>
        /// <param name="position">The position of the geometry.</param>
        /// <param name="yaw">Vertical axis (yaw).</param>
        /// <param name="pitch">Transverse axis (pitch).</param>
        /// <param name="roll">Longitudinal axis (roll).</param>
        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw, float pitch, float roll)
        {
            var effect = geometry.Effect;

            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position);
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;

            geometry.Draw(effect);
        }
    }
}