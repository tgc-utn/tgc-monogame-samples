using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Heightmaps.SimpleTerrain
{
    /// <summary>
    ///     Model On A Simple Terrain:
    ///     This sample demonstrates how to use information about a heightmap's vertex normals to follow the contour of the
    ///     terrain.
    ///     Author: Mariano Banquiero
    ///     TODO needs a refactor.
    /// </summary>
    public class ModelOnASimpleTerrain : TGCSample
    {
        public float angle;
        public Vector3 DesiredLookAt;
        public bool hay_lookAt;
        public Vector3 LookAt;

        private Model model;
        public Vector2 pos;
        public Vector3 tcCitoPos;
        public SimpleTerrain terrain;

        private float offSet = 0f;

        /// <inheritdoc />
        public ModelOnASimpleTerrain(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Heightmaps;
            Name = "Model On A Simple Terrain";
            Description =
                "This sample demonstrates how to use information about a heightmap's vertex normals to follow the contour of the terrain.";
        }

        private Camera Camera { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            DesiredLookAt = Vector3.Zero;
            pos = Vector2.Zero;
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(5000, 1300, 5000), DesiredLookAt,
                5, 50000);

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
            
            model = Game.Content.Load<Model>("3D/tgcito-classic/tgcito-classic");

            offSet = model.Meshes[0].BoundingSphere.Radius;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            var da = 0.01f;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left)) angle -= da;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right)) angle += da;

            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            float vel_lineal = 10;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up)) pos += dir * vel_lineal;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down)) pos -= dir * vel_lineal;

            var X = pos.X;
            var Z = pos.Y;

            tcCitoPos = new Vector3(X, terrain.Height(X, Z) + offSet, Z);
            DesiredLookAt = new Vector3(X, terrain.Height(X, Z), Z);
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

            var pos2 = pos - dir * 800;

            // obtengo la altura maxima desde la camara hasta tgcito
            float H = 0;
            for (var i = 0; i < 10; ++i)
            {
                var t = i / 10.0f;
                var p = pos2 * t + pos * (1 - t);
                var Hi = terrain.Height(p.X, p.Y) + 50;
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

            // computo 3 puntos sobre la superficie del heighmap
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var tan = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
            var pos_ade = pos + dir * 100;
            var pos_der = pos + tan * 100;
            var PosAdelante = new Vector3(pos_ade.X, terrain.Height(pos_ade.X, pos_ade.Y) + offSet, pos_ade.Y);
            var PosDerecha = new Vector3(pos_der.X, terrain.Height(pos_der.X, pos_der.Y) + offSet, pos_der.Y);

            var matWorld = CalcularMatrizOrientacion(10, tcCitoPos, PosAdelante, PosDerecha);

            // dibujo el mesh
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = matWorld;
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        // helper, calcula una matrix de world en base a la posicion, escalado y direccion del mesh
        public Matrix CalcularMatrizOrientacion(float scale, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var matWorld = Matrix.CreateScale(scale * 0.1f);

            // determino la orientacion
            var Dir = p1 - p0;
            Dir.Normalize();
            var Tan = p2 - p0;
            Tan.Normalize();
            var VUP = Vector3.Cross(Tan, Dir);
            VUP.Normalize();
            Tan = Vector3.Cross(VUP, Dir);
            Tan.Normalize();

            var V = VUP;
            var U = Tan;

            var Orientacion = new Matrix();
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.CreateTranslation(p0);
            return matWorld;
        }
    }
}