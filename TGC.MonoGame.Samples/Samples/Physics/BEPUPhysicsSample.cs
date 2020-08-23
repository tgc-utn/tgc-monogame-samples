using System;
using System.Collections.Generic;
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

namespace TGC.MonoGame.Samples.Samples.Physics
{
    public class BEPUPhysicsSample : TGCSample
    {
        public List<BoxPrimitive> Boxes;

        //We'll randomize the size of bullets.
        private Random Random;

        public List<SpherePrimitive> Spheres;

        /// <inheritdoc />
        public BEPUPhysicsSample(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Physics;
            Name = "bepu physics 2";
            Description = "bepu physics 2";
        }

        /// <summary>
        ///     Gets the buffer pool used by the demo's simulation.
        ///     Note that the buffer pool used by the simulation is not considered to be *owned* by the simulation.
        ///     The simulation merely uses the pool.
        ///     Disposing the simulation will not dispose or clear the buffer pool.
        /// </summary>
        public BufferPool BufferPool { get; private set; }

        private Camera Camera { get; set; }

        /// <summary>
        ///     Gets the simulation created by the sample's Initialize call.
        /// </summary>
        public Simulation Simulation { get; protected set; }

        /// <summary>
        ///     Gets the thread dispatcher available for use by the simulation.
        /// </summary>
        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Random = new Random(5);
            //The buffer pool is a source of raw memory blobs for the engine to use.
            BufferPool = new BufferPool();

            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(20, 8, 125), 6);

            //The PositionFirstTimestepper is the simplest timestepping mode, but since it integrates velocity into position at the start of the frame, directly modified velocities outside of the timestep
            //will be integrated before collision detection or the solver has a chance to intervene. That's fine in this demo. Other built-in options include the PositionLastTimestepper and the SubsteppingTimestepper.
            //Note that the timestepper also has callbacks that you can use for executing logic between processing stages, like BeforeCollisionDetection.
            Simulation = Simulation.Create(BufferPool, new SimpleSelfContainedDemo.NarrowPhaseCallbacks(),
                new SimpleSelfContainedDemo.PoseIntegratorCallbacks(new System.Numerics.Vector3(0, -10, 0)),
                new PositionFirstTimestepper());

            Boxes = new List<BoxPrimitive>();
            Spheres = new List<SpherePrimitive>();

            var boxShape = new Box(1, 1, 1);
            boxShape.ComputeInertia(1, out var boxInertia);
            var boxIndex = Simulation.Shapes.Add(boxShape);
            const int pyramidCount = 40;
            for (var pyramidIndex = 0; pyramidIndex < pyramidCount; ++pyramidIndex)
            {
                const int rowCount = 20;
                for (var rowIndex = 0; rowIndex < rowCount; ++rowIndex)
                {
                    var columnCount = rowCount - rowIndex;
                    for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                    {
                        var position = new System.Numerics.Vector3((-columnCount * 0.5f + columnIndex) * boxShape.Width,
                            (rowIndex + 0.5f) * boxShape.Height,
                            (pyramidIndex - pyramidCount * 0.5f) * (boxShape.Length + 4));
                        Simulation.Bodies.Add(BodyDescription.CreateDynamic(position,
                            boxInertia,
                            new CollidableDescription(boxIndex, 0.1f),
                            new BodyActivityDescription(0.01f)));
                        Boxes.Add(new BoxPrimitive(GraphicsDevice, Vector3.One,
                            new Vector3(position.X, position.Y, position.Z), Color.Black, Color.Red, Color.Yellow,
                            Color.Green, Color.Blue, Color.Magenta, Color.White, Color.Cyan));
                    }
                }
            }

            Simulation.Statics.Add(new StaticDescription(new System.Numerics.Vector3(0, -0.5f, 0),
                new CollidableDescription(Simulation.Shapes.Add(new Box(2500, 1, 2500)), 0.1f)));

            ThreadDispatcher = new SimpleThreadDispatcher(Environment.ProcessorCount);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Z))
            {
                //Create the shape that we'll launch at the pyramids when the user presses a button.
                var raduis = 0.5f + 5 * (float) Random.NextDouble();
                var bulletShape = new Sphere(raduis);
                //Note that the use of radius^3 for mass can produce some pretty serious mass ratios. 
                //Observe what happens when a large ball sits on top of a few boxes with a fraction of the mass-
                //the collision appears much squishier and less stable. For most games, if you want to maintain rigidity, you'll want to use some combination of:
                //1) Limit the ratio of heavy object masses to light object masses when those heavy objects depend on the light objects.
                //2) Use a shorter timestep duration and update more frequently.
                //3) Use a greater number of solver iterations.
                //#2 and #3 can become very expensive. In pathological cases, it can end up slower than using a quality-focused solver for the same simulation.
                //Unfortunately, at the moment, bepuphysics v2 does not contain any alternative solvers, so if you can't afford to brute force the the problem away,
                //the best solution is to cheat as much as possible to avoid the corner cases.
                var position = new System.Numerics.Vector3(0, 8, 130);
                var bodyDescription = BodyDescription.CreateConvexDynamic(position,
                    new BodyVelocity(new System.Numerics.Vector3(0, 0, 150)),
                    bulletShape.Radius * bulletShape.Radius * bulletShape.Radius, Simulation.Shapes, bulletShape);
                Spheres.Add(new SpherePrimitive(GraphicsDevice, raduis, 16));
                Simulation.Bodies.Add(bodyDescription);
            }

            //In the demos, we use one time step per frame. We don't bother modifying the physics time step duration for different monitors so different refresh rates
            //change the rate of simulation. This doesn't actually change the result of the simulation, though, and the simplicity is a good fit for the demos.
            //In the context of a 'real' application, you could instead use a time accumulator to take time steps of fixed length as needed, or
            //fully decouple simulation and rendering rates across different threads.
            //(In either case, you'd also want to interpolate or extrapolate simulation results during rendering for smoothness.)
            //Note that taking steps of variable length can reduce stability. Gradual or one-off changes can work reasonably well.
            Simulation.Timestep(1 / 60f, ThreadDispatcher);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var i = 0;

            Boxes.ForEach(box =>
            {
                var position = Simulation.Bodies.Sets[0].Poses[i].Position;
                DrawGeometry(box, new Vector3(position.X, position.Y, position.Z));
                i++;
            });

            Spheres.ForEach(sphere =>
            {
                var position = Simulation.Bodies.Sets[0].Poses[i].Position;
                DrawGeometry(sphere, new Vector3(position.X, position.Y, position.Z));
                i++;
            });

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Unload any content here.
        /// </summary>
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

        /// <summary>
        ///     Draw the geometry applying a rotation and translation.
        /// </summary>
        /// <param name="geometry">The geometry to draw.</param>
        /// <param name="position">The position of the geometry.</param>
        /// <param name="yaw">Vertical axis (yaw).</param>
        /// <param name="pitch">Transverse axis (pitch).</param>
        /// <param name="roll">Longitudinal axis (roll).</param>
        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position)
        {
            var effect = geometry.Effect;

            effect.World = Matrix.CreateTranslation(position);
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;

            geometry.Draw(effect);
        }
    }
}