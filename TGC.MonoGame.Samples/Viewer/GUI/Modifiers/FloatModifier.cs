﻿using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Float Modifier that allows for changing a Float value in real time
    /// </summary>
    public class FloatModifier : IModifier
    {
        private float _floatValue;

        private string _name;

        private bool _hasBounds;

        private float _min;

        private float _max;

        /// <summary>
        ///     Creates a Float Modifier with a given name.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        public FloatModifier(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name and an action on change.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">The action to be called when the value changes</param>
        public FloatModifier(string name, Action<float> baseOnChange) : this(name)
        {
            OnChange += baseOnChange;
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an action on change and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public FloatModifier(string name, Action<float> baseOnChange, float defaultValue) : this(name, baseOnChange)
        {
            _floatValue = defaultValue;
            baseOnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an action on change, a default value, and a minimum and maximum values
        ///     for the float.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="baseOnChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        /// <param name="min">The minimum value for the modifier</param>
        /// <param name="max">The maximum value for the modifier</param>
        public FloatModifier(string name, Action<float> baseOnChange, float defaultValue, float min, float max) : this(
            name, baseOnChange, defaultValue)
        {
            _hasBounds = true;
            _min = min;
            _max = max;
            baseOnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name and an <see cref="EffectParameter" />.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter" /> that will recieve the Float as value</param>
        public FloatModifier(string name, EffectParameter effectParameter) : this(name)
        {
            OnChange += x => effectParameter.SetValue(x);
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an <see cref="EffectParameter" /> and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter" /> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public FloatModifier(string name, EffectParameter effectParameter, float defaultValue) : this(name,
            effectParameter)
        {
            _floatValue = defaultValue;
            effectParameter.SetValue(defaultValue);
        }

        /// <summary>
        ///     Creates a Float Modifier with a given name, an <see cref="EffectParameter" />, a default value, and a minimum and
        ///     maximum values for the float.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter" /> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        /// <param name="min">The minimum value for the modifier</param>
        /// <param name="max">The maximum value for the modifier</param>
        public FloatModifier(string name, EffectParameter effectParameter, float defaultValue, float min, float max) :
            this(name, effectParameter, defaultValue)
        {
            _hasBounds = true;
            _min = min;
            _max = max;
        }


        /// <summary>
        ///     Draws the Float Modifier.
        /// </summary>
        public void Draw()
        {
            var valueChanged = _hasBounds
                ? ImGui.DragFloat(_name, ref _floatValue, 0.01f * (_max - _min), _min, _max)
                : ImGui.DragFloat(_name, ref _floatValue);

            if (valueChanged)
                OnChange.Invoke(_floatValue);
        }

        private event Action<float> OnChange;
    }
}