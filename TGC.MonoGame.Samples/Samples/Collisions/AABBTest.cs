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
        private Camera Camera { get; set; }

        // The Model of the Robot to draw
        private Model Robot { get; set; }

        // The World Matrix for the first Robot
        private Matrix RobotOneWorld { get; set; }
        
        // The World Matrix for the second Robot
        private Matrix RobotTwoWorld { get; set; }

        // The BoundingBox for the first Robot
        private BoundingBox RobotOneBox { get; set; }
        
        // The BoundingBox for the second Robot
        private BoundingBox RobotTwoBox { get; set; }

        // The first Robot's position
        private Vector3 RobotOnePosition { get; set; }
        
        // The second Robot's position
        private Vector3 RobotTwoPosition { get; set; }

        // Indicates if the AABBs are touching
        private bool AreAABBsTouching { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            
            // Creates a Static Camera looking at the origin
            Camera = new StaticCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 250f, -Vector3.Normalize(Vector3.One), Vector3.Up);

            // Set the Robot positions
            RobotTwoPosition = new Vector3(-60f, 0f, 0f);
            RobotOnePosition = new Vector3(60f, 0f, 0f);

            // Set the World Matrices for each Robot
            RobotOneWorld = Matrix.CreateTranslation(RobotOnePosition);           
            RobotTwoWorld = Matrix.CreateTranslation(RobotTwoPosition);

            // Initialize AABB touching value as false
            AreAABBsTouching = false;

            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load the Robot Model and enable default lighting
            Robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            ((BasicEffect)Robot.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();


            // Create AABBs
            // This gets an AABB with the bounds of the robot model
            RobotOneBox = BoundingVolumesExtensions.CreateAABBFrom(Robot);
            
            // This moves the min and max points to the world position of each robot (one and two)
            RobotTwoBox = new BoundingBox(RobotOneBox.Min + RobotTwoPosition, RobotOneBox.Max + RobotTwoPosition);
            RobotOneBox = new BoundingBox(RobotOneBox.Min + RobotOnePosition, RobotOneBox.Max + RobotOnePosition);

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
            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            // Update the boolean value depending on the intersection of the two AABBs
            AreAABBsTouching = RobotOneBox.Intersects(RobotTwoBox);

            base.Update(gameTime);
        }

        private void MoveRobot(Vector3 increment)
        {
            // Move the RobotTwoPosition, then update (move) the Bounding Box,
            // and update its matrix
            RobotTwoPosition += increment;
            RobotTwoBox = new BoundingBox(RobotTwoBox.Min + increment, RobotTwoBox.Max + increment);
            RobotTwoWorld = Matrix.CreateTranslation(RobotTwoPosition);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Draw the two robots with their corresponding World matrices
            Robot.Draw(RobotOneWorld, Camera.View, Camera.Projection);
            Robot.Draw(RobotTwoWorld, Camera.View, Camera.Projection);

            // Draw bounding boxes
            // Center is half the average between min and max
            // Size is the difference between max and min
            // Also draw red if they touch

            var colorOne = AreAABBsTouching ? Color.Red : Color.Yellow;
            var colorTwo = AreAABBsTouching ? Color.Red : Color.Green;

            Game.Gizmos.DrawCube((RobotOneBox.Max + RobotOneBox.Min) / 2f, RobotOneBox.Max - RobotOneBox.Min, colorOne);
            Game.Gizmos.DrawCube((RobotTwoBox.Max + RobotTwoBox.Min) / 2f, RobotTwoBox.Max - RobotTwoBox.Min, colorTwo);

            base.Draw(gameTime);
        }


    }
}
