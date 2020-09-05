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

namespace TGC.MonoGame.Samples.Samples.Physics
{
    public class BepuPhysicsSample : TGCSample
    {
        /// <inheritdoc />
        public BepuPhysicsSample(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Physics;
            Name = "BepuPhysics 2 - Wall demo";
            Description = "A wall of boxes, because you can't have a physics engine without wall of boxes.";
        }

        private List<Color> SphereColors { get; set; } = new List<Color> {Color.DarkCyan, Color.DarkGray, Color.DarkMagenta, Color.DarkSeaGreen, Color.DarkViolet, Color.MonoGameOrange};

    private List<SpherePrimitive> Boxes { get; set; }

        /// <summary>
        ///     We'll randomize the size of bullets.
        /// </summary>
        private Random Random { get; set; }

        private List<SpherePrimitive> Spheres { get; set; }

        private List<BodyHandle> BodiesSphereHandles { get; set; }
        private List<BodyHandle> BodiesBoxesHandles { get; set; }

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

        private Model Model { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Random = new Random(5);
            //The buffer pool is a source of raw memory blobs for the engine to use.
            BufferPool = new BufferPool();

            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(20, 10, 250), 55, 0.4f);

            //The PositionFirstTimestepper is the simplest timestepping mode, but since it integrates velocity into position at the start of the frame, directly modified velocities outside of the timestep
            //will be integrated before collision detection or the solver has a chance to intervene. That's fine in this demo. Other built-in options include the PositionLastTimestepper and the SubsteppingTimestepper.
            //Note that the timestepper also has callbacks that you can use for executing logic between processing stages, like BeforeCollisionDetection.
            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new NumericVector3(0, -100, 0)), new PositionFirstTimestepper());

            Boxes = new List<SpherePrimitive>();
            Spheres = new List<SpherePrimitive>();

            CanShoot = true;

            BodiesSphereHandles = new List<BodyHandle>();
            BodiesBoxesHandles = new List<BodyHandle>();

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
                Boxes.Add(new SpherePrimitive(GraphicsDevice, radius, 16, Color.RosyBrown));

                var bodyHandle = Simulation.Bodies.Add(bodyDescription);

                BodiesBoxesHandles.Add(bodyHandle);
            }

            // Creates a floor
            Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -0.5f, 0),
                new CollidableDescription(Simulation.Shapes.Add(new Box(2500, 1, 2500)), 0.1f)));

            ThreadDispatcher = new SimpleThreadDispatcher(Environment.ProcessorCount);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // This are meshes/model/primitives collections to render
            Model = Game.Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic");
            ((BasicEffect) Model.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();

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

                Spheres.Add(new SpherePrimitive(GraphicsDevice, radius * 2, 16, SphereColors[(int) Math.Round(Random.NextDouble() * 5)]));

                var bodyHandle = Simulation.Bodies.Add(bodyDescription);

                BodiesSphereHandles.Add(bodyHandle);
            }

            if (Game.CurrentKeyboardState.IsKeyUp(Keys.Z)) CanShoot = true;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var t in BodiesBoxesHandles)
                Boxes.ForEach(box =>
                {
                    var position = Simulation.Bodies.GetBodyReference(t).Pose.Position;
                    var quaternion = Simulation.Bodies.GetBodyReference(t).Pose.Orientation;
                    var transform = Matrix.CreateScale(0.1f) *
                                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                                        quaternion.W)) *
                                    Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                    Model.Draw(transform, Camera.View, Camera.Projection);
                });

            foreach (var t in BodiesSphereHandles)
                Spheres.ForEach(sphere =>
                {
                    var position = Simulation.Bodies.GetBodyReference(t).Pose.Position;
                    var quaternion = Simulation.Bodies.GetBodyReference(t).Pose.Orientation;
                    var transform =
                        Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                            quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                    sphere.Draw(transform, Camera.View, Camera.Projection);
                });

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            //If you intend to reuse the BufferPool, disposing the simulation is a good idea- it returns all the buffers to the pool for reuse.
            //Here, we dispose it, but it's not really required; we immediately thereafter clear the BufferPool of all held memory.
            //Note that failing to dispose buffer pools can result in memory leaks.
            Simulation.Dispose();
            ThreadDispatcher.Dispose();
            BufferPool.Clear();

            base.UnloadContent();
        }
    }
}