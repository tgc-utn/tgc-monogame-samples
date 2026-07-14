using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TGC.MonoGame.Samples.Collisions;

namespace TGC.MonoGame.Samples.Samples.CompleteSolutions.ComboRata
{
    // TODO needs a refactor.
    public class TunelMesh
    {
        private const int MaxPath = 800;
        public float Angle;
        public float Ant_pos;

        public bool Colision { get; private set; }

        private int _currObs;
        public short[] Indexes;

        public int Level { get; set; }

        public int NumberOfIndices;
        public int NumberOfVertices;
        private int[,] _obstaculo = new int[2, MaxPath];
        private readonly Vector3[] _path = new Vector3[MaxPath];

        public float Pos { get; private set; }

        public Vector3 PosGamer;
        public Vector3 Position;
        public Vector3 Up;
        private readonly float _velLineal = 6.0f;
        public VertexPositionColorTexture[] Vertices;
        public Vector3 ViewDir;

        public TunelMesh()
        {
            Vertices = new VertexPositionColorTexture[100000];
            Indexes = new short[100000];
            FillVertices();
        }

        public void FillVertices()
        {
            var t = 0;
            var k = 0;
            var step = 8;
            var rnd = new Random();
            float r = Level == 0 ? 300 : 200;

            for (var s = 0; s < MaxPath; ++s)
            {
                _path[s] = new Vector3(MathF.Cos(s * 16f * MathF.PI / MaxPath) * 10000f, s * 10f,
                    MathF.Sin(s * 16f * MathF.PI / MaxPath) * 10000f);
                /*
                //x = jCos(a * t) - Cos(b * t) ^ 3 y = Sin(c * t) - Sin(d * t) ^ 3;
                float ts = s * 0.01f;
                float a = 1;
                float b = 3;
                float c = 1;
                float d = 2;
                float X = MathF.Cos(a * ts) - MathF.Pow(MathF.Cos(b * ts), 3.0f);
                float Y = MathF.Sin(c * ts) - MathF.Pow(MathF.Sin(d * ts), 3.0f);
                //float X = s*0.1f;
                //float Y = 0;
                _path[s] = new Vector3(X * 10000f, s * 0f, Y * 10000f);
                */
            }

            var colores = new Color[6];
            colores[0] = new Color(255, 0, 0);
            colores[1] = new Color(0, 255, 0);
            colores[2] = new Color(0, 0, 255);
            colores[3] = new Color(255, 255, 0);
            colores[4] = new Color(0, 255, 255);
            colores[5] = new Color(255, 255, 255);
            for (var s = 0; s < MaxPath - 1; ++s)
            {
                var q0 = _path[s];
                var q11 = _path[s + 1];

                var dist = (q11 - q0).Length();
                var n = Vector3.Normalize(q11 - q0);
                var u = Vector3.Cross(new Vector3(0, 1, 0), n);
                var v = Vector3.Cross(n, u);

                for (var i = 0; i < step; ++i)
                {
                    var alfa_1 = 2.0f * MathF.PI / step * i;
                    var alfa_2 = 2.0f * MathF.PI / step * (i + 1);
                    var p1 = q0 + (u * MathF.Sin(alfa_1) * r) + (v * MathF.Cos(alfa_1) * r);
                    var p2 = q0 + (u * MathF.Sin(alfa_2) * r) + (v * MathF.Cos(alfa_2) * r);

                    var q1 = p1 + (n * dist);
                    var q2 = p2 + (n * dist);

                    var clr = colores[(t / 4) % 6];
                    Vertices[t + 0].Position = p1;
                    Vertices[t + 0].TextureCoordinate = new Vector2(0, 0);
                    Vertices[t + 0].Color = clr;
                    Vertices[t + 1].Position = p2;
                    Vertices[t + 1].TextureCoordinate = new Vector2(0, 1);
                    Vertices[t + 1].Color = clr;
                    Vertices[t + 2].Position = q1;
                    Vertices[t + 2].TextureCoordinate = new Vector2(1, 0);
                    Vertices[t + 2].Color = clr;
                    Vertices[t + 3].Position = q2;
                    Vertices[t + 3].TextureCoordinate = new Vector2(1, 1);
                    Vertices[t + 3].Color = clr;

                    Indexes[k++] = (short)(t + 0);
                    Indexes[k++] = (short)(t + 1);
                    Indexes[k++] = (short)(t + 2);
                    Indexes[k++] = (short)(t + 2);
                    Indexes[k++] = (short)(t + 1);
                    Indexes[k++] = (short)(t + 3);

                    t += 4;
                }

                // if (((s % 4 == 0 && level == 0) || (s % 2 == 0 && level == 1)) && s > 10)
                if ((s % 4 == 0 || Level == 1) && s > 10)
                {
                    // obstalculo
                    var ct = new Vector2(10, 1);

                    if (Level == 0)
                    {
                        // obstaculos en el nivel 0
                        _obstaculo[0, s] = t; // primer vertice del obstaculo
                        var i = rnd.Next(0, step);
                        int[] ndx;
                        int tipo;
                        if (rnd.NextDouble() < 0.5)
                        {
                            ndx = new[] { 0, 1, 2, 3 };
                            tipo = 0;
                        }
                        else
                        {
                            ndx = new[] { 0, 1, 4, 5 };
                            tipo = 1;
                        }

                        for (var j = 0; j < 4; ++j)
                        {
                            var alfa = 2.0f * MathF.PI / step * (i + ndx[j]);
                            var p = q0 + (u * MathF.Sin(alfa) * r) + (v * MathF.Cos(alfa) * r);
                            Vertices[t + j].Position = p;
                            Vertices[t + j].TextureCoordinate = ct;
                            Vertices[t + j].Color = new Color(255, 100, 255);
                        }

                        if (tipo == 1)
                        {
                            // hago el obstaculo un poco mas angosto
                            var up = Vertices[t + 1].Position - Vertices[t].Position;
                            up.Normalize();
                            Vertices[t].Position += up * 80;
                            Vertices[t + 1].Position -= up * 80;
                            Vertices[t + 2].Position -= up * 80;
                            Vertices[t + 3].Position += up * 80;
                        }

                        Indexes[k++] = (short)(t + 0);
                        Indexes[k++] = (short)(t + 1);
                        Indexes[k++] = (short)(t + 2);

                        Indexes[k++] = (short)(t + 0);
                        Indexes[k++] = (short)(t + 2);
                        Indexes[k++] = (short)(t + 3);

                        t += 4;

                        if (rnd.NextDouble() < 0.5)
                        {
                            _obstaculo[1, s] = t; // primer vertice del obstaculo
                            i += tipo == 0 ? 4 : 2;
                            for (var j = 0; j < 4; ++j)
                            {
                                var alfa = 2.0f * MathF.PI / step * (i + ndx[j]);
                                var p = q0 + (u * MathF.Sin(alfa) * r) + (v * MathF.Cos(alfa) * r);
                                Vertices[t + j].Position = p;
                                Vertices[t + j].TextureCoordinate = ct;
                                Vertices[t + j].Color = new Color(255, 100, 255);
                            }

                            if (tipo == 1)
                            {
                                // hago el obstaculo un poco mas angosto
                                var up = Vertices[t + 1].Position - Vertices[t].Position;
                                up.Normalize();
                                Vertices[t].Position += up * 80;
                                Vertices[t + 1].Position -= up * 80;
                                Vertices[t + 2].Position -= up * 80;
                                Vertices[t + 3].Position += up * 80;
                            }

                            Indexes[k++] = (short)(t + 0);
                            Indexes[k++] = (short)(t + 1);
                            Indexes[k++] = (short)(t + 2);

                            Indexes[k++] = (short)(t + 0);
                            Indexes[k++] = (short)(t + 2);
                            Indexes[k++] = (short)(t + 3);

                            t += 4;
                        }
                    }
                    else
                    {
                        // obstaculos en el nivel 1
                        for (var l = 0; l < 2; ++l)
                        {
                            _obstaculo[l, s] = t; // primer vertice del obstaculo
                            var i = rnd.Next(0, step);
                            var alfa_0 = 2.0f * MathF.PI / step * i;
                            var p0 = q0 + (u * MathF.Sin(alfa_0) * r) + (v * MathF.Cos(alfa_0) * r);
                            var alfa_1 = 2.0f * MathF.PI / step * (i + 1);
                            var p1 = q0 + (u * MathF.Sin(alfa_1) * r) + (v * MathF.Cos(alfa_1) * r);
                            var n0 = p0 - q0;
                            n0.Normalize();
                            var n1 = p1 - q0;
                            n1.Normalize();
                            Vertices[t].Position = p0;
                            Vertices[t].TextureCoordinate = ct;
                            Vertices[t].Color = new Color(255, 100, 255);
                            Vertices[t + 1].Position = p0 + (n0 * 100);
                            Vertices[t + 1].TextureCoordinate = ct;
                            Vertices[t + 1].Color = new Color(255, 100, 255);
                            Vertices[t + 2].Position = p1 + (n1 * 100);
                            Vertices[t + 2].TextureCoordinate = ct;
                            Vertices[t + 2].Color = new Color(255, 100, 255);
                            Vertices[t + 3].Position = p1;
                            Vertices[t + 3].TextureCoordinate = ct;
                            Vertices[t + 3].Color = new Color(255, 100, 255);

                            Indexes[k++] = (short)(t + 0);
                            Indexes[k++] = (short)(t + 1);
                            Indexes[k++] = (short)(t + 2);

                            Indexes[k++] = (short)(t + 0);
                            Indexes[k++] = (short)(t + 2);
                            Indexes[k++] = (short)(t + 3);

                            t += 4;
                        }
                    }
                }
            }

            NumberOfVertices = t;
            NumberOfIndices = k;

            Ant_pos = Pos = 0;
            Angle = 0;
        }

