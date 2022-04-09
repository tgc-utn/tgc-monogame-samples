using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    public class ModelActionCollection : List<Action<ModelData>>, IActionCollection
    {
        public void AddSetWorld(EffectParameter parameter)
        {
            Add(data => parameter.SetValue(data.World));
        }

        public void AddSetWorldViewProjection(EffectParameter parameter)
        {
            Add(data => parameter.SetValue(data.GetWorldViewProjection()));
        }

        public void AddSetInverseTranspose(EffectParameter parameter)
        {
            Add(data => parameter.SetValue(Matrix.Transpose(Matrix.Invert(data.World))));
        }

        public void AddMainTexture(EffectParameter parameter)
        {
            Add(data => parameter.SetValue(data.Textures.First()));
        }

        public void Execute(ModelData data)
        {
            for (var index = 0; index < Count; index++)
                this[index].Invoke(data);
        }

    }
}