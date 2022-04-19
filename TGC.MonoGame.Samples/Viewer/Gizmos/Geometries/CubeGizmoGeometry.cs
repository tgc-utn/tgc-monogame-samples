using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing Wire Cubes.
    /// </summary>
    class CubeGizmoGeometry : GizmoGeometry
    {
        private static Matrix PrecalculatedFrustumTransform = Matrix.CreateTranslation(Vector3.Backward * 0.5f) * Matrix.CreateScale(new Vector3(2f, 2f, 1f));

        /// <summary>
        ///     Creates a Wire Cube.
        /// </summary>
        /// <param name="graphicsDevice">A GraphicsDevice to bind the geometry.</param>
        public CubeGizmoGeometry(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            var vertices = new VertexPosition[8]
            {
                new VertexPosition(new Vector3(0.5f, 0.5f, 0.5f)),
                new VertexPosition(new Vector3(-0.5f, 0.5f, 0.5f)),
                new VertexPosition(new Vector3(0.5f, -0.5f, 0.5f)),
                new VertexPosition(new Vector3(-0.5f, -0.5f, 0.5f)),
                new VertexPosition(new Vector3(0.5f, 0.5f, -0.5f)),
                new VertexPosition(new Vector3(-0.5f, 0.5f, -0.5f)),
                new VertexPosition(new Vector3(0.5f, -0.5f, -0.5f)),
                new VertexPosition(new Vector3(-0.5f, -0.5f, -0.5f))
            };
            var indices = new ushort[24] 
            { 
                0, 1, 
                0, 2,  
                1, 3,
                3, 2, 

                4, 5, 
                4, 6, 
                5, 7,  
                7, 6,   

                0, 4,
                1, 5,
                2, 6,
                3, 7
            };
            InitializeVertices(vertices);
            InitializeIndices(indices);
        }

        /// <summary>
        ///     Calculates a World matrix for this cube.
        /// </summary>
        /// <param name="origin">The position in World space.</param>
        /// <param name="size">The scale of the cube.</param>
        /// <returns>The calculated World matrix.</returns>
        public static Matrix CalculateWorld(Vector3 origin, Vector3 size)
        {
            return Matrix.CreateScale(size) * Matrix.CreateTranslation(origin);
        }
        
        /// <summary>
        ///     Calculates a World matrix for a frustum.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix, generally from a camera or an application.</param>
        /// <returns>The calculated World matrix.</returns>
        public static Matrix CalculateFrustumWorld(Matrix viewProjection)
        {
            return PrecalculatedFrustumTransform * Matrix.Invert(viewProjection);
        }
    }
}
