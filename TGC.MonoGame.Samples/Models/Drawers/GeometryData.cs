using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    public struct GeometryData
    {
        internal List<Matrix> Matrices;
        internal List<Texture> Textures;
        internal List<Vector4> Vectors;
        internal List<float> Scalars;

        public GeometryData(List<Matrix> matrices = default, List<Texture> textures = default, List<Vector4> vectors = default, List<float> scalars = default)
        {
            Matrices = matrices ?? new List<Matrix>();
            Textures = textures ?? new List<Texture>();
            Vectors = vectors ?? new List<Vector4>();
            Scalars = scalars ?? new List<float>();
        }


        public GeometryData Clone()
        {
            // Trick to copy list
            var matrices = Matrices.ConvertAll(m => m);
            var textures = Textures.ConvertAll(t => t);
            var vectors = Vectors.ConvertAll(v => v);
            var scalars = Scalars.ConvertAll(s => s);

            return new GeometryData(matrices, textures, vectors, scalars);
        }
    }
}