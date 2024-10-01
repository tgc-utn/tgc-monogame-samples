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
            _noDepth = new DepthStencilState();
            _noDepth.DepthBufferEnable = false;
            _noDepth.DepthBufferFunction = CompareFunction.Always;
        }

        private AxisLines _axisLines;

        private EffectPass _backgroundPass;

        private Color _baseColor;
        private EffectParameter _colorParameter;
        private ContentManager _content;
        private CubeGizmoGeometry _cube;
        private CylinderGizmoGeometry _cylinder;
        private DiskGizmoGeometry _disk;

        private Dictionary<GizmoGeometry, Dictionary<Color, List<Matrix>>> _drawInstances = new ();

        private Effect _effect;
        private EffectPass _foregroundPass;

        private GraphicsDevice _graphicsDevice;

        private LineSegmentGizmoGeometry _lineSegment;

        private readonly DepthStencilState _noDepth;
        private PolyLineGizmoGeometry _polyLine;
        private readonly Dictionary<Color, List<Vector3[]>> _polyLinesToDraw = new ();
        private Matrix _projection;
        private SphereGizmoGeometry _sphere;

        private Matrix _view;
        private Matrix _viewProjection;
        private EffectParameter _worldViewProjectionParameter;

        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Loads all the content necessary for drawing Gizmos.
        /// </summary>
        /// <param name="device">The GraphicsDevice to use when drawing. It is also used to bind buffers.</param>
        /// <param name="content">The ContentManager to manage Gizmos resources.</param>
        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            _graphicsDevice = device;

            _content = content;

            _effect = _content.Load<Effect>("Effects/Gizmos");

            _backgroundPass = _effect.CurrentTechnique.Passes["Background"];
            _foregroundPass = _effect.CurrentTechnique.Passes["Foreground"];
            _worldViewProjectionParameter = _effect.Parameters["WorldViewProjection"];
            _colorParameter = _effect.Parameters["Color"];

            _lineSegment = new LineSegmentGizmoGeometry(_graphicsDevice);
            _cube = new CubeGizmoGeometry(_graphicsDevice);
            _sphere = new SphereGizmoGeometry(_graphicsDevice, 20);
            _polyLine = new PolyLineGizmoGeometry(_graphicsDevice);
            _disk = new DiskGizmoGeometry(_graphicsDevice, 20);
            _cylinder = new CylinderGizmoGeometry(_graphicsDevice, 20);
            _axisLines = new AxisLines(_graphicsDevice, _content.Load<Model>("3D/geometries/arrow"));

            _drawInstances[_lineSegment] = new Dictionary<Color, List<Matrix>>();
            _drawInstances[_sphere] = new Dictionary<Color, List<Matrix>>();
            _drawInstances[_cube] = new Dictionary<Color, List<Matrix>>();
            _drawInstances[_disk] = new Dictionary<Color, List<Matrix>>();
            _drawInstances[_cylinder] = new Dictionary<Color, List<Matrix>>();
        }

        /// <summary>
        ///     Adds a draw instance specifying the geometry, its color and the world matrix to use when drawing.
        /// </summary>
        /// <param name="type">The GizmoGeometry to be drawn.</param>
        /// <param name="color">The color of the geometry.</param>
        /// <param name="world">The world matrix to be used when drawing.</param>
        private void AddDrawInstance(GizmoGeometry type, Color color, Matrix world)
        {
            var instancesByType = _drawInstances[type];
            instancesByType.TryAdd(color, new List<Matrix>());
            instancesByType[color].Add(world * _viewProjection);
        }

        /// <summary>
        ///     Draws a line between the points origin and destination using the Gizmos color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 destination)
        {
            DrawLine(origin, destination, _baseColor);
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
            AddDrawInstance(_lineSegment, color, world);
        }

        /// <summary>
        ///     Draws a wire cube with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size)
        {
            DrawCube(origin, size, _baseColor);
        }

        /// <summary>
        ///     Draws a wire cube with a World matrix using the Gizmos color.
        /// </summary>
        /// <param name="world">The World matrix of the cube.</param>
        public void DrawCube(Matrix world)
        {
            DrawCube(world, _baseColor);
        }

        /// <summary>
        ///     Draws a wire cube with a World matrix using the specified color.
        /// </summary>
        /// <param name="world">The World matrix of the cube.</param>
        /// <param name="color">The color of the cube.</param>
        public void DrawCube(Matrix world, Color color)
        {
            AddDrawInstance(_cube, color, world);
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
            AddDrawInstance(_cube, color, world);
        }

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size)
        {
            DrawSphere(origin, size, _baseColor);
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
            AddDrawInstance(_sphere, color, world);
        }

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the Gizmos color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        public void DrawPolyLine(Vector3[] points)
        {
            DrawPolyLine(points, _baseColor);
        }

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the specified color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        /// <param name="color">The color of the poly-line.</param>
        public void DrawPolyLine(Vector3[] points, Color color)
        {
            _polyLinesToDraw.TryAdd(color, new List<Vector3[]>());
            _polyLinesToDraw[color].Add(points);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the Gizmos color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        public void DrawFrustum(Matrix viewProjection)
        {
            DrawFrustum(viewProjection, _baseColor);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the specified color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        /// <param name="color">The color of the frustum.</param>
        public void DrawFrustum(Matrix viewProjection, Color color)
        {
            var world = CubeGizmoGeometry.CalculateFrustumWorld(viewProjection);
            AddDrawInstance(_cube, color, world);
        }

        /// <summary>
        ///     Draws a wire circle with an origin and normal direction using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius)
        {
            DrawDisk(origin, normal, radius, _baseColor);
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
            AddDrawInstance(_disk, color, world);
        }

        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size)
        {
            DrawCylinder(origin, rotation, size, _baseColor);
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
            AddDrawInstance(_cylinder, color, world);
        }

        /// <summary>
        ///     Draws a wire cylinder with a World matrix using the Gizmos color.
        /// </summary>
        /// <param name="world">The World matrix of the cylinder.</param>
        public void DrawCylinder(Matrix world)
        {
            DrawCylinder(world, _baseColor);
        }

        /// <summary>
        ///     Draws a wire cylinder with a World matrix using the specified color.
        /// </summary>
        /// <param name="world">The World matrix of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Matrix world, Color color)
        {
            AddDrawInstance(_cylinder, color, world);
        }

        /// <summary>
        ///     Sets the Gizmos color. All Gizmos drawn after are going to use this color if they do not specify one.
        /// </summary>
        /// <param name="color">The Gizmos color to set.</param>
        public void SetColor(Color color)
        {
            _baseColor = color;
        }

        /// <summary>
        ///     Updates the View and Projection matrices. Should be called on an Update loop after the camera is updated.
        /// </summary>
        /// <param name="view">The View matrix of a camera.</param>
        /// <param name="projection">The Projection matrix of a camera or a viewport.</param>
        public void UpdateViewProjection(Matrix view, Matrix projection)
        {
            _view = view;
            _projection = projection;
            _viewProjection = _view * _projection;
            _axisLines.SetView(view);
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
            var depth = _graphicsDevice.DepthStencilState;
            _graphicsDevice.DepthStencilState = _noDepth;

            DrawBaseGizmosGeometries(_backgroundPass);
            DrawPolyLines(_backgroundPass);

            // Restore our depth
            _graphicsDevice.DepthStencilState = depth;

            // Draw our foreground geometry
            DrawBaseGizmosGeometries(_foregroundPass);
            DrawPolyLines(_foregroundPass);

            _axisLines.Draw();

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
            foreach (var drawInstance in _drawInstances)
            {
                var geometry = drawInstance.Key;
                geometry.Bind();

                foreach (var colorEntry in drawInstance.Value)
                {
                    _colorParameter.SetValue(colorEntry.Key.ToVector3());

                    matrices = colorEntry.Value;
                    count = matrices.Count;

                    for (var index = 0; index < count; index++)
                    {
                        _worldViewProjectionParameter.SetValue(matrices[index]);
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
            _worldViewProjectionParameter.SetValue(Matrix.Identity);
            foreach (var polyLineInstance in _polyLinesToDraw)
            {
                _colorParameter.SetValue(polyLineInstance.Key.ToVector3());

                positions = polyLineInstance.Value;
                count = positions.Count;
                for (var index = 0; index < count; index++)
                {
                    pass.Apply();
                    _polyLine.Draw(positions[index]);
                }
            }
        }

        /// <summary>
        ///     Clears all the draw instances, so we don't draw the same as the past frame.
        /// </summary>
        private void CleanDrawInstances()
        {
            _polyLinesToDraw.Clear();

            _drawInstances[_lineSegment].Clear();
            _drawInstances[_sphere].Clear();
            _drawInstances[_cube].Clear();
            _drawInstances[_disk].Clear();
            _drawInstances[_cylinder].Clear();
        }

        /// <summary>
        ///     Disposes the used resources (geometries and content).
        /// </summary>
        public void Dispose()
        {
            _lineSegment.Dispose();
            _sphere.Dispose();
            _cube.Dispose();
            _disk.Dispose();
            _cylinder.Dispose();
            _effect.Dispose();
            _content.Dispose();
        }
    }
}