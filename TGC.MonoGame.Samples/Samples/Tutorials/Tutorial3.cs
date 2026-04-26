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
    ///     Author: Rene Juan Rico Mendoza.
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

        private Camera _camera;
        private CubePrimitive _box;
        private Vector3 _boxPosition;
        private CylinderPrimitive _cylinder;
        private Vector3 _cylinderPosition;
        private SpherePrimitive _sphere;
        private Vector3 _spherePosition;
        private TeapotPrimitive _teapot;
        private Vector3 _teapotPosition;
        private TorusPrimitive _torus;
        private Vector3 _torusPosition;
        private TrianglePrimitive _triangle;
        private float _yaw;
        private float _pitch;
        private float _roll;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            _camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);

            _box = new CubePrimitive(GraphicsDevice, 10, Color.DarkCyan, Color.DarkMagenta, Color.DarkGreen,
                Color.MonoGameOrange, Color.Black, Color.DarkGray);
            _boxPosition = Vector3.Zero;
            _cylinder = new CylinderPrimitive(GraphicsDevice, 20, 10, 16);
            _cylinderPosition = new Vector3(-20, 0, 0);
            _sphere = new SpherePrimitive(GraphicsDevice, 10);
            _spherePosition = new Vector3(0, -15, 0);
            _teapot = new TeapotPrimitive(GraphicsDevice, 10);
            _teapotPosition = new Vector3(20, 0, 0);
            _torus = new TorusPrimitive(GraphicsDevice, 10, 1, 16);
            _torusPosition = new Vector3(-20, 15, 0);
            _triangle = new TrianglePrimitive(GraphicsDevice, new Vector3(-10f, 10f, 0f), new Vector3(0f, 20f, 0f),
                new Vector3(10f, 10f, 0f), Color.Black, Color.Cyan, Color.Magenta);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var time = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            _yaw += time * 0.4f;
            _pitch += time * 0.8f;
            _roll += time * 0.9f;

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Save the past RasterizerState
            var oldRasterizerState = GraphicsDevice.RasterizerState;
            // Use a RasterizerState which has Back-Face Culling disabled
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            DrawGeometry(_box, _boxPosition, _yaw, _pitch, _roll);
            DrawGeometry(_cylinder, _cylinderPosition, _yaw, _pitch, _roll);
            DrawGeometry(_sphere, _spherePosition, -_yaw, _pitch, _roll);
            DrawGeometry(_teapot, _teapotPosition, _yaw, -_pitch, _roll);
            DrawGeometry(_torus, _torusPosition, _yaw, _pitch, -_roll);

            // Restore the old RasterizerState
            GraphicsDevice.RasterizerState = oldRasterizerState;

            var triangleEffect = _triangle.Effect;
            triangleEffect.World = Matrix.Identity;
            triangleEffect.View = _camera.View;
            triangleEffect.Projection = _camera.Projection;
            triangleEffect.LightingEnabled = false;
            _triangle.Draw(triangleEffect);

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
            effect.View = _camera.View;
            effect.Projection = _camera.Projection;

            geometry.Draw(effect);
        }
    }
}