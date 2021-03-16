using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    public class FloatModifier : IModifier
    {
        private string Name;

        private float FloatValue;

        private event Action<float> OnChange;

        private bool HasBounds = false;

        private float Min;

        private float Max;

        public FloatModifier(string name)
        {
            Name = name;
        }

        public FloatModifier(string name, Action<float> baseOnChange) : this(name)
        {
            OnChange += baseOnChange;
        }

        public FloatModifier(string name, Action<float> baseOnChange, float defaultValue) : this(name, baseOnChange)
        {
            FloatValue = defaultValue;
        }

        public FloatModifier(string name, Action<float> baseOnChange, float defaultValue, float min, float max) : this(name, baseOnChange, defaultValue)
        {
            HasBounds = true;
            Min = min;
            Max = max;
        }

        public FloatModifier(string name, EffectParameter effectParameter) : this(name)
        {
            OnChange += (x) => effectParameter.SetValue(x);
        }

        public FloatModifier(string name, EffectParameter effectParameter, float defaultValue) : this(name, effectParameter)
        {
            FloatValue = defaultValue;
            effectParameter.SetValue(defaultValue);
        }

        public FloatModifier(string name, EffectParameter effectParameter, float defaultValue, float min, float max) : this(name, effectParameter, defaultValue)
        {
            HasBounds = true;
            Min = min;
            Max = max;
        }

        public void Draw()
        {
            bool valueChanged = HasBounds ? 
                ImGui.DragFloat(Name, ref FloatValue, 0.01f, Min, Max) : 
                ImGui.DragFloat(Name, ref FloatValue);
            if (valueChanged)
                OnChange.Invoke(FloatValue);
        }

        public static void ModifyEffect(EffectParameter parameter, float value)
        {
            parameter.SetValue(value);
        }

    }
}
