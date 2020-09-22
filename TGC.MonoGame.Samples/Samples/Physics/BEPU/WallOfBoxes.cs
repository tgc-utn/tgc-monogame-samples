using System;
using System.Collections.Generic;
using System.Linq;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Physics;
using TGC.MonoGame.Samples.Viewer;
using NumericVector3 = System.Numerics.Vector3;

namespace TGC.MonoGame.Samples.Samples.Physics.BEPU
{
    public class WallOfBoxes : TGCSample
    {
        /// <inheritdoc />
        public WallOfBoxes(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Physics;
            Name = "BepuPhysics 2 - Wall demo";
            Description = "A wall of boxes, because you can't have a physics engine without wall of boxes.";
        }

        private CubePrimitive Box { get; set; }

        /// <summary>
        ///     We'll randomize the size of bullets.
        /// </summary>
        private Random Random { get; set; }

        private SpherePrimitive Sphere { get; set; }
        private List<float> Radii { get; set; }

        private List<BodyHandle> SphereHandles { get; set; }
        private List<BodyHandle> BoxHandles { get; set; }

        private List<Matrix> BoxesWorld { get; set; }

        private List<Matrix> SpheresWorld { get; set; }

        private bool CanShoot { get; set; }

        /// <summary>
        ///     Gets the buffer pool used by the demo's simulation.
        ///     Note that the buffer pool used by the simulation is not considered to be *owned* by the simulation.
        ///     The simulation merely uses the pool.
        ///     Disposing the simulation will not dispose or clear the buffer pool.
        /// </summary>
        private BufferPool BufferPool { get; set; }

        private Camera Camera { get; set; }

        /// <summary>
        ///     Gets the simulation created by the sample's Initialize call.
        /// </summary>
        private Simulation Simulation { get; set; }

        /// <summary>
        ///     Gets the thread dispatcher available for use by the simulation.
        /// </summary>
        private SimpleThreadDispatcher ThreadDispatcher { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            CanShoot = true;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Random = new Random(5);

            //The buffer pool is a source of raw memory blobs for the engine to use.
            BufferPool = new BufferPool();

            Radii = new List<float>();
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(20, 10, 250), 55, 0.4f);


            Box = new CubePrimitive(GraphicsDevice, 10f, Color.Red);
            Sphere = new SpherePrimitive(GraphicsDevice, 1f, 16);

            SphereHandles = new List<BodyHandle>();
            BoxHandles = new List<BodyHandle>();


            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);



