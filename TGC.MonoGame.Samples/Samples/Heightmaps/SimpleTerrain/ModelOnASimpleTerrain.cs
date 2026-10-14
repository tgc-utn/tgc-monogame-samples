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
        private float _angle;
        private Vector3 _desiredLookAt;
        private bool _hayLookAt;
        private Vector3 _lookAt;

        private Model model;
        private Vector2 _pos;
        private Vector3 _tgcitoPos;
        private SimpleTerrain _terrain;

        private float offSet;

        /// <inheritdoc />
        public ModelOnASimpleTerrain(TGCViewer game)
            : base(game)
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
            _desiredLookAt = Vector3.Zero;
            _pos = Vector2.Zero;
            Camera = new TargetCamera(
                GraphicsDevice.Viewport.AspectRatio,
                new Vector3(5000, 1300, 5000),
                _desiredLookAt,
                5,
                50000);

            base.Initialize();
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            var da = 0.01f;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left))
            {
                _angle -= da;
            }

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right))
            {
                _angle += da;
            }

            var dir = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
            float vel_lineal = 10;
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up))
            {
                _pos += dir * vel_lineal;
            }

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down))
            {
                _pos -= dir * vel_lineal;
            }

            var x = _pos.X;
            var z = _pos.Y;

            _tgcitoPos = new Vector3(x, _terrain.Height(x, z) + offSet, z);
            _desiredLookAt = new Vector3(x, _terrain.Height(x, z), z);
            if (!_hayLookAt)
            {
                _lookAt = _desiredLookAt;
                _hayLookAt = true;
            }
            else
            {
                var lamda = 0.05f;
                _lookAt = (_desiredLookAt * lamda) + (_lookAt * (1 - lamda));
            }

            var pos2 = _pos - (dir * 800);

            // I get the maximum height from the camera to tgcito.
            float h = 0;
            for (var i = 0; i < 10; ++i)
            {
                var t = i / 10.0f;
                var p = (pos2 * t) + (_pos * (1 - t));
                var hi = _terrain.Height(p.X, p.Y) + 50;
                if (hi > h)
                {
                    h = hi;
                }
            }

            var position = new Vector3(pos2.X, _desiredLookAt.Y + h, pos2.Y);
            Camera.View = Matrix.CreateLookAt(position, _lookAt, new Vector3(0, 1, 0));

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // I draw the terrain, turning off the backface culling
            var oldRasterizerState = GraphicsDevice.RasterizerState;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            _terrain.Draw(Matrix.Identity, Camera.View, Camera.Projection);
            GraphicsDevice.RasterizerState = oldRasterizerState;

            // compute 3 points on the heightmap surface
            var dir = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
            var tan = new Vector2(-MathF.Sin(_angle), MathF.Cos(_angle));
            var pos_ade = _pos + (dir * 100);
            var pos_der = _pos + (tan * 100);
            var posAdelante = new Vector3(pos_ade.X, _terrain.Height(pos_ade.X, pos_ade.Y) + offSet, pos_ade.Y);
            var posDerecha = new Vector3(pos_der.X, _terrain.Height(pos_der.X, pos_der.Y) + offSet, pos_der.Y);

            var matWorld = CalcularMatrizOrientacion(10, _tgcitoPos, posAdelante, posDerecha);

            // I draw the mesh
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

        // helper, calculates a world matrix based on the position, scaling, and direction of the mesh
        public Matrix CalcularMatrizOrientacion(float scale, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var matWorld = Matrix.CreateScale(scale * 0.1f);

            // I set the orientation
            var dir = p1 - p0;
            dir.Normalize();
            var tan = p2 - p0;
            tan.Normalize();
            var vUP = Vector3.Cross(tan, dir);
            vUP.Normalize();
            tan = Vector3.Cross(vUP, dir);
            tan.Normalize();

            var v = vUP;
            var u = tan;

            var orientacion = new Matrix();
            orientacion.M11 = u.X;
            orientacion.M12 = u.Y;
            orientacion.M13 = u.Z;
            orientacion.M14 = 0;

            orientacion.M21 = v.X;
            orientacion.M22 = v.Y;
            orientacion.M23 = v.Z;
            orientacion.M24 = 0;

            orientacion.M31 = dir.X;
            orientacion.M32 = dir.Y;
            orientacion.M33 = dir.Z;
            orientacion.M34 = 0;

            orientacion.M41 = 0;
            orientacion.M42 = 0;
            orientacion.M43 = 0;
            orientacion.M44 = 1;
            matWorld = matWorld * orientacion;

            // transfer
            matWorld = matWorld * Matrix.CreateTranslation(p0);
            return matWorld;
        }

        /// <inheritdoc/>
        protected override void LoadContent()
        {
            var terrainEffect = Game.Content.Load<Effect>(ContentFolderEffects + "Terrain");

            // heights
            var terrainHeigthmap = Game.Content.Load<Texture2D>(ContentFolderTextures + "Heightmaps/heightmap");

            // basic color
            var terrainColorMap = Game.Content.Load<Texture2D>(ContentFolderTextures + "Heightmaps/colormap");

            // blend texture 1
            var terrainGrass = Game.Content.Load<Texture2D>(ContentFolderTextures + "grass");

            // blend texture 2
            var terrainGround = Game.Content.Load<Texture2D>(ContentFolderTextures + "ground");
            _terrain = new SimpleTerrain(GraphicsDevice, terrainHeigthmap, terrainColorMap, terrainGrass, terrainGround, terrainEffect);

            model = Game.Content.Load<Model>("3D/tgcito-classic/tgcito-classic");

            offSet = model.Meshes[0].BoundingSphere.Radius;

            base.LoadContent();
        }
    }
}
