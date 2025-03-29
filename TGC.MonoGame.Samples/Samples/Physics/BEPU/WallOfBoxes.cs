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
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.Samples.Physics.Bepu;
using TGC.MonoGame.Samples.Viewer;
using NumericVector3 = System.Numerics.Vector3;

namespace TGC.MonoGame.Samples.Samples.Physics.Bepu
{
    /// <summary>
    ///     Wall Of Boxes:
    ///     Sample of how to use the Bepu physics engine with simple geometries.
    ///     Authors: Leandro Javier Laiño, Rodrigo Nicolás Garcia, Ronán Ariel Vinitzca.
    /// </summary>
    public class WallOfBoxes : TGCSample
    {
        /// <inheritdoc />
        public WallOfBoxes(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Physics;
            Name = "BepuPhysics - Wall of boxes";
            Description = "Time to drop boxes!";
        }

        private QuadPrimitive _floor;
        private Matrix _floorWorld;
        private BoxPrimitive _box;

        /// <summary>
        ///     We'll randomize the size of bullets.
        /// </summary>
        private Random _random;

        private SpherePrimitive _sphere;
        private List<float> _radii;
        private List<BodyHandle> _sphereHandles;
        private List<BodyHandle> _boxHandles;
        private List<Matrix> _boxesWorld;
        private List<Matrix> _spheresWorld;
        private bool _canShoot;

        private Effect _tilingEffect;

        /// <summary>
        ///     Gets the buffer pool used by the demo's simulation.
        ///     Note that the buffer pool used by the simulation is not considered to be *owned* by the simulation.
        ///     The simulation merely uses the pool.
        ///     Disposing the simulation will not dispose or clear the buffer pool.
        /// </summary>
        private BufferPool _bufferPool;

        private Camera _camera;

        /// <summary>
        ///     Gets the simulation created by the sample's Initialize call.
        /// </summary>
        private Simulation _simulation;

        private SpriteFont _spriteFont;

        /// <summary>
        ///     Gets the thread dispatcher available for use by the simulation.
        /// </summary>
        private SimpleThreadDispatcher _threadDispatcher;

        /// <inheritdoc />
        public override void Initialize()
        {
            _canShoot = true;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            _random = new Random(5);

            _spriteFont = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            
            //The buffer pool is a source of raw memory blobs for the engine to use.
            _bufferPool = new BufferPool();

            _radii = new List<float>();
            _camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(40, 60, 150), 55, 0.4f);

            var boxTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-3");
            _box = new BoxPrimitive(GraphicsDevice, Vector3.One * 10, boxTexture);

            _sphere = new SpherePrimitive(GraphicsDevice);

            _sphereHandles = new List<BodyHandle>();
            _boxHandles = new List<BodyHandle>();

            var targetThreadCount = Math.Max(1,
                Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            _threadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

            // This are meshes/model/primitives collections to render
            // The PositionFirstTimestepper is the simplest timestepping mode, but since it integrates velocity into position at the start of the frame, directly modified velocities outside of the timestep
            // will be integrated before collision detection or the solver has a chance to intervene. That's fine in this demo. Other built-in options include the PositionLastTimestepper and the SubsteppingTimestepper.
            // Note that the timestepper also has callbacks that you can use for executing logic between processing stages, like BeforeCollisionDetection.
            _simulation = Simulation.Create(_bufferPool,
                new NarrowPhaseCallbacks(new SpringSettings(30, 1)),
                new PoseIntegratorCallbacks(new NumericVector3(0, -100, 0)),
                new SolveDescription(8, 1));

            // Creates a floor
            var floorTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "floor/adoquin-2");
            _floor = new QuadPrimitive(GraphicsDevice);

            _tilingEffect = Game.Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            _tilingEffect.Parameters["Texture"].SetValue(floorTexture);
            _tilingEffect.Parameters["Tiling"].SetValue(Vector2.One * 50f);

