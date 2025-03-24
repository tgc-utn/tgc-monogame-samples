using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions
{
    /// <summary>
    ///     Shows how to test collision for different volumes
    /// </summary>
    public class BoundingVolumesTest : TGCSample
    {
        /// <inheritdoc />
        public BoundingVolumesTest(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Collisions;
            Name = "Bounding Volumes Test";
            Description = "Shows how to test collision for different Bounding Volumes.";
        }

        // Camera to draw the scene
        private Camera _camera;

        // The Model of the Robot to draw
        private Model _robot;

        // BasicEffect to draw the Robot
        private BasicEffect _robotEffect;

        // The World Matrix for the Robot
        private Matrix _robotWorld;

        // The Robot's position
        private Vector3 _robotPosition;

        // The <see cref="BoundingBox">BoundingBox</see> for the Robot
        private BoundingBox _robotBox;

        // The Model of the Chair to draw
        private Model _chair;

        // The World Matrix for the Chair
        private Matrix _chairWorld;

        // The World Matrix for the Chair Oriented Bounding Box
        private Matrix _chairObbWorld;

        // The OrientedBoundingBox of the Chair
        private OrientedBoundingBox _chairBox;
        
        // The Y Angle for the Chair
        private float _chairAngle;

        // The Scale Matrix of the Chair
        private Matrix _chairScale;
        
        // The Translation Matrix of the Chair
        private Matrix _chairTranslation;

        // The Model of the Tank to draw
        private Model _tank;

        // The World Matrix for the Tank
        private Matrix _tankWorld;

        // The BoundingSphere of the Tank
        private BoundingSphere _tankSphere;

        // The position of the Tank
        private Vector3 _tankPosition;

        // The World Matrix for the second Robot
        private Matrix _robotTwoWorld;
        
        // The BoundingCylinder for the second Robot
        private BoundingCylinder _robotTwoCylinder;

        // Indicates if the Robot is touching the Chair
        private bool _touchingChair;

        // Indicates if the Robot is touching the Chair
        private bool _touchingTank;

        // Indicates if the Robot is touching the other Robot
        private bool _touchingOtherRobot;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;

            // Creates a Static Camera pointing to the center
            _camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, 
                Vector3.Forward * 100f + Vector3.Up * 100f, 
                -Vector3.Normalize(Vector3.Forward + Vector3.Up), 
                Vector3.Up);

            // Robot position and matrix initialization
            _robotPosition = Vector3.Zero;
            _robotWorld = Matrix.CreateScale(0.3f);

            // Chair position and matrix initialization
            _chairAngle = MathHelper.PiOver4;
            _chairScale = Matrix.CreateScale(0.5f);
            _chairTranslation = Matrix.CreateTranslation(Vector3.UnitX * 50f);
            _chairWorld = _chairScale * Matrix.CreateRotationY(_chairAngle) * _chairTranslation;

            // Tank position and matrix initialization
            _tankPosition = -Vector3.UnitX * 10f + Vector3.UnitZ * 10f;
            _tankWorld = Matrix.CreateScale(3f) * Matrix.CreateTranslation(_tankPosition);

            // Robot two position and matrix initialization
            var robotTwoPosition = Vector3.UnitX * -50f + Vector3.UnitZ * 10f;
            _robotTwoWorld = Matrix.CreateScale(0.25f) * Matrix.CreateRotationY(MathHelper.PiOver4 + MathHelper.PiOver2) * Matrix.CreateTranslation(robotTwoPosition);
            _robotTwoCylinder = new BoundingCylinder(robotTwoPosition, 5f, 10f);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the models
            _robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");

            _chair = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");

            _tank = Game.Content.Load<Model>(ContentFolder3D + "tank/tank");

            // Enable the default lighting for the models
            EnableDefaultLighting(_robot);
            EnableDefaultLighting(_tank);
            EnableDefaultLighting(_chair);
            
            // Save our RobotEffect
            _robotEffect = ((BasicEffect)_robot.Meshes.FirstOrDefault()?.Effects.FirstOrDefault());
            
            // Create an AABB
            // This gets an AABB with the bounds of the robot model
            _robotBox = BoundingVolumesExtensions.CreateAABBFrom(_robot);
            // Scale it down half-size as the model is scaled down as well
            _robotBox = BoundingVolumesExtensions.Scale(_robotBox, 0.3f);

            // Create an OBB for a model
            // First, get an AABB from the model
            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(_chair);
            // Scale it to match the model's transform
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, 0.5f);
            // Create an Oriented Bounding Box from the AABB
            _chairBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            // Move the center
            _chairBox.Center = Vector3.UnitX * 50f;
            // Then set its orientation!
            _chairBox.Orientation = Matrix.CreateRotationY(_chairAngle);

            // Create a Bounding Sphere for a model
            _tankSphere = BoundingVolumesExtensions.CreateSphereFrom(_tank);
            _tankSphere.Center = _tankPosition;
            _tankSphere.Radius *= 3f;

            // Set depth to default
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.LoadContent();
        }

        private void EnableDefaultLighting(Model model)
        {
            foreach (var mesh in model.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Move the robot depending on key presses

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
                MoveRobot(Vector3.Left);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
                MoveRobot(Vector3.Right);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
                MoveRobot(Vector3.Backward);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
                MoveRobot(Vector3.Forward);

            // Update Gizmos with the View Projection matrices
            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            // Rotate the box
            _chairAngle += 0.01f;
            var rotation = Matrix.CreateRotationY(_chairAngle);
            _chairWorld = _chairScale * rotation * _chairTranslation;
            _chairBox.Orientation = rotation;

            // Create an OBB World-matrix so we can draw a cube representing it
            _chairObbWorld = Matrix.CreateScale(_chairBox.Extents * 2f) *
                 _chairBox.Orientation *
                 _chairTranslation;

            // Update the boolean values depending on the intersection of the Bounding Volumes
            _touchingChair = _chairBox.Intersects(_robotBox);
            _touchingTank = _tankSphere.Intersects(_robotBox);
            _touchingOtherRobot = !_robotTwoCylinder.Intersects(_robotBox).Equals(BoxCylinderIntersection.None);

            base.Update(gameTime);
        }

        private void MoveRobot(Vector3 increment)
        {
            // Update the position of the Robot
            _robotPosition += increment;
            // Update its Bounding Box, moving both min and max positions
            _robotBox = new BoundingBox(_robotBox.Min + increment, _robotBox.Max + increment);
            _robotWorld = Matrix.CreateScale(0.3f) * Matrix.CreateTranslation(_robotPosition);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Set the DiffuseColor to white to draw this Robot
            _robotEffect.DiffuseColor = Color.White.ToVector3();
            _robot.Draw(_robotWorld, _camera.View, _camera.Projection);

            // Set the DiffuseColor to red to draw this Robot
            _robotEffect.DiffuseColor = Color.Red.ToVector3();
            _robot.Draw(_robotTwoWorld, _camera.View, _camera.Projection);

            // Draw the Chair
            _chair.Draw(_chairWorld, _camera.View, _camera.Projection);

            // Draw the Tank
            _tank.Draw(_tankWorld, _camera.View, _camera.Projection);

            // Draw bounding volumes

            Game.Gizmos.DrawCube(BoundingVolumesExtensions.GetCenter(_robotBox), BoundingVolumesExtensions.GetExtents(_robotBox) * 2f, Color.Yellow);

            Game.Gizmos.DrawCube(_chairObbWorld, _touchingChair ? Color.Orange : Color.Green);

            Game.Gizmos.DrawSphere(_tankSphere.Center, _tankSphere.Radius * Vector3.One, _touchingTank ? Color.Orange : Color.Purple);

            Game.Gizmos.DrawCylinder(_robotTwoCylinder.Transform, _touchingOtherRobot ? Color.Orange : Color.White);

            base.Draw(gameTime);
        }
    }
}
