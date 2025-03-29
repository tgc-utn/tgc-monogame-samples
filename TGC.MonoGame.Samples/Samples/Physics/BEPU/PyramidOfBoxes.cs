using System;
using System.Collections.Generic;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Physics.Bepu;
using TGC.MonoGame.Samples.Viewer;
using NumericVector3 = System.Numerics.Vector3;

namespace TGC.MonoGame.Samples.Samples.Physics.Bepu
{
    /// <summary>
    ///     Pyramid Of Boxes:
    ///     Ported sample from bepu physics 2
    ///     https://github.com/bepu/bepuphysics2/blob/master/Demos/Demos/PyramidDemo.cs
    ///     Author: Ronán Ariel Vinitzca.
    /// </summary>
    public class PyramidOfBoxes : TGCSample
    {
        private List<Matrix> _activeBoxesWorld;

        private List<BodyHandle> _boxHandles;

        private Camera _camera;

        private bool _canShoot = true;

        private CubePrimitive _cubePrimitive;

        private List<Matrix> _inactiveBoxesWorld;

        private List<float> _radii;

        private Random _random;

        private List<BodyHandle> _sphereHandles;

        private SpriteFont _spriteFont;

        private SpherePrimitive _spherePrimitive;

        private List<Matrix> _spheresWorld;

        public PyramidOfBoxes(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Physics;
            Name = "BepuPhysics - Pyramid of boxes";
            Description = "A pyramid of boxes, because you can't have a physics engine without pyramids of boxes.";
        }

        /// <summary>
        ///     Gets the simulation created by the demo's Initialize call.
        /// </summary>
        public Simulation Simulation { get; protected set; }

        //Note that the buffer pool used by the simulation is not considered to be *owned* by the simulation. The simulation merely uses the pool.
        //Disposing the simulation will not dispose or clear the buffer pool.
        /// <summary>
        ///     Gets the buffer pool used by the demo's simulation.
        /// </summary>
        public BufferPool BufferPool { get; private set; }

        /// <summary>
        ///     Gets the thread dispatcher available for use by the simulation.
        /// </summary>
        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }

