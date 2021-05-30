using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Float Modifier that allows for changing a Float value in real time
    /// </summary>
    public class FloatModifier : IModifier
    {
        private string Name;

        private float FloatValue;

        private event Action<float> OnChange;

        private bool HasBounds = false;

        private float Min;

        private float Max;


        /// <summary>
        ///     Creates a Float Modifier with a given name, an action on change and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public FloatModifier(string name, Action<float> baseOnChange, float defaultValue)
        {
            Name = name;
            OnChange += baseOnChange;
            FloatValue = defaultValue;
            baseOnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an action on change, a default value, and a minimum and maximum values for the float.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        /// <param name="min">The minimum value for the modifier</param>
        /// <param name="max">The maximum value for the modifier</param>
        public FloatModifier(string name, Action<float> baseOnChange, float defaultValue, float min, float max) : this(name, baseOnChange, defaultValue)
        {
            HasBounds = true;
            Min = min;
            Max = max;
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an <see cref="EffectParameter"/> and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public FloatModifier(string name, EffectParameter effectParameter, float defaultValue) 
            : this(name, (x) => effectParameter.SetValue(x), defaultValue)
        {

        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an <see cref="EffectParameter"/>, a default value, and a minimum and maximum values for the float.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        /// <param name="min">The minimum value for the modifier</param>
        /// <param name="max">The maximum value for the modifier</param>
        public FloatModifier(string name, EffectParameter effectParameter, float defaultValue, float min, float max) 
            : this(name, (x) => effectParameter.SetValue(x), defaultValue, min, max)
        {

        }

        /// <summary>
        ///     Draws the Float Modifier.
        /// </summary>
        public void Draw()
        {
            bool valueChanged = HasBounds ? 
                ImGui.DragFloat(Name, ref FloatValue, 0.01f, Min, Max) : 
                ImGui.DragFloat(Name, ref FloatValue);

            if (valueChanged)
                OnChange.Invoke(FloatValue);
        }
    }
}
