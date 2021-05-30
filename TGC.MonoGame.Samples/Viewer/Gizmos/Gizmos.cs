using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Viewer.Gizmos.Geometries;

namespace TGC.MonoGame.Samples.Viewer.Gizmos
{
    /// <summary>
    ///     Renders Gizmos
    /// </summary>
    public class Gizmos
    {
        /// <summary>
        ///     Creates a GizmosRenderer.
        /// </summary>
        public Gizmos()
        {
            NoDepth = new DepthStencilState();
            NoDepth.DepthBufferEnable = false;
            NoDepth.DepthBufferFunction = CompareFunction.Always;
        }

        private AxisLines AxisLines { get; set; }

        private EffectPass BackgroundPass { get; set; }

        private Color BaseColor { get; set; }
        private EffectParameter ColorParameter { get; set; }
        private ContentManager Content { get; set; }
        private CubeGizmoGeometry Cube { get; set; }
        private CylinderGizmoGeometry Cylinder { get; set; }
        private DiskGizmoGeometry Disk { get; set; }

        private Dictionary<GizmoGeometry, Dictionary<Color, List<Matrix>>> DrawInstances { get; } =
            new Dictionary<GizmoGeometry, Dictionary<Color, List<Matrix>>>();

        private Effect Effect { get; set; }
        private EffectPass ForegroundPass { get; set; }

        private GraphicsDevice GraphicsDevice { get; set; }

        private LineSegmentGizmoGeometry LineSegment { get; set; }

        private DepthStencilState NoDepth { get; }
        private PolyLineGizmoGeometry PolyLine { get; set; }
        private Dictionary<Color, List<Vector3[]>> PolyLinesToDraw { get; } = new Dictionary<Color, List<Vector3[]>>();
        private Matrix Projection { get; set; }
        private SphereGizmoGeometry Sphere { get; set; }

        private Matrix View { get; set; }
        private Matrix ViewProjection { get; set; }
        private EffectParameter WorldViewProjectionParameter { get; set; }

        public bool Enabled { get; set; } = true;

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

        /// <summary>
        ///     Draws a line between the points origin and destination using the Gizmos color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 destination)
        {
            DrawLine(origin, destination, BaseColor);
        }

        /// <summary>
        ///     Draws a line between the points origin and destination using the specified color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 destination, Color color)
        {
            var world = LineSegmentGizmoGeometry.CalculateWorld(origin, destination);
            AddDrawInstance(LineSegment, BaseColor, world);
        }

        /// <summary>
        ///     Draws a wire cube with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size)
        {
            DrawCube(origin, size, BaseColor);
        }

        /// <summary>
        ///     Draws a wire cube with a World matrix using the Gizmos color.
        /// </summary>
        /// <param name="world">The World matrix of the cube.</param>
        public void DrawCube(Matrix world)
        {
            DrawCube(world, BaseColor);
        }

        /// <summary>
        ///     Draws a wire cube with a World matrix using the specified color.
        /// </summary>
        /// <param name="world">The World matrix of the cube.</param>
        /// <param name="color">The color of the cube.</param>
        public void DrawCube(Matrix world, Color color)
        {
            AddDrawInstance(Cube, color, world);
        }

        /// <summary>
        ///     Draws a wire cube with an origin and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        /// <param name="color">The color of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size, Color color)
        {
            var world = CubeGizmoGeometry.CalculateWorld(origin, size);
            AddDrawInstance(Cube, color, world);
        }

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size)
        {
            DrawSphere(origin, size, BaseColor);
        }

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        /// <param name="color">The color of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size, Color color)
        {
            var world = SphereGizmoGeometry.CalculateWorld(origin, size);
            AddDrawInstance(Sphere, color, world);
        }

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the Gizmos color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        public void DrawPolyLine(Vector3[] points)
        {
            DrawPolyLine(points, BaseColor);
        }

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the specified color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        /// <param name="color">The color of the poly-line.</param>
        public void DrawPolyLine(Vector3[] points, Color color)
        {
            PolyLinesToDraw.TryAdd(color, new List<Vector3[]>());
            PolyLinesToDraw[color].Add(points);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the Gizmos color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        public void DrawFrustum(Matrix viewProjection)
        {
            DrawFrustum(viewProjection, BaseColor);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the specified color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        /// <param name="color">The color of the frustum.</param>
        public void DrawFrustum(Matrix viewProjection, Color color)
        {
            var world = CubeGizmoGeometry.CalculateFrustumWorld(viewProjection);
            AddDrawInstance(Cube, color, world);
        }

        /// <summary>
        ///     Draws a wire circle with an origin and normal direction using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius)
        {
            DrawDisk(origin, normal, radius, BaseColor);
        }

        /// <summary>
        ///     Draws a wire disk (a circle) with an origin and normal direction using the specified color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        /// <param name="color">The color of the disk.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius, Color color)
        {
            var world = DiskGizmoGeometry.CalculateWorld(origin, normal, radius);
            AddDrawInstance(Disk, color, world);
        }

        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size)
        {
            DrawCylinder(origin, rotation, size, BaseColor);
        }

        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size, Color color)
        {
            var world = CylinderGizmoGeometry.CalculateWorld(origin, rotation, size);
            AddDrawInstance(Cylinder, color, world);
        }

        /// <summary>
        ///     Draws a wire cylinder with a World matrix using the Gizmos color.
        /// </summary>
        /// <param name="world">The World matrix of the cylinder.</param>
        public void DrawCylinder(Matrix world)
        {
            DrawCylinder(world, BaseColor);
        }

        /// <summary>
        ///     Draws a wire cylinder with a World matrix using the specified color.
        /// </summary>
        /// <param name="world">The World matrix of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Matrix world, Color color)
        {
            AddDrawInstance(Cylinder, color, world);
        }

        /// <summary>
        ///     Sets the Gizmos color. All Gizmos drawn after are going to use this color if they do not specify one.
        /// </summary>
        /// <param name="color">The Gizmos color to set.</param>
        public void SetColor(Color color)
        {
            BaseColor = color;
        }

        /// <summary>
        ///     Updates the View and Projection matrices. Should be called on an Update loop after the camera is updated.
        /// </summary>
        /// <param name="view">The View matrix of a camera.</param>
        /// <param name="projection">The Projection matrix of a camera or a viewport.</param>
        public void UpdateViewProjection(Matrix view, Matrix projection)
        {
            View = view;
            Projection = projection;
            ViewProjection = View * Projection;
            AxisLines.SetMatrices(view, projection);
        }

        /// <summary>
        ///     Effectively draws the geometry using the parameters from past draw calls. Should be used after calling the other
        ///     draw methods.
        /// </summary>
        public void Draw()
        {
            if (!Enabled)
                return;

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

            CleanDrawInstances();
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

                    for (var index = 0; index < count; index++)
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
                for (var index = 0; index < count; index++)
                {
                    pass.Apply();
                    PolyLine.Draw(positions[index]);
                }
            }
        }

        /// <summary>
        ///     Clears all the draw instances, so we don't draw the same as the past frame.
        /// </summary>
        private void CleanDrawInstances()
        {
            PolyLinesToDraw.Clear();

            DrawInstances[LineSegment].Clear();
            DrawInstances[Sphere].Clear();
            DrawInstances[Cube].Clear();
            DrawInstances[Disk].Clear();
            DrawInstances[Cylinder].Clear();
        }

        /// <summary>
        ///     Disposes the used resources (geometries and content).
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