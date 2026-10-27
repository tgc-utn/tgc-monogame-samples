using System;
using System.Collections.Generic;

using ImGuiNET;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     An Options Modifier that allows for selecting a value from a list.
    /// </summary>
    /// <typeparam name="TEnumType">The enum whose values are the selectable options.</typeparam>
    public class OptionsModifier<TEnumType> : IModifier
        where TEnumType : Enum
    {
        private int _currentOptionIndex;

        private readonly Action<TEnumType> _onChange;

        private readonly string _name;

        private readonly string[] _options;

        /// <summary>
        ///     Creates an Options Modifiers with a name and option change listener.
        ///     This way to construct an Options Modifier uses the enum names as option names.
        ///     The default option is the first one on the enum type.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI.</param>
        /// <param name="onChange">An action to be called when the option value changes.</param>
        public OptionsModifier(string name, Action<TEnumType> onChange)
            : this(name, Enum.GetNames(typeof(TEnumType)), default(TEnumType), onChange)
        {
        }

        /// <summary>
        ///     Creates an Options Modifiers with a name, default option value and option change listener.
        ///     This way to construct an Options Modifier uses the enum names as option names.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI.</param>
        /// <param name="defaultValue">The default option value.</param>
        /// <param name="onChange">An action to be called when the option value changes.</param>
        public OptionsModifier(string name, TEnumType defaultValue, Action<TEnumType> onChange)
            : this(name, Enum.GetNames(typeof(TEnumType)), defaultValue, onChange)
        {
        }

        /// <summary>
        ///     Creates an Options Modifiers with a name, option names, default option value and option change listener.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI.</param>
        /// <param name="optionNames">The sorted option names list.</param>
        /// <param name="defaultValue">The default option value.</param>
        /// <param name="onChange">An action to be called when the option value changes.</param>
        public OptionsModifier(string name, List<string> optionNames, TEnumType defaultValue, Action<TEnumType> onChange)
            : this(name, optionNames.ToArray(), defaultValue, onChange)
        {
        }

        /// <summary>
        ///     Creates an Options Modifiers with a name, option names, default option value and option change listener.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI.</param>
        /// <param name="optionNames">The sorted option names array.</param>
        /// <param name="defaultValue">The default option value.</param>
        /// <param name="onChange">An action to be called when the option value changes.</param>
        public OptionsModifier(string name, string[] optionNames, TEnumType defaultValue, Action<TEnumType> onChange)
        {
            _name = name;
            _options = optionNames;
            _currentOptionIndex = Convert.ToInt32(defaultValue);
            _onChange = onChange;
            _onChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Draws the Options Modifier.
        /// </summary>
        public void Draw()
        {
            if (ImGui.Combo(_name, ref _currentOptionIndex, _options, _options.Length))
            {
                _onChange.Invoke((TEnumType)Enum.ToObject(typeof(TEnumType), _currentOptionIndex));
            }
        }
    }
}
