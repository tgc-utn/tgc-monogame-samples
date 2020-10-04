using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Physics;

namespace TGC.MonoGame.Samples.Samples.CompleteSolutions.ComboRata
{
    // TODO needs a refactor.
    public class TunelMesh
    {
        public const int max_path = 800;
        public float angle;
        public float ant_pos;
        public bool colision;
        public int curr_obs;
        public short[] Indexes;
        public int level;
        public int NumberOfIndices;
        public int NumberOfVertices;
        public int[,] obstaculo = new int[2, max_path];
        public Vector3[] Path = new Vector3[max_path];
        public float pos;
        public Vector3 PosGamer;
        public Vector3 Position;
        public Vector3 Up;
        public float vel_lineal = 6.0f;
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
            float r = level == 0 ? 300 : 200;

            for (var s = 0; s < max_path; ++s)
            {
                Path[s] = new Vector3(MathF.Cos(s * 16f * MathF.PI / max_path) * 10000f, s * 10f,
                    MathF.Sin(s * 16f * MathF.PI / max_path) * 10000f);
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
                Path[s] = new Vector3(X * 10000f, s * 0f, Y * 10000f);
                */
            }

            var colores = new Color[6];
            colores[0] = new Color(255, 0, 0);
            colores[1] = new Color(0, 255, 0);
            colores[2] = new Color(0, 0, 255);
            colores[3] = new Color(255, 255, 0);
            colores[4] = new Color(0, 255, 255);
            colores[5] = new Color(255, 255, 255);
            for (var s = 0; s < max_path - 1; ++s)
            {
                var Q0 = Path[s];
                var Q1 = Path[s + 1];

                var dist = (Q1 - Q0).Length();
                var N = Vector3.Normalize(Q1 - Q0);
                var U = Vector3.Cross(new Vector3(0, 1, 0), N);
                var V = Vector3.Cross(N, U);

                for (var i = 0; i < step; ++i)
                {
                    var alfa_1 = 2.0f * MathF.PI / step * i;
                    var alfa_2 = 2.0f * MathF.PI / step * (i + 1);
                    var p1 = Q0 + U * MathF.Sin(alfa_1) * r + V * MathF.Cos(alfa_1) * r;
                    var p2 = Q0 + U * MathF.Sin(alfa_2) * r + V * MathF.Cos(alfa_2) * r;

                    var q1 = p1 + N * dist;
                    var q2 = p2 + N * dist;

                    var clr = colores[t / 4 % 6];
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

                    Indexes[k++] = (short) (t + 0);
                    Indexes[k++] = (short) (t + 1);
                    Indexes[k++] = (short) (t + 2);
                    Indexes[k++] = (short) (t + 2);
                    Indexes[k++] = (short) (t + 1);
                    Indexes[k++] = (short) (t + 3);

                    t += 4;
                }

                //                if (((s % 4 == 0 && level == 0) || (s % 2 == 0 && level == 1)) && s > 10)
                if ((s % 4 == 0 || level == 1) && s > 10)
                {
                    // obstalculo
                    var ct = new Vector2(10, 1);

                    if (level == 0)
                    {
                        // obstaculos en el nivel 0
                        obstaculo[0, s] = t; // primer vertice del obstaculo
                        var i = rnd.Next(0, step);
                        int[] ndx;
                        int tipo;
                        if (rnd.NextDouble() < 0.5)
                        {
                            ndx = new[] {0, 1, 2, 3};
                            tipo = 0;
                        }
                        else
                        {
                            ndx = new[] {0, 1, 4, 5};
                            tipo = 1;
                        }

                        for (var j = 0; j < 4; ++j)
                        {
                            var alfa = 2.0f * MathF.PI / step * (i + ndx[j]);
                            var p = Q0 + U * MathF.Sin(alfa) * r + V * MathF.Cos(alfa) * r;
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

                        Indexes[k++] = (short) (t + 0);
                        Indexes[k++] = (short) (t + 1);
                        Indexes[k++] = (short) (t + 2);

                        Indexes[k++] = (short) (t + 0);
                        Indexes[k++] = (short) (t + 2);
                        Indexes[k++] = (short) (t + 3);

                        t += 4;

                        if (rnd.NextDouble() < 0.5)
                        {
                            obstaculo[1, s] = t; // primer vertice del obstaculo
                            i += tipo == 0 ? 4 : 2;
                            for (var j = 0; j < 4; ++j)
                            {
                                var alfa = 2.0f * MathF.PI / step * (i + ndx[j]);
                                var p = Q0 + U * MathF.Sin(alfa) * r + V * MathF.Cos(alfa) * r;
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

                            Indexes[k++] = (short) (t + 0);
                            Indexes[k++] = (short) (t + 1);
                            Indexes[k++] = (short) (t + 2);

                            Indexes[k++] = (short) (t + 0);
                            Indexes[k++] = (short) (t + 2);
                            Indexes[k++] = (short) (t + 3);

                            t += 4;
                        }
                    }
                    else
                    {
                        // obstaculos en el nivel 1
                        for (var L = 0; L < 2; ++L)
                        {
                            obstaculo[L, s] = t; // primer vertice del obstaculo
                            var i = rnd.Next(0, step);
                            var alfa_0 = 2.0f * MathF.PI / step * i;
                            var p0 = Q0 + U * MathF.Sin(alfa_0) * r + V * MathF.Cos(alfa_0) * r;
                            var alfa_1 = 2.0f * MathF.PI / step * (i + 1);
                            var p1 = Q0 + U * MathF.Sin(alfa_1) * r + V * MathF.Cos(alfa_1) * r;
                            var n0 = p0 - Q0;
                            n0.Normalize();
                            var n1 = p1 - Q0;
                            n1.Normalize();
                            Vertices[t].Position = p0;
                            Vertices[t].TextureCoordinate = ct;
                            Vertices[t].Color = new Color(255, 100, 255);
                            Vertices[t + 1].Position = p0 + n0 * 100;
                            Vertices[t + 1].TextureCoordinate = ct;
                            Vertices[t + 1].Color = new Color(255, 100, 255);
                            Vertices[t + 2].Position = p1 + n1 * 100;
                            Vertices[t + 2].TextureCoordinate = ct;
                            Vertices[t + 2].Color = new Color(255, 100, 255);
                            Vertices[t + 3].Position = p1;
                            Vertices[t + 3].TextureCoordinate = ct;
                            Vertices[t + 3].Color = new Color(255, 100, 255);

                            Indexes[k++] = (short) (t + 0);
                            Indexes[k++] = (short) (t + 1);
                            Indexes[k++] = (short) (t + 2);

                            Indexes[k++] = (short) (t + 0);
                            Indexes[k++] = (short) (t + 2);
                            Indexes[k++] = (short) (t + 3);

                            t += 4;
                        }
                    }
                }
            }

            NumberOfVertices = t;
            NumberOfIndices = k;

            ant_pos = pos = 0;
            angle = 0;
        }

        public void Update(float elapsedTime, KeyboardState keyboardState)
        {
            var vel_an = 5f * (level == 0 ? 1 : -1);
            if (keyboardState.IsKeyDown(Keys.Left))
                angle -= elapsedTime * vel_an;

            if (keyboardState.IsKeyDown(Keys.Right))
                angle += elapsedTime * vel_an;

            ant_pos = pos;
            pos += elapsedTime * (keyboardState.IsKeyDown(Keys.Space) ? 0 : vel_lineal);
            var i = (int) Math.Floor(pos);

            var frac = pos - i;
            var Q0 = Path[i % max_path];
            var Q1 = Path[(i + 1) % max_path];
            var Q2 = Path[(i + 2) % max_path];
            var N0 = Vector3.Normalize(Q1 - Q0);
            var N1 = Vector3.Normalize(Q2 - Q1);

            var U = Vector3.Cross(new Vector3(0, 1, 0), N0);
            var V = Vector3.Cross(N0, U);
            Position = Q0 * (1 - frac) + Q1 * frac;
            ViewDir = N0 * (1 - frac) + N1 * frac;
            Up = V * MathF.Cos(angle) + U * MathF.Sin(angle);

            if (level == 0)
            {
                PosGamer = Position + ViewDir * 700 - Up * 250;
            }
            else
            {
                PosGamer = Position + ViewDir * 700 + Up * 200;
                Position += Up * 420;
            }

            // collision detect
            colision = false;
            for (var L = 0; L < 2 && !colision; ++L)
            for (var k = 1; k < 3 && !colision; ++k)
            {
                var p_obs = curr_obs = obstaculo[L, i + k];
                if (p_obs > 0)
                {
                    var p = PosGamer - ViewDir * 50;
                    var q = PosGamer + ViewDir * 50;

                    var p0 = Vertices[p_obs].Position;
                    var p1 = Vertices[p_obs + 1].Position;
                    var p2 = Vertices[p_obs + 2].Position;
                    var p3 = Vertices[p_obs + 3].Position;

                    var uvw = new Vector3();
                    var Ip = new Vector3();
                    float t;

                    if (TGCCollisionUtils.IntersectSegmentTriangle(p, q, p0, p1, p2, out uvw, out t, out Ip) ||
                        TGCCollisionUtils.IntersectSegmentTriangle(p, q, p0, p2, p3, out uvw, out t, out Ip))
                        colision = true;
                }
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            var ViewMatrix = Matrix.CreateLookAt(Position, Position + ViewDir, Up);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(ViewMatrix);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    Vertices, 0, NumberOfVertices,
                    Indexes, 0, NumberOfIndices / 3);
            }
        }
    }
}