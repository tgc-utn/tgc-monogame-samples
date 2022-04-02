using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Models.Drawers
{
    interface IActionCollection
    {
        void AddSetWorld(EffectParameter parameter);

        void AddSetWorldViewProjection(EffectParameter parameter);

        void AddSetInverseTranspose(EffectParameter parameter);

        void AddMainTexture(EffectParameter parameter);
    }
}
