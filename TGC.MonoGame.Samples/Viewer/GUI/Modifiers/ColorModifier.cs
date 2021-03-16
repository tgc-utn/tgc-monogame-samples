using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Numerics;
using MonoGameColor = Microsoft.Xna.Framework.Color;
namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    class ColorModifier : IModifier
    {
        private string Name;

        private Vector4 ColorValue;

        private event Action<MonoGameColor> OnChange;

        public ColorModifier(string name)
        {
            Name = name;
        }

        public ColorModifier(string name, Action<MonoGameColor> baseOnChange) : this(name)
        {
            OnChange += baseOnChange;
        }


        public ColorModifier(string name, Action<MonoGameColor> baseOnChange, MonoGameColor defaultColor) : this(name, baseOnChange)
        {
            ColorValue = Convert(defaultColor);
        }

        public ColorModifier(string name, EffectParameter effectParameter) : this(name)
        {
            OnChange += (x) => effectParameter.SetValue(x.ToVector3());
        }

        public ColorModifier(string name, EffectParameter effectParameter, MonoGameColor defaultColor) : this(name, effectParameter)
        {
            ColorValue = Convert(defaultColor);
            effectParameter.SetValue(defaultColor.ToVector3());
        }


        private Vector4 Convert(MonoGameColor color)
        {
            var monoGameVector = color.ToVector4();
            return new Vector4(monoGameVector.X, monoGameVector.Y, monoGameVector.Z, monoGameVector.W);
        }

        private MonoGameColor Convert(Vector4 vector)
        {
            return new MonoGameColor(vector.X, vector.Y, vector.Z, vector.W);
        }


        public void Draw()
        {
            bool valueChanged = ImGui.ColorEdit4(Name, ref ColorValue);
            if (valueChanged)
                OnChange.Invoke(Convert(ColorValue));
        }


    }
}