        public void Update(float elapsedTime, KeyboardState keyboardState)
        {
            var vel_an = 5f * (Level == 0 ? 1 : -1);
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                Angle -= elapsedTime * vel_an;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                Angle += elapsedTime * vel_an;
            }

            Ant_pos = Pos;
            Pos += elapsedTime * (keyboardState.IsKeyDown(Keys.Space) ? 0 : _velLineal);
            var i = (int)Math.Floor(Pos);

            var frac = Pos - i;
            var q0 = _path[i % MaxPath];
            var q1 = _path[(i + 1) % MaxPath];
            var q2 = _path[(i + 2) % MaxPath];
            var n0 = Vector3.Normalize(q1 - q0);
            var n1 = Vector3.Normalize(q2 - q1);

            var u = Vector3.Cross(new Vector3(0, 1, 0), n0);
            var v = Vector3.Cross(n0, u);
            Position = (q0 * (1 - frac)) + (q1 * frac);
            ViewDir = (n0 * (1 - frac)) + (n1 * frac);
            Up = (v * MathF.Cos(Angle)) + (u * MathF.Sin(Angle));

            if (Level == 0)
            {
                PosGamer = Position + (ViewDir * 700) - (Up * 250);
            }
            else
            {
                PosGamer = Position + (ViewDir * 700) + (Up * 200);
                Position += Up * 420;
            }