            // This are meshes/model/primitives collections to render
            //The PositionFirstTimestepper is the simplest timestepping mode, but since it integrates velocity into position at the start of the frame, directly modified velocities outside of the timestep
            //will be integrated before collision detection or the solver has a chance to intervene. That's fine in this demo. Other built-in options include the PositionLastTimestepper and the SubsteppingTimestepper.
            //Note that the timestepper also has callbacks that you can use for executing logic between processing stages, like BeforeCollisionDetection.
            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new NumericVector3(0, -100, 0)), new PositionFirstTimestepper());

            // Creates a floor
            Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -0.5f, 0),
                new CollidableDescription(Simulation.Shapes.Add(new Box(2500, 1, 2500)), 0.1f)));

            BoxesWorld = new List<Matrix>();
            SpheresWorld = new List<Matrix>();

            // Single Box
            var radius = 10;
            for (var j = 0; j < 5; j++)
                for (var i = 0; i < 20; i++)
                {
                    var boxShape = new Box(radius, radius, radius);
                    boxShape.ComputeInertia(0.4f, out var boxInertia);
                    var boxIndex = Simulation.Shapes.Add(boxShape);
                    var position = new NumericVector3(-30 + i * 10 + 1, j * 10 + 1, -40);

                    var bodyDescription = BodyDescription.CreateDynamic(position, boxInertia,
                        new CollidableDescription(boxIndex, 0.1f), new BodyActivityDescription(0.01f));

                    var bodyHandle = Simulation.Bodies.Add(bodyDescription);

                    BoxHandles.Add(bodyHandle);
                }


            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            //In the demos, we use one time step per frame. We don't bother modifying the physics time step duration for different monitors so different refresh rates
            //change the rate of simulation. This doesn't actually change the result of the simulation, though, and the simplicity is a good fit for the demos.
            //In the context of a 'real' application, you could instead use a time accumulator to take time steps of fixed length as needed, or
            //fully decouple simulation and rendering rates across different threads.
            //(In either case, you'd also want to interpolate or extrapolate simulation results during rendering for smoothness.)
            //Note that taking steps of variable length can reduce stability. Gradual or one-off changes can work reasonably well.
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            Camera.Update(gameTime);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Z) && CanShoot)
            {
                CanShoot = false;
                //Create the shape that we'll launch at the pyramids when the user presses a button.
                var radius = 0.5f + 5 * (float) Random.NextDouble();
                var bulletShape = new Sphere(radius);

                //Note that the use of radius^3 for mass can produce some pretty serious mass ratios. 
                //Observe what happens when a large ball sits on top of a few boxes with a fraction of the mass-
                //the collision appears much squishier and less stable. For most games, if you want to maintain rigidity, you'll want to use some combination of:
                //1) Limit the ratio of heavy object masses to light object masses when those heavy objects depend on the light objects.
                //2) Use a shorter timestep duration and update more frequently.
                //3) Use a greater number of solver iterations.
                //#2 and #3 can become very expensive. In pathological cases, it can end up slower than using a quality-focused solver for the same simulation.
                //Unfortunately, at the moment, bepuphysics v2 does not contain any alternative solvers, so if you can't afford to brute force the the problem away,
                //the best solution is to cheat as much as possible to avoid the corner cases.
                var position = new NumericVector3(-40 + 210 * (float) Random.NextDouble(), 130, 130);
                var bodyDescription = BodyDescription.CreateConvexDynamic(position,
                    new BodyVelocity(new NumericVector3((float) Random.NextDouble(), 0, -110)),
                    bulletShape.Radius * bulletShape.Radius * bulletShape.Radius, Simulation.Shapes, bulletShape);

                var bodyHandle = Simulation.Bodies.Add(bodyDescription);

                Radii.Add(radius);
                SphereHandles.Add(bodyHandle);
            }

            if (Game.CurrentKeyboardState.IsKeyUp(Keys.Z)) CanShoot = true;


            BoxesWorld.Clear();
            int boxHandleCount = BoxHandles.Count;
            for (int index = 0; index < boxHandleCount; index++)
            {
                var pose = Simulation.Bodies.GetBodyReference(BoxHandles[index]).Pose;
                NumericVector3 position = pose.Position;
                var quaternion = pose.Orientation;
                Matrix world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) *
                    Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                BoxesWorld.Add(world);
            }


            SpheresWorld.Clear();
            int sphereHandleCount = SphereHandles.Count;
            for (int index = 0; index < sphereHandleCount; index++)
            {
                var pose = Simulation.Bodies.GetBodyReference(SphereHandles[index]).Pose;
                NumericVector3 position = pose.Position;
                var quaternion = pose.Orientation;
                Matrix world =
                    Matrix.CreateScale(Radii[index]) * 
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) *
                    Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                SpheresWorld.Add(world);
            }



            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            BoxesWorld.ForEach(boxWorld => Box.Draw(boxWorld, Camera.View, Camera.Projection));
            SpheresWorld.ForEach(sphereWorld => Sphere.Draw(sphereWorld, Camera.View, Camera.Projection));
            
            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            //If you intend to reuse the BufferPool, disposing the simulation is a good idea- it returns all the buffers to the pool for reuse.
            //Here, we dispose it, but it's not really required; we immediately thereafter clear the BufferPool of all held memory.
            //Note that failing to dispose buffer pools can result in memory leaks.
            Simulation.Dispose();

            BufferPool.Clear();

            ThreadDispatcher.Dispose();

            base.UnloadContent();
        }
    }
}