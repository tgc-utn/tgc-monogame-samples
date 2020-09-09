using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Transformations
{
    /// <summary>
    ///     Solar System:
    ///     Units Involved:
    ///     # Unit 3 - 3D Basics - Transformations
    ///     Shows how to concatenate transformations to generate movements of planets in the solar system.
    ///     Author: Matias Leone, Leandro Barbagallo.
    /// </summary>
    public class SolarSystem : TGCSample
    {
        private const float AxisRotationSpeed = 0.125f;
        private const float EarthAxisRotationSpeed = 2.5f;
        private const float EarthOrbitSpeed = 0.5f;
        private const float MoonOrbitSpeed = 2.5f;

        private const float EarthOrbitOffset = 700;
        private const float MoonOrbitOffset = 80;

        // Scales of each of the celestial bodies.
        private readonly Vector3 _earthScale = new Vector3(3, 3, 3);
        private readonly Vector3 _moonScale = new Vector3(0.5f, 0.5f, 0.5f);
        private readonly Vector3 _sunScale = new Vector3(12, 12, 12);

        /// <inheritdoc />
        public SolarSystem(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Solar System";
            Description =
                "Shows how to concatenate transformations to generate movements of planets in the solar system. You can move around the scene with a simple camera that handles asdw and arrows keys.";
        }

        private float AxisRotation { get; set; }
        private Camera Camera { get; set; }
        private SpherePrimitive Sun { get; set; }
        private Matrix SunTranslation { get; set; }
        private SpherePrimitive Earth { get; set; }
        private float EarthAxisRotation { get; set; }
        private float EarthOrbitRotation { get; set; }
        private Matrix EarthTranslation { get; set; }
        private SpherePrimitive Moon { get; set; }
        private float MoonOrbitRotation { get; set; }
        private Matrix MoonTranslation { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 100, 1500), Vector3.Zero, 1,
                3000);

            Sun = new SpherePrimitive(GraphicsDevice, 20, 32, Color.MonoGameOrange);
            Earth = new SpherePrimitive(GraphicsDevice, 20, 32, Color.LightSkyBlue);
            Moon = new SpherePrimitive(GraphicsDevice, 20, 32, Color.LightSlateGray);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
            // Update transformation of the sun.
            SunTranslation = GetSunTransform();
            // Update transformation of the earth.
            EarthTranslation = GetEarthTransform();
            // Update transformation of the moon.
            MoonTranslation = GetMoonTransform(EarthTranslation);

            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            AxisRotation += AxisRotationSpeed * elapsedTime;
            EarthAxisRotation += EarthAxisRotationSpeed * elapsedTime;
            EarthOrbitRotation += EarthOrbitSpeed * elapsedTime;
            MoonOrbitRotation += MoonOrbitSpeed * elapsedTime;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(Camera.View, Camera.Projection);

            // Render the sun.
            DrawGeometry(Sun, SunTranslation);

            // Render the earth.
            DrawGeometry(Earth, EarthTranslation);

            // Render the moon.
            DrawGeometry(Moon, MoonTranslation);

            base.Draw(gameTime);
        }

        private Matrix GetSunTransform()
        {
            var scale = Matrix.CreateScale(_sunScale);
            var yRot = Matrix.CreateRotationY(AxisRotation);

            return scale * yRot;
        }

        private Matrix GetEarthTransform()
        {
            var scale = Matrix.CreateScale(_earthScale);
            var yRot = Matrix.CreateRotationY(EarthAxisRotation);
            var sunOffset = Matrix.CreateTranslation(EarthOrbitOffset, 0, 0);
            var earthOrbit = Matrix.CreateRotationY(EarthOrbitRotation);

            return scale * yRot * sunOffset * earthOrbit;
        }

        private Matrix GetMoonTransform(Matrix earthTransform)
        {
            var scale = Matrix.CreateScale(_moonScale);
            var yRot = Matrix.CreateRotationY(AxisRotation);
            var earthOffset = Matrix.CreateTranslation(MoonOrbitOffset, 0, 0);
            var moonOrbit = Matrix.CreateRotationY(MoonOrbitRotation);

            return scale * yRot * earthOffset * moonOrbit * earthTransform;
        }

        /// <summary>
        ///     Draw the geometry applying a rotation and translation.
        /// </summary>
        /// <param name="geometry">The geometry to draw.</param>
        /// <param name="transform">The transform to apply.</param>
        private void DrawGeometry(GeometricPrimitive geometry, Matrix transform)
        {
            var effect = geometry.Effect;

            effect.World = transform;
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;

            geometry.Draw(effect);
        }
    }
}