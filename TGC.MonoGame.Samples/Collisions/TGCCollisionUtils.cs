using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Collisions
{
    /// <summary>
    ///     Utilidades para hacer detección de colisiones
    /// </summary>
    public class TGCCollisionUtils
    {
        /// <summary>
        ///     Detecta colision entre un segmento pq y un triangulo abc.
        ///     Devuelve true si hay colision y carga las coordenadas barycentricas (u,v,w) de la colision, el
        ///     instante t de colision y el punto c de colision.
        ///     Basado en: Real Time Collision Detection pag 191
        /// </summary>
        /// <param name="p">Inicio del segmento</param>
        /// <param name="q">Fin del segmento</param>
        /// <param name="a">Vertice 1 del triangulo</param>
        /// <param name="b">Vertice 2 del triangulo</param>
        /// <param name="c">Vertice 3 del triangulo</param>
        /// <param name="uvw">Coordenadas barycentricas de colision</param>
        /// <param name="t">Instante de colision</param>
        /// <param name="col">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool IntersectSegmentTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c,
            out Vector3 uvw, out float t, out Vector3 col)
        {
            float u;
            float v;
            float w;
            uvw = Vector3.Zero;
            col = Vector3.Zero;
            t = -1;

            var ab = b - a;
            var ac = c - a;
            var qp = p - q;

            // Compute triangle normal. Can be precalculated or cached if intersecting multiple segments against the same triangle
            var n = Vector3.Cross(ab, ac);

            // Compute denominator d. If d <= 0, segment is parallel to or points away from triangle, so exit early
            var d = Vector3.Dot(qp, n);
            if (d <= 0.0f) return false;

            // Compute intersection t value of pq with plane of triangle. A ray intersects iff 0 <= t.
            // Segment intersects iff 0 <= t <= 1. Delay dividing by d until intersection has been found to pierce triangle
            var ap = p - a;
            t = Vector3.Dot(ap, n);
            if (t < 0.0f) return false;
            if (t > d) return false; // For segment; exclude this code line for a ray test

            // Compute barycentric coordinate components and test if within bounds
            var e = Vector3.Cross(qp, ap);
            v = Vector3.Dot(ac, e);
            if (v < 0.0f || v > d) return false;
            w = -Vector3.Dot(ab, e);
            if (w < 0.0f || v + w > d) return false;

            // Segment/ray intersects triangle. Perform delayed division and compute the last barycentric coordinate component
            var ood = 1.0f / d;
            t *= ood;
            v *= ood;
            w *= ood;
            u = 1.0f - v - w;

            uvw.X = u;
            uvw.Y = v;
            uvw.Z = w;
            col = p + t * (p - q);
            return true;
        }
    }
}