        public override void Initialize()
        {
            BufferPool = new BufferPool();
            //Generally, shoving as many threads as possible into the simulation won't produce the best results on systems with multiple logical cores per physical core.
            //Environment.ProcessorCount reports logical core count only, so we'll use a simple heuristic here- it'll leave one or two logical cores idle.
            //For the common Intel quad core with hyperthreading, this'll use six logical cores and leave two logical cores free to be used for other stuff.
            //This is by no means perfect. To maximize performance, you'll need to profile your simulation and target hardware.
            //Note that issues can be magnified on older operating systems like Windows 7 if all logical cores are given work.

            //Generally, the more memory bandwidth you have relative to CPU compute throughput, and the more collision detection heavy the simulation is relative to solving,
            //the more benefit you get out of SMT/hyperthreading. 
            //For example, if you're using the 64 core quad memory channel AMD 3990x on a scene composed of thousands of ragdolls, 
            //there won't be enough memory bandwidth to even feed half the physical cores. Using all 128 logical cores would just add overhead.

            //It may be worth using something like hwloc to extract extra information to reason about.
            var targetThreadCount = Math.Max(1,
                Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;
            _camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 25, 160), size);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            
            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(new SpringSettings(30, 1)),
                new PoseIntegratorCallbacks(new NumericVector3(0, -10, 0)), new SolveDescription(8, 1));

            _sphereHandles = new List<BodyHandle>();
            _activeBoxesWorld = new List<Matrix>();
            _inactiveBoxesWorld = new List<Matrix>();
            _spheresWorld = new List<Matrix>();
            _random = new Random();
            _boxHandles = new List<BodyHandle>(800);
            _radii = new List<float>();

            var boxShape = new Box(1, 1, 1);
            var boxInertia = boxShape.ComputeInertia(1);
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
                        var bh = Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                            new NumericVector3((-columnCount * 0.5f + columnIndex) * boxShape.Width,
                                (rowIndex + 0.5f) * boxShape.Height,
                                (pyramidIndex - pyramidCount * 0.5f) * (boxShape.Length + 4)),
                            boxInertia,
                            new CollidableDescription(boxIndex, 0.1f),
                            new BodyActivityDescription(0.01f)));
                        _boxHandles.Add(bh);
                    }
                }
            }

            //Prevent the boxes from falling into the void.
            Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -0.5f, 0),
                Simulation.Shapes.Add(new Box(2500, 1, 2500))));

            _cubePrimitive = new CubePrimitive(GraphicsDevice, 1f, Color.White);

            _spherePrimitive = new SpherePrimitive(GraphicsDevice);

            var count = _boxHandles.Count;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            for (var index = 0; index < count; index++)
            {
                var bodyHandle = _boxHandles[index];
                var bodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);
                var position = bodyReference.Pose.Position;
                var quaternion = bodyReference.Pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                _cubePrimitive.Draw(world, _camera.View, _camera.Projection);
            }

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            //const float simulationDt = 1 / 60f;
            //Using a fixed time per update to match the demos simulation update rate.
            //time += 1 / 60f;
            //In the demos, we use one time step per frame. We don't bother modifying the physics time step duration for different monitors so different refresh rates
            //change the rate of simulation. This doesn't actually change the result of the simulation, though, and the simplicity is a good fit for the demos.
            //In the context of a 'real' application, you could instead use a time accumulator to take time steps of fixed length as needed, or
            //fully decouple simulation and rendering rates across different threads.
            //(In either case, you'd also want to interpolate or extrapolate simulation results during rendering for smoothness.)
            //Note that taking steps of variable length can reduce stability. Gradual or one-off changes can work reasonably well.
            Simulation.Timestep(1 / 60f, ThreadDispatcher);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Z) && _canShoot)
            {
                _canShoot = false;
                //Create the shape that we'll launch at the pyramids when the user presses a button.
                var radius = 0.5f + 5 * (float) _random.NextDouble();
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
                var velocity = _camera.FrontDirection * 30f;
                var position = new NumericVector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);
                var bodyDescription = BodyDescription.CreateConvexDynamic(position,
                    new BodyVelocity(new NumericVector3(velocity.X, velocity.Y, velocity.Z)),
                    bulletShape.Radius * bulletShape.Radius * bulletShape.Radius, Simulation.Shapes, bulletShape);

                var bodyHandle = Simulation.Bodies.Add(bodyDescription);

                _radii.Add(radius);
                _sphereHandles.Add(bodyHandle);
            }

            if (Game.CurrentKeyboardState.IsKeyUp(Keys.Z))
            {
                _canShoot = true;
            }

            _activeBoxesWorld.Clear();
            _inactiveBoxesWorld.Clear();
            var count = _boxHandles.Count;
            for (var index = 0; index < count; index++)
            {
                var bodyHandle = _boxHandles[index];
                var bodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);
                var position = bodyReference.Pose.Position;
                var quaternion = bodyReference.Pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                if (bodyReference.Awake)
                    _activeBoxesWorld.Add(world);
                else
                    _inactiveBoxesWorld.Add(world);
            }

            _spheresWorld.Clear();
            var sphereCount = _sphereHandles.Count;
            for (var index = 0; index < sphereCount; index++)
            {
                var bodyHandle = _sphereHandles[index];
                var bodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);
                var position = bodyReference.Pose.Position;
                var quaternion = bodyReference.Pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                _spheresWorld.Add(world);
            }

            _camera.Update(gameTime);

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            
            var count = _boxHandles.Count;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _cubePrimitive.Effect.DiffuseColor = new Vector3(1f, 0f, 0f);
            _activeBoxesWorld.ForEach(boxWorld => _cubePrimitive.Draw(boxWorld, _camera.View, _camera.Projection));
            _cubePrimitive.Effect.DiffuseColor = new Vector3(0.1f, 0.1f, 0.3f);
            _inactiveBoxesWorld.ForEach(boxWorld => _cubePrimitive.Draw(boxWorld, _camera.View, _camera.Projection));

            _spheresWorld.ForEach(sphereWorld => _spherePrimitive.Draw(sphereWorld, _camera.View, _camera.Projection));

            Game.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            Game.SpriteBatch.DrawString(_spriteFont, "Box handled: " + count + ".", new Vector2(GraphicsDevice.Viewport.Width - 500, 0), Color.White);
            Game.SpriteBatch.DrawString(_spriteFont, "Launch spheres with the 'Z' key.", new Vector2(GraphicsDevice.Viewport.Width - 500, 25), Color.White);
            Game.SpriteBatch.End();
            
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            // TODO check why Simulation.Dispose method sometimes fails
            Simulation.Dispose();

            BufferPool.Clear();

            ThreadDispatcher.Dispose();

            base.UnloadContent();
        }
    }
}