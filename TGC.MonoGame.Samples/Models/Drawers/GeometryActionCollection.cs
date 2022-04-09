using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    public class GeometryActionCollection : List<Action<ModelData, GeometryData>>, IActionCollection
    {
        public bool Empty => Count == 0;

        public void AddSetWorld(EffectParameter parameter)
        {
            Add((modelData, geometryData) => parameter.SetValue(geometryData.Matrices.First() * modelData.World));
        }

        public void AddSetWorldViewProjection(EffectParameter parameter)
        {
            Add((modelData, geometryData) => 
                parameter.SetValue(geometryData.Matrices.First() * modelData.GetWorldViewProjection()));
        }

        public void AddSetInverseTranspose(EffectParameter parameter)
        {
            Add((modelData, geometryData) =>
                parameter.SetValue(Matrix.Transpose(Matrix.Invert(geometryData.Matrices.First() * modelData.World))));
        }

        public void AddMainTexture(EffectParameter parameter)
        {
            Add((modelData, geometryData) => parameter.SetValue(geometryData.Textures.First()));
        }


        internal void Execute(ModelData modelData, GeometryData geometryData)
        {
            for (var index = 0; index < Count; index++)
                this[index].Invoke(modelData, geometryData);
        }
    }
}