            _floorWorld = Matrix.CreateScale(400f) * Matrix.CreateTranslation(new Vector3(75,0, -150));
            _simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -0.5f, 0),
                _simulation.Shapes.Add(new Box(2000, 1, 2000))));

            _boxesWorld = new List<Matrix>();
            _spheresWorld = new List<Matrix>();

            // Single Box
            var radius = 10;
            for (var j = 0; j < 5; j++)
            {
                for (var i = 0; i < 20; i++)
                {
                    var boxShape = new Box(radius, radius, radius);
                    var boxInertia = boxShape.ComputeInertia(0.4f);
                    var boxIndex = _simulation.Shapes.Add(boxShape);
                    var position = new NumericVector3(-30 + i * 10 + 1, j * 10 + 1, -40);

                    var bodyDescription = BodyDescription.CreateDynamic(position, boxInertia, 
                        new CollidableDescription(boxIndex, 0.1f), new BodyActivityDescription(0.01f));

                    var bodyHandle = _simulation.Bodies.Add(bodyDescription);

                    _boxHandles.Add(bodyHandle);
                }
            }

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // In the demos, we use one time step per frame. We don't bother modifying the physics time step duration for different monitors so different refresh rates
            // change the rate of simulation. This doesn't actually change the result of the simulation, though, and the simplicity is a good fit for the demos.
            // In the context of a 'real' application, you could instead use a time accumulator to take time steps of fixed length as needed, or
            // fully decouple simulation and rendering rates across different threads.
            // (In either case, you'd also want to interpolate or extrapolate simulation results during rendering for smoothness.)
            // Note that taking steps of variable length can reduce stability. Gradual or one-off changes can work reasonably well.
            _simulation.Timestep(1 / 60f, _threadDispatcher);
            _camera.Update(gameTime);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Z) && _canShoot)
            {
                _canShoot = false;
                // Create the shape that we'll launch at the pyramids when the user presses a button.
                var radius = 0.5f + 5 * (float) _random.NextDouble();
                var bulletShape = new Sphere(radius);

                // Note that the use of radius^3 for mass can produce some pretty serious mass ratios. 
                // Observe what happens when a large ball sits on top of a few boxes with a fraction of the mass-
                // the collision appears much squishier and less stable. For most games, if you want to maintain rigidity, you'll want to use some combination of:
                // 1) Limit the ratio of heavy object masses to light object masses when those heavy objects depend on the light objects.
                // 2) Use a shorter timestep duration and update more frequently.
                // 3) Use a greater number of solver iterations.
                // #2 and #3 can become very expensive. In pathological cases, it can end up slower than using a quality-focused solver for the same simulation.
                // Unfortunately, at the moment, bepuphysics v2 does not contain any alternative solvers, so if you can't afford to brute force the the problem away,
                // the best solution is to cheat as much as possible to avoid the corner cases.
                var position = new NumericVector3(-40 + 210 * (float) _random.NextDouble(), 130, 130);
                var bodyDescription = BodyDescription.CreateConvexDynamic(position,
                    new BodyVelocity(new NumericVector3((float) _random.NextDouble(), 0, -110)),
                    bulletShape.Radius * bulletShape.Radius * bulletShape.Radius, _simulation.Shapes, bulletShape);

                var bodyHandle = _simulation.Bodies.Add(bodyDescription);

                _radii.Add(radius);
                _sphereHandles.Add(bodyHandle);
            }

            if (Game.CurrentKeyboardState.IsKeyUp(Keys.Z)) 
                _canShoot = true;


            _boxesWorld.Clear();
            var boxHandleCount = _boxHandles.Count;
            for (var index = 0; index < boxHandleCount; index++)
            {
                var pose = _simulation.Bodies.GetBodyReference(_boxHandles[index]).Pose;
                var position = pose.Position;
                var quaternion = pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                _boxesWorld.Add(world);
            }

            _spheresWorld.Clear();
            var sphereHandleCount = _sphereHandles.Count;
            for (var index = 0; index < sphereHandleCount; index++)
            {
                var pose = _simulation.Bodies.GetBodyReference(_sphereHandles[index]).Pose;
                var position = pose.Position;
                var quaternion = pose.Orientation;
                var world = Matrix.CreateScale(_radii[index]) *
                            Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                                quaternion.W)) *
                            Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                _spheresWorld.Add(world);
            }

            Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _tilingEffect.Parameters["WorldViewProjection"].SetValue(_floorWorld * _camera.View * _camera.Projection);
            _floor.Draw(_tilingEffect);
            _boxesWorld.ForEach(boxWorld => _box.Draw(boxWorld, _camera.View, _camera.Projection));
            _spheresWorld.ForEach(sphereWorld => _sphere.Draw(sphereWorld, _camera.View, _camera.Projection));

            Game.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            Game.SpriteBatch.DrawString(_spriteFont, "Launch spheres with the 'Z' key.", new Vector2(GraphicsDevice.Viewport.Width - 500, 0), Color.White);
            Game.SpriteBatch.End();
            
            base.Draw(gameTime);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            // If you intend to reuse the BufferPool, disposing the simulation is a good idea- it returns all the buffers to the pool for reuse.
            // Here, we dispose it, but it's not really required; we immediately thereafter clear the BufferPool of all held memory.
            // Note that failing to dispose buffer pools can result in memory leaks.
            _simulation.Dispose();

            _bufferPool.Clear();

            _threadDispatcher.Dispose();

            base.UnloadContent();
        }
    }
}