            // collision detect
            Colision = false;
            for (var l = 0; l < 2 && !Colision; ++l)
            {
                for (var k = 1; k < 3 && !Colision; ++k)
                {
                    var p_obs = _currObs = _obstaculo[l, i + k];
                    if (p_obs > 0)
                    {
                        var p = PosGamer - (ViewDir * 50);
                        var q = PosGamer + (ViewDir * 50);

                        var p0 = Vertices[p_obs].Position;
                        var p1 = Vertices[p_obs + 1].Position;
                        var p2 = Vertices[p_obs + 2].Position;
                        var p3 = Vertices[p_obs + 3].Position;

                        var uvw = new Vector3();
                        var ip = new Vector3();
                        float t;

                        if (TGCCollisionUtils.IntersectSegmentTriangle(p, q, p0, p1, p2, out uvw, out t, out ip) ||
                            TGCCollisionUtils.IntersectSegmentTriangle(p, q, p0, p2, p3, out uvw, out t, out ip))
                        {
                            Colision = true;
                        }
                    }
                }
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            var viewMatrix = Matrix.CreateLookAt(Position, Position + ViewDir, Up);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(viewMatrix);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    Vertices, 0, NumberOfVertices,
                    Indexes, 0, NumberOfIndices / 3);
            }
        }
    }
}
