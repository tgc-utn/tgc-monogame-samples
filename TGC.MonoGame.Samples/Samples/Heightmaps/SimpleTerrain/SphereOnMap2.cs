using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Heightmaps.SimpleTerrain
{
    public class SphereOnMap2 : TGCSample
    {
        private const float SphereRotatingVelocity = 0.06f;


        public float angle;
        public Vector3 DesiredLookAt;
        public bool hay_lookAt;
        public Vector3 LookAt;

        public Vector2 pos;
        public SimpleTerrain terrain;
        public float offSet = 0f;
        public Vector3 spherePos;

        private Matrix SphereRotation { get; set; }

        /// <inheritdoc />
        public SphereOnMap2(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Heightmaps;
            Name = "Sphere On Map 2";
            Description = "Sphere On Map 2";
        }

        private Camera Camera { get; set; }

        private SpherePrimitive Sphere { get; set; }
        private Matrix SphereTranslation { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            DesiredLookAt = Vector3.Zero;
            pos = Vector2.Zero;
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(5000, 1300, 5000), DesiredLookAt,
                5, 50000);

            float diameter = 20;
            Sphere = new SpherePrimitive(GraphicsDevice, diameter, 32, Color.Black);

            offSet = (diameter / 2) + 50;

            SphereRotation = Matrix.Identity;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var terrainEffect = Game.Content.Load<Effect>(ContentFolderEffects + "Terrain");
            // alturas pp dichas
            var terrainHeigthmap = Game.Content.Load<Texture2D>(ContentFolderTextures + "Heightmaps/heightmap");
            // color basico
            var terrainColorMap = Game.Content.Load<Texture2D>(ContentFolderTextures + "Heightmaps/colormap");
            // blend texture 1
            var terrainGrass = Game.Content.Load<Texture2D>(ContentFolderTextures + "grass");
            // blend texture 2
            var terrainGround = Game.Content.Load<Texture2D>(ContentFolderTextures + "ground");
            terrain = new SimpleTerrain(GraphicsDevice, terrainHeigthmap, terrainColorMap, terrainGrass, terrainGround, terrainEffect);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            var da = 0.01f;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left)) {
                angle -= da;
            }
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right)) {
                angle += da;
            } 

            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            float vel_lineal = 5;
            Matrix sphereRotationZ;
            Matrix sphereRotationX;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up)) {
                pos += dir * vel_lineal;
                if (dir.X > 0)
                {
                    sphereRotationZ = Matrix.CreateRotationZ(dir.X * -SphereRotatingVelocity);
                }
                else {
                    sphereRotationZ = Matrix.CreateRotationZ(dir.X * -SphereRotatingVelocity);
                }
                if (dir.Y > 0)
                {
                    sphereRotationX = Matrix.CreateRotationX(dir.Y * SphereRotatingVelocity);
                }
                else {
                    sphereRotationX = Matrix.CreateRotationX(dir.Y * SphereRotatingVelocity);
                }

                SphereRotation *= sphereRotationZ * sphereRotationX;
            }
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down)) {
                pos -= dir * vel_lineal;
                if (dir.X > 0)
                {
                    sphereRotationZ = Matrix.CreateRotationZ(dir.X * SphereRotatingVelocity);
                }
                else {
                    sphereRotationZ = Matrix.CreateRotationZ(dir.X * SphereRotatingVelocity);
                }
                if (dir.Y > 0)
                {
                    sphereRotationX = Matrix.CreateRotationX(dir.Y * -SphereRotatingVelocity);
                }
                else {
                    sphereRotationX = Matrix.CreateRotationX(dir.Y * -SphereRotatingVelocity);
                }

                SphereRotation *= sphereRotationZ * sphereRotationX;
            } 

            var X = pos.X;
            var Z = pos.Y;

            spherePos = new Vector3(X, terrain.Height(X, Z) + offSet, Z);
            DesiredLookAt = new Vector3(X, terrain.Height(X, Z) + offSet, Z);
            if (!hay_lookAt)
            {
                LookAt = DesiredLookAt;
                hay_lookAt = true;
            }
            else
            {
                var lamda = 0.05f;
                LookAt = DesiredLookAt * lamda + LookAt * (1 - lamda);
            }

            var pos2 = pos - dir * 300;

            // obtengo la altura maxima desde la camara hasta el auto
            float H = 0;
            for (var i = 0; i < 10; ++i)
            {
                var t = i / 10.0f;
                var p = pos2 * t + pos * (1 - t);
                var Hi = terrain.Height(p.X, p.Y) + offSet + 50;
                if (Hi > H) H = Hi;
            }

            var Position = new Vector3(pos2.X, DesiredLookAt.Y + H, pos2.Y);
            Camera.View = Matrix.CreateLookAt(Position, LookAt, new Vector3(0, 1, 0));

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);


            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // dibujo el terreno, apagando el backface culling
            var oldRasterizerState = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            terrain.Draw(Matrix.Identity, Camera.View, Camera.Projection);
            GraphicsDevice.RasterizerState = oldRasterizerState;

            DrawGeometry(Sphere, Matrix.CreateScale(50 * 0.1f) * SphereRotation * Matrix.CreateTranslation(spherePos));

            base.Draw(gameTime);
        }

        private void DrawGeometry(GeometricPrimitive geometry, Matrix transform)
        {
            var effect = geometry.Effect;

            effect.World = transform;
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;

            geometry.Draw(effect);
        }
    }

}