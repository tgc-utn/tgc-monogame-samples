using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Modifier to change values in <see cref="Samples.TGCSample"/>.
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        ///     Draws the Modifier
        /// </summary>
        void Draw();
    }
}
