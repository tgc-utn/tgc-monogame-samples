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
        private const float AXIS_ROTATION_SPEED = 0.125f;
        private const float EARTH_AXIS_ROTATION_SPEED = 2.5f;
        private const float EARTH_ORBIT_SPEED = 0.5f;
        private const float MOON_ORBIT_SPEED = 2.5f;

        private const float EARTH_ORBIT_OFFSET = 700;
        private const float MOON_ORBIT_OFFSET = 80;

        private readonly Vector3 EARTH_SCALE = new Vector3(3, 3, 3);
        private readonly Vector3 MOON_SCALE = new Vector3(0.5f, 0.5f, 0.5f);

        //Escalas de cada uno de los astros
        private readonly Vector3 SUN_SCALE = new Vector3(12, 12, 12);
        private float AxisRotation;

        /// <inheritdoc />
        public SolarSystem(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Transformations;
            Name = "Solar System";
            Description =
                "Shows how to concatenate transformations to generate movements of planets in the solar system. You can move around the scene with a simple camera that handles asdw and arrows keys.";
        }

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
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 100, 1500), 5);

            Sun = new SpherePrimitive(GraphicsDevice, 10, 16, Color.MonoGameOrange);
            Earth = new SpherePrimitive(GraphicsDevice, 10, 16, Color.LightSkyBlue);
            Moon = new SpherePrimitive(GraphicsDevice, 10, 16, Color.LightSlateGray);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            //TODO load textures to set in the spheres.
            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);
            //Actualizar transformacion el sol
            SunTranslation = GetSunTransform();
            //Actualizar transformacion la tierra
            EarthTranslation = GetEarthTransform();
            //Actualizar transformacion la luna
            MoonTranslation = GetMoonTransform(EarthTranslation);

            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            AxisRotation += AXIS_ROTATION_SPEED * elapsedTime;
            EarthAxisRotation += EARTH_AXIS_ROTATION_SPEED * elapsedTime;
            EarthOrbitRotation += EARTH_ORBIT_SPEED * elapsedTime;
            MoonOrbitRotation += MOON_ORBIT_SPEED * elapsedTime;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            AxisLines.Draw(Camera.View, Camera.Projection);

            //Renderizar el sol
            DrawGeometry(Sun, SunTranslation);

            //Renderizar la tierra
            DrawGeometry(Earth, EarthTranslation);

            //Renderizar la luna
            DrawGeometry(Moon, MoonTranslation);

            base.Draw(gameTime);
        }

        private Matrix GetSunTransform()
        {
            var scale = Matrix.CreateScale(SUN_SCALE);
            var yRot = Matrix.CreateRotationY(AxisRotation);

            return scale * yRot;
        }

        private Matrix GetEarthTransform()
        {
            var scale = Matrix.CreateScale(EARTH_SCALE);
            var yRot = Matrix.CreateRotationY(EarthAxisRotation);
            var sunOffset = Matrix.CreateTranslation(EARTH_ORBIT_OFFSET, 0, 0);
            var earthOrbit = Matrix.CreateRotationY(EarthOrbitRotation);

            return scale * yRot * sunOffset * earthOrbit;
        }

        private Matrix GetMoonTransform(Matrix earthTransform)
        {
            var scale = Matrix.CreateScale(MOON_SCALE);
            var yRot = Matrix.CreateRotationY(AxisRotation);
            var earthOffset = Matrix.CreateTranslation(MOON_ORBIT_OFFSET, 0, 0);
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