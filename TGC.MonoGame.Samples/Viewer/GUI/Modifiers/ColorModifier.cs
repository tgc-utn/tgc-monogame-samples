using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Numerics;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Color modifier that allows for changing a Color value in real time
    /// </summary>
    internal class ColorModifier : IModifier
    {
        private Vector4 _colorValue;
        private readonly string _name;
        private readonly Action<MonoGameColor> _onChange;

        /// <summary>
        ///     Creates a Color Modifier with a given name.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        private ColorModifier(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Creates a Color Modifier with a given name and an action on change.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">An action to be called when the Color changes</param>
        public ColorModifier(string name, Action<MonoGameColor> baseOnChange) : this(name)
        {
            _onChange += baseOnChange;
        }

        /// <summary>
        ///     Creates a Color Modifier with a given name, action on change and a default Color.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">An action to be called when the Color changes</param>
        /// <param name="defaultColor">The Color that the Color Modifier starts with</param>
        public ColorModifier(string name, Action<MonoGameColor> baseOnChange, MonoGameColor defaultColor) : this(name,
            baseOnChange)
        {
            _colorValue = Convert(defaultColor);
            baseOnChange.Invoke(defaultColor);
        }

        /// <summary>
        ///     Creates a Color Modifier with a given name and an <see cref="EffectParameter" />.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter" /> that will recieve the Color as value</param>
        public ColorModifier(string name, EffectParameter effectParameter) : this(name)
        {
            _onChange += x => effectParameter.SetValue(x.ToVector3());
        }

        /// <summary>
        ///     Creates a Color Modifier with a given name, an <see cref="EffectParameter" />, and a default Color.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter" /> that will recieve the Color as value</param>
        /// <param name="defaultColor">The Color that the Color Modifier starts with</param>
        public ColorModifier(string name, EffectParameter effectParameter, MonoGameColor defaultColor) : this(name,
            effectParameter)
        {
            _colorValue = Convert(defaultColor);
            effectParameter.SetValue(defaultColor.ToVector3());
        }

        /// <summary>
        ///     Draws the Color Modifier
        /// </summary>
        public void Draw()
        {
            if (ImGui.ColorEdit4(_name, ref _colorValue))
            {
                _onChange.Invoke(Convert(_colorValue));
            }
        }

        /// <summary>
        ///     Converts a Color from the MonoGame namespace to a System Vector4.
        /// </summary>
        /// <param name="color">The Color to be converted</param>
        /// <returns>A System Vector4 that holds the values of the Color</returns>
        private Vector4 Convert(MonoGameColor color)
        {
            var monoGameVector = color.ToVector4();
            return new Vector4(monoGameVector.X, monoGameVector.Y, monoGameVector.Z, monoGameVector.W);
        }

        /// <summary>
        ///     Converts a System Vector4 to a Color from the MonoGame namespace.
        /// </summary>
        /// <param name="vector">The Vector4 to be converted to a Color</param>
        /// <returns>A MonoGame Color that holds the values of the Vector4</returns>
        private MonoGameColor Convert(Vector4 vector)
        {
            return new MonoGameColor(vector.X, vector.Y, vector.Z, vector.W);
        }
    }
}