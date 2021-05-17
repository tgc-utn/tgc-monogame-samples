using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Viewer
{
    /// <summary>
    ///     Renders Gizmos
    /// </summary>
    class GizmosRenderer : IGizmosRenderer
    {
        private Dictionary<GizmoGeometry, Dictionary<Color, List<Matrix>>> DrawInstances = new Dictionary<GizmoGeometry, Dictionary<Color, List<Matrix>>>();
        private Dictionary<Color, List<Vector3[]>> PolyLinesToDraw = new Dictionary<Color, List<Vector3[]>>();

        private LineSegmentGizmoGeometry LineSegment;
        private CubeGizmoGeometry Cube;
        private SphereGizmoGeometry Sphere;
        private PolyLineGizmoGeometry PolyLine;
        private DiskGizmoGeometry Disk;
        private CylinderGizmoGeometry Cylinder;
        private AxisLines AxisLines;

        private GraphicsDevice GraphicsDevice;
        private ContentManager Content;

        private Effect Effect;

        private EffectPass BackgroundPass;
        private EffectPass ForegroundPass;
        private EffectParameter WorldViewProjectionParameter;
        private EffectParameter ColorParameter;

        private DepthStencilState NoDepth;

        private Matrix View;
        private Matrix Projection;
        private Matrix ViewProjection;

        private Color BaseColor;


        /// <summary>
        ///     Creates a GizmosRenderer.
        /// </summary>
        public GizmosRenderer()
        {
            NoDepth = new DepthStencilState();
            NoDepth.DepthBufferEnable = false;
            NoDepth.DepthBufferFunction = CompareFunction.Always;
        }

        /// <summary>
        ///     Loads all the content necessary for drawing Gizmos.
        /// </summary>
        /// <param name="device">The GraphicsDevice to use when drawing. It is also used to bind buffers.</param>
        /// <param name="content">The ContentManager to manage Gizmos resources.</param>
        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            GraphicsDevice = device;

            Content = content;

            Effect = Content.Load<Effect>("Effects/Gizmos");

            BackgroundPass = Effect.CurrentTechnique.Passes["Background"];
            ForegroundPass = Effect.CurrentTechnique.Passes["Foreground"];
            WorldViewProjectionParameter = Effect.Parameters["WorldViewProjection"];
            ColorParameter = Effect.Parameters["Color"];

            LineSegment = new LineSegmentGizmoGeometry(GraphicsDevice);
            Cube = new CubeGizmoGeometry(GraphicsDevice);
            Sphere = new SphereGizmoGeometry(GraphicsDevice, 20);
            PolyLine = new PolyLineGizmoGeometry(GraphicsDevice);
            Disk = new DiskGizmoGeometry(GraphicsDevice, 20);
            Cylinder = new CylinderGizmoGeometry(GraphicsDevice, 20);
            AxisLines = new AxisLines(GraphicsDevice, Content.Load<Model>("3D/geometries/arrow"));

            DrawInstances[LineSegment] = new Dictionary<Color, List<Matrix>>();
            DrawInstances[Sphere] = new Dictionary<Color, List<Matrix>>();
            DrawInstances[Cube] = new Dictionary<Color, List<Matrix>>();
            DrawInstances[Disk] = new Dictionary<Color, List<Matrix>>();
            DrawInstances[Cylinder] = new Dictionary<Color, List<Matrix>>();
        }


        /// <summary>
        ///     Adds a draw instance specifying the geometry, its color and the world matrix to use when drawing. 
        /// </summary>
        /// <param name="type">The GizmoGeometry to be drawn.</param>
        /// <param name="color">The color of the geometry.</param>
        /// <param name="world">The world matrix to be used when drawing.</param>
        private void AddDrawInstance(GizmoGeometry type, Color color, Matrix world)
        {
            var instancesByType = DrawInstances[type];
            instancesByType.TryAdd(color, new List<Matrix>());
            instancesByType[color].Add(world * ViewProjection);
        }

        /// <inheritdoc />
        public void DrawLine(Vector3 origin, Vector3 destination)
        {
            DrawLine(origin, destination, BaseColor);
        }

        /// <inheritdoc />
        public void DrawLine(Vector3 origin, Vector3 destination, Color color)
        {
            var world = LineSegmentGizmoGeometry.CalculateWorld(origin, destination);
            AddDrawInstance(LineSegment, color, world);
        }


        /// <inheritdoc />
        public void DrawCube(Vector3 origin, Vector3 size)
        {
            DrawCube(origin, size, BaseColor);
        }

        /// <inheritdoc />
        public void DrawCube(Matrix world)
        {
            DrawCube(world, BaseColor);
        }


        /// <inheritdoc />
        public void DrawCube(Matrix world, Color color)
        {
            AddDrawInstance(Cube, color, world);
        }


        /// <inheritdoc />
        public void DrawCube(Vector3 origin, Vector3 size, Color color)
        {
            var world = CubeGizmoGeometry.CalculateWorld(origin, size);
            DrawCube(world, color);
        }

        /// <inheritdoc />
        public void DrawSphere(Vector3 origin, Vector3 size)
        {
            DrawSphere(origin, size, BaseColor);
        }

        /// <inheritdoc />
        public void DrawSphere(Vector3 origin, Vector3 size, Color color)
        {
            var world = SphereGizmoGeometry.CalculateWorld(origin, size);
            AddDrawInstance(Sphere, color, world);
        }


        /// <inheritdoc />
        public void DrawPolyLine(Vector3[] points)
        {
            DrawPolyLine(points);
        }


        /// <inheritdoc />
        public void DrawPolyLine(Vector3[] points, Color color)
        {
            PolyLinesToDraw.TryAdd(color, new List<Vector3[]>());
            PolyLinesToDraw[color].Add(points);
        }

        /// <inheritdoc />
        public void DrawFrustum(Matrix viewProjection)
        {
            DrawFrustum(viewProjection, BaseColor);
        }

        /// <inheritdoc />
        public void DrawFrustum(Matrix viewProjection, Color color)
        {
            var world = CubeGizmoGeometry.CalculateFrustumWorld(viewProjection);
            AddDrawInstance(Cube, color, world);
        }

        /// <inheritdoc />
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius)
        {
            DrawDisk(origin, normal, radius, BaseColor);
        }

        /// <inheritdoc />
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius, Color color)
        {
            var world = DiskGizmoGeometry.CalculateWorld(origin, normal, radius);
            AddDrawInstance(Disk, color, world);
        }

        /// <inheritdoc />
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size)
        {
            DrawCylinder(origin, rotation, size, BaseColor);
        }


        /// <inheritdoc />
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size, Color color)
        {
            var world = CylinderGizmoGeometry.CalculateWorld(origin, rotation, size);
            DrawCylinder(world, color);
        }


        /// <inheritdoc />
        public void DrawCylinder(Matrix world)
        {
            DrawCylinder(world, BaseColor);
        }


        /// <inheritdoc />
        public void DrawCylinder(Matrix world, Color color)
        {
            AddDrawInstance(Cylinder, color, world);
        }

        /// <inheritdoc />
        public void SetColor(Color color)
        {
            BaseColor = color;
        }

        /// <inheritdoc />
        public void UpdateMatrices(Matrix view, Matrix projection)
        {
            View = view;
            Projection = projection;
            ViewProjection = View * Projection;
            AxisLines.SetMatrices(view, projection);
        }

        /// <inheritdoc />
        void IGizmosRenderer.Draw()
        {
            // Save our depth state, then use ours
            var depth = GraphicsDevice.DepthStencilState;
            GraphicsDevice.DepthStencilState = NoDepth;

            DrawBaseGizmosGeometries(BackgroundPass);
            DrawPolyLines(BackgroundPass);

            // Restore our depth
            GraphicsDevice.DepthStencilState = depth;

            // Draw our foreground geometry
            DrawBaseGizmosGeometries(ForegroundPass);
            DrawPolyLines(ForegroundPass);

            AxisLines.Draw();

            CleanDrawinstances();
        }


        /// <summary>
        ///     Draws all Gizmos that are sub-classes of GizmoGeometry.
        /// </summary>
        /// <param name="pass">The pass from an effect to draw the geometry with.</param>
        private void DrawBaseGizmosGeometries(EffectPass pass)
        {
            var count = 0;
            List<Matrix> matrices;
            foreach (var drawInstance in DrawInstances)
            {
                var geometry = drawInstance.Key;
                geometry.Bind();

                foreach (var colorEntry in drawInstance.Value)
                {
                    ColorParameter.SetValue(colorEntry.Key.ToVector3());

                    matrices = colorEntry.Value;
                    count = matrices.Count;

                    for (int index = 0; index < count; index++)
                    {
                        WorldViewProjectionParameter.SetValue(matrices[index]);
                        pass.Apply();
                        geometry.Draw();
                    }
                }
            }
        }


        /// <summary>
        ///     Draws all Gizmos that are poly-lines.
        /// </summary>
        /// <param name="pass">The pass from an effect to draw the geometry with.</param>
        private void DrawPolyLines(EffectPass pass)
        {
            var count = 0;
            List<Vector3[]> positions;
            WorldViewProjectionParameter.SetValue(Matrix.Identity);
            foreach (var polyLineInstance in PolyLinesToDraw)
            {
                ColorParameter.SetValue(polyLineInstance.Key.ToVector3());

                positions = polyLineInstance.Value;
                count = positions.Count;
                for (int index = 0; index < count; index++)
                {
                    pass.Apply();
                    PolyLine.Draw(positions[index]);
                }
            }
        }


        /// <summary>
        ///    Clears all the draw instances, so we don't draw the same as the past frame.
        /// </summary>
        private void CleanDrawinstances()
        {
            PolyLinesToDraw.Clear();

            DrawInstances[LineSegment].Clear();
            DrawInstances[Sphere].Clear();
            DrawInstances[Cube].Clear();
            DrawInstances[Disk].Clear();
            DrawInstances[Cylinder].Clear();
        }

        /// <summary>
        ///    Disposes the used resources (geometries and content).
        /// </summary>
        public void Dispose()
        {
            LineSegment.Dispose();
            Sphere.Dispose();
            Cube.Dispose();
            Disk.Dispose();
            Cylinder.Dispose();
            Effect.Dispose();
            Content.Dispose();
        }
    }
}
