using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Optimizations
{
    /// <summary>
    /// Shows how to perform basic frustum culling for visibility testing.
    /// Author: Ronan Vinitzca.
    /// </summary>
    public class FrustumCulling : TGCSample
    {

        /// <summary>
        /// The index of the box Bounding Volume model to update
        /// </summary>
        private const int BoxIndexToUpdate = 1;

        /// <summary>
        /// The index of the sphere Bounding Volume model to update
        /// </summary>
        private const int SphereIndexToUpdate = 2;

        /// <summary>
        /// A Camera to check against bounding volumes
        /// </summary>
        private Camera TestCamera { get; set; }

        /// <summary>
        /// A Camera to view the optimization technique
        /// </summary>
        private Camera Camera { get; set; }

        /// <summary>
        /// A Bounding Frustum to check visibility
        /// </summary>
        private BoundingFrustum BoundingFrustum { get; set; }

        /// <summary>
        /// A collection of models to draw with World matrices and AABBs for visibility testing
        /// </summary>
        private DrawInstanceBox[] BoxModels { get; set; }

        /// <summary>
        /// A collection of models to draw with World matrices and spheres for visibility testing
        /// </summary>
        private DrawInstanceSphere[] SphereModels { get; set; }

        /// <summary>
        /// A font to draw text
        /// </summary>
        private SpriteFont Font { get; set; }


        /// <inheritdoc />
        public FrustumCulling(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Optimizations;
            Name = "Frustum Culling";
            Description = "Shows how to perform basic frustum culling for visibility testing";
        }


        /// <inheritdoc />
        public override void Initialize()
        {
            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;

            // Create a camera not to render objects but to test them against its frustum
            TestCamera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 50, 1000), size);
            
            // Create a camera to render the scene and visualize the frustum culling technique
            var cameraPosition = Vector3.One * 1000f;
            Camera = new StaticCamera(
                GraphicsDevice.Viewport.AspectRatio,
                cameraPosition, Vector3.Normalize(Vector3.Backward * 500f - cameraPosition), Vector3.Up);
            Camera.BuildProjection(GraphicsDevice.Viewport.AspectRatio, 60.0f, 30000f, MathF.PI / 2.5f);
          
            // Create a bounding frustum to check bounding volumes against it
            BoundingFrustum = new BoundingFrustum(TestCamera.View * TestCamera.Projection);

            base.Initialize();
        }


        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Load a font to render the draw call count
            Font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

            // Load the robot mesh, and generate an AABB and sphere from it
            var robot = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            var robotAABB = BoundingVolumesExtensions.CreateAABBFrom(robot);
            var robotSphere = BoundingVolumesExtensions.CreateSphereFrom(robot);

            // Load the chair mesh, and generate an AABB and sphere from it
            var chair = Game.Content.Load<Model>(ContentFolder3D + "chair/chair");
            var chairAABB = BoundingVolumesExtensions.CreateAABBFrom(chair);
            var chairSphere = BoundingVolumesExtensions.CreateSphereFrom(chair);

            // Load the sphere mesh and floor, from runtime generated primitives
            var logo = Game.Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
            var logoAABB = BoundingVolumesExtensions.CreateAABBFrom(logo);


            // Create draw instances for models that use AABBs for bounding volumes
            BoxModels = new DrawInstanceBox[]
            {
                new DrawInstanceBox
                {
                    Model = logo, 
                    Box = logoAABB, 
                    World = Matrix.CreateTranslation(Vector3.Right * 500f)
                },
                new DrawInstanceBox
                {
                    Model = robot,
                    Box = robotAABB,
                    World = Matrix.CreateTranslation(Vector3.Left * 300f)
                },
                new DrawInstanceBox
                {
                    Model = robot,
                    Box = robotAABB,
                    World = Matrix.CreateTranslation(Vector3.Up * 700f)
                },
                new DrawInstanceBox
                {
                    Model = chair,
                    Box = chairAABB,
                    World = Matrix.CreateTranslation(Vector3.Backward * 600f)
                },
            };

            // Update the AABBs
            for (var index = 0; index < BoxModels.Length; index++)
            {
                BoxModels[index].UpdateAABB();
            }

            // Create draw instances for models that use spheres for bounding volumes
            SphereModels = new DrawInstanceSphere[]
            {
                new DrawInstanceSphere
                {
                    Model = chair,
                    Sphere = chairSphere,
                    World = Matrix.CreateTranslation(Vector3.Right * 100f)
                },
                new DrawInstanceSphere
                {
                    Model = robot,
                    Sphere = robotSphere,
                    World = Matrix.CreateTranslation(new Vector3(-300f, 40f, 150f))
                },
                new DrawInstanceSphere
                {
                    Model = chair,
                    Sphere = chairSphere,
                    World = Matrix.CreateTranslation(new Vector3(-300f, 120f, -700f))
                },
            };

            // Update the spheres
            for (var index = 0; index < SphereModels.Length; index++)
            {
                SphereModels[index].UpdateSphere();
            }

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Update the state of the camera to test collisions against
            TestCamera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            // Update the view projection matrix of the bounding frustum
            BoundingFrustum.Matrix = TestCamera.View * TestCamera.Projection;

            // Move a model from each type
            var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            BoxModels[BoxIndexToUpdate].World = Matrix.CreateTranslation(Vector3.Right * MathF.Sin(time) * 400f);
            BoxModels[BoxIndexToUpdate].UpdateAABB();
            SphereModels[SphereIndexToUpdate].World = Matrix.CreateTranslation(Vector3.Up * MathF.Cos(time) * 400f);
            SphereModels[SphereIndexToUpdate].UpdateSphere();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            // Set the background color to cornflower blue
            Game.Background = Color.CornflowerBlue;

            // Record the draw call count for this frame
            var drawCallCount = 0;

            // Set the state of the Graphics Device in case the sprite batch alters it
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.Opaque;

            DrawInstanceBox boxModel;
            bool drawn;

            // For each model that uses an AABB for visibility testing
            // Check if it collides with the frustum, then draw
            for (var index = 0; index < BoxModels.Length; index++)
            {
                boxModel = BoxModels[index];
                drawn = false;
                if (BoundingFrustum.Intersects(boxModel.BoxWorldSpace))
                {
                    boxModel.Model.Draw(boxModel.World, Camera.View, Camera.Projection);                    
                    drawCallCount++;
                    drawn = true;
                }

                // Draw gizmos for each AABB
                var extents = BoundingVolumesExtensions.GetExtents(boxModel.BoxWorldSpace) * 2f;
                var box = BoundingVolumesExtensions.GetCenter(boxModel.BoxWorldSpace);
                Game.Gizmos.DrawCube(box, extents, drawn ? Color.Red : Color.Green);
            }

            
            DrawInstanceSphere sphereModel;

            // For each model that uses a sphere for visibility testing
            // Check if it collides with the frustum, then draw
            for (var index = 0; index < SphereModels.Length; index++)
            {
                sphereModel = SphereModels[index];
                drawn = false;
                if (BoundingFrustum.Intersects(sphereModel.Sphere))
                {
                    sphereModel.Model.Draw(sphereModel.World, Camera.View, Camera.Projection);
                    drawCallCount++;
                    drawn = true;
                }

                // Draw gizmos for each sphere
                Game.Gizmos.DrawSphere(sphereModel.Sphere.Center, sphereModel.Sphere.Radius * Vector3.One,
                    drawn ? Color.Red : Color.Green);
            }

            // Draw a gizmo for the frustum
            Game.Gizmos.DrawFrustum(TestCamera.View * TestCamera.Projection, Color.Yellow);

            // Show the draw call count
            DisplayDrawCallCount(drawCallCount);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Renders the draw call count into the screen.
        /// </summary>
        /// <param name="drawCallCount">The amount of draw calls for this frame</param>
        private void DisplayDrawCallCount(int drawCallCount)
        {
            Game.SpriteBatch.Begin();

            var textToShow = "Draw Calls: " + drawCallCount.ToString();
            var textPosition = new Vector2(GraphicsDevice.Viewport.Width / 2f, 20) -
                                    Font.MeasureString(textToShow) / 2;

            Game.SpriteBatch.DrawString(Font, textToShow, textPosition, Color.Black);
            
            Game.SpriteBatch.End();
        }

    }

    /// <summary>
    /// Represents a model to draw with a world matrix to place it in the world, and an AABB for visibility testing.
    /// </summary>
    struct DrawInstanceBox
    {
        /// <summary>
        /// A model to draw
        /// </summary>
        public Model Model;
        
        /// <summary>
        /// A box to get its max and min positions in local coordinates
        /// </summary>
        public BoundingBox Box;
        
        /// <summary>
        /// The actual box to perform intersection tests
        /// </summary>
        public BoundingBox BoxWorldSpace;

        /// <summary>
        /// A world matrix to transform the model into world space
        /// </summary>
        public Matrix World;

        /// <summary>
        /// Updates the AABB by using the world matrix and the original box min and max positions (in local coordinates).
        /// </summary>
        public void UpdateAABB()
        {
            var translation = World.Translation;
            BoxWorldSpace.Min = Box.Min + translation;
            BoxWorldSpace.Max = Box.Max + translation;
        }

    }

    /// <summary>
    /// Represents a model to draw with a world matrix to place it in the world, and a sphere for visibility testing.
    /// </summary>
    struct DrawInstanceSphere
    {
        /// <summary>
        /// A model to draw
        /// </summary>
        public Model Model;

        /// <summary>
        /// A sphere to perform intersection tests
        /// </summary>
        public BoundingSphere Sphere;

        /// <summary>
        /// A world matrix to transform the model into world space
        /// </summary>
        public Matrix World;

        /// <summary>
        /// Updates the sphere by using the world matrix
        /// </summary>
        public void UpdateSphere()
        {
            Sphere.Center = World.Translation;
        }
    }
}
