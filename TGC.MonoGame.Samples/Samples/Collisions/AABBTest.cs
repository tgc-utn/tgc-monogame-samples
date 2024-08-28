using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Collisions
{
    /// <summary>
    ///     Shows how to test collision for two AABBs
    /// </summary>
    public class AABBTest : TGCSample
    {
        /// <inheritdoc />
        public AABBTest(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Collisions;
            Name = "AABB Test";
            Description = "Shows how to test collision for two AABBs.";
        }

        // Camera to draw the scene
        private Camera _camera;

        // The Model of the Robot to draw
        private Model _robot;

        // The World Matrix for the first Robot
        private Matrix _robotOneWorld;
        
        // The World Matrix for the second Robot
        private Matrix _robotTwoWorld;

        // The BoundingBox for the first Robot
        private BoundingBox _robotOneBox;
        
        // The BoundingBox for the second Robot
        private BoundingBox _robotTwoBox;

        // The first Robot's position
        private Vector3 _robotOnePosition;
        
        // The second Robot's position
        private Vector3 _robotTwoPosition;

        // Indicates if the AABBs are touching
        private bool _areAabBsTouching;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            
            // Creates a Static Camera looking at the origin
            _camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 250f, -Vector3.Normalize(Vector3.One), Vector3.Up);

            // Set the Robot positions
            _robotTwoPosition = new Vector3(-60f, 0f, 0f);
            _robotOnePosition = new Vector3(60f, 0f, 0f);

            // Set the World Matrices for each Robot
            _robotOneWorld = Matrix.CreateTranslation(_robotOnePosition);           
            _robotTwoWorld = Matrix.CreateTranslation(_robotTwoPosition);

            // Initialize AABB touching value as false
            _areAabBsTouching = false;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the Robot Model and enable default lighting
            _robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            ((BasicEffect)_robot.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();

            // Create AABBs
            // This gets an AABB with the bounds of the robot model
            _robotOneBox = BoundingVolumesExtensions.CreateAABBFrom(_robot);
            
            // This moves the min and max points to the world position of each robot (one and two)
            _robotTwoBox = new BoundingBox(_robotOneBox.Min + _robotTwoPosition, _robotOneBox.Max + _robotTwoPosition);
            _robotOneBox = new BoundingBox(_robotOneBox.Min + _robotOnePosition, _robotOneBox.Max + _robotOnePosition);

            // Set depth to default
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Move the robot depending on key presses

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
                MoveRobot(Vector3.Right);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
                MoveRobot(Vector3.Left);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
                MoveRobot(Vector3.Backward);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
                MoveRobot(Vector3.Forward);

            // Update Gizmos with the View Projection matrices
            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            // Update the boolean value depending on the intersection of the two AABBs
            _areAabBsTouching = _robotOneBox.Intersects(_robotTwoBox);

            base.Update(gameTime);
        }

        private void MoveRobot(Vector3 increment)
        {
            // Move the RobotTwoPosition, then update (move) the Bounding Box,
            // and update its matrix
            _robotTwoPosition += increment;
            _robotTwoBox = new BoundingBox(_robotTwoBox.Min + increment, _robotTwoBox.Max + increment);
            _robotTwoWorld = Matrix.CreateTranslation(_robotTwoPosition);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Draw the two robots with their corresponding World matrices
            _robot.Draw(_robotOneWorld, _camera.View, _camera.Projection);
            _robot.Draw(_robotTwoWorld, _camera.View, _camera.Projection);

            // Draw bounding boxes
            // Center is half the average between min and max
            // Size is the difference between max and min
            // Also draw red if they touch

            var colorOne = _areAabBsTouching ? Color.Red : Color.Yellow;
            var colorTwo = _areAabBsTouching ? Color.Red : Color.Green;

            Game.Gizmos.DrawCube((_robotOneBox.Max + _robotOneBox.Min) / 2f, _robotOneBox.Max - _robotOneBox.Min, colorOne);
            Game.Gizmos.DrawCube((_robotTwoBox.Max + _robotTwoBox.Min) / 2f, _robotTwoBox.Max - _robotTwoBox.Min, colorTwo);

            base.Draw(gameTime);
        }
    }
}
