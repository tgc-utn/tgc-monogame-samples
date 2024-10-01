using ImGuiNET;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     An Options Modifier that allows for selecting a value from a list
    /// </summary>
    internal class OptionsModifier<EnumType> : IModifier where EnumType : Enum
    {
        private int _currentOptionIndex;

        private Action<EnumType> _onChange;

        private string _name;

        private string[] _options;


        /// <summary>
        ///     Creates an Options Modifiers with a name and option change listener.
        ///     This way to construct an Options Modifier uses the enum names as option names.
        ///     The default option is the first one on the enum type.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public OptionsModifier(string name, Action<EnumType> onChange) 
            : this(name, Enum.GetNames(typeof(EnumType)), default(EnumType), onChange)
        {

        }


        /// <summary>
        ///     Creates an Options Modifiers with a name, default option value and option change listener.
        ///     This way to construct an Options Modifier uses the enum names as option names.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="defaultValue">The default option value</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public OptionsModifier(string name, EnumType defaultValue, Action<EnumType> onChange) 
            : this(name, Enum.GetNames(typeof(EnumType)), defaultValue, onChange)
        {

        }

        /// <summary>
        ///     Creates an Options Modifiers with a name, option names, default option value and option change listener.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="optionNames">The sorted option names list</param>
        /// <param name="defaultValue">The default option value</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public OptionsModifier(string name, List<string> optionNames, EnumType defaultValue, Action<EnumType> onChange)
            : this(name, optionNames.ToArray(), defaultValue, onChange)
        {

        }

        /// <summary>
        ///     Creates an Options Modifiers with a name, option names, default option value and option change listener.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="optionNames">The sorted option names array</param>
        /// <param name="defaultValue">The default option value</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public OptionsModifier(string name, string[] optionNames, EnumType defaultValue, Action<EnumType> onChange)
        {
            _name = name;
            _options = optionNames;
            _currentOptionIndex = Convert.ToInt32(defaultValue);
            _onChange = onChange;
            _onChange.Invoke(defaultValue);
        }


        /// <summary>
        ///     Draws the Options Modifier
        /// </summary>
        public void Draw()
        {
            if (ImGui.Combo(_name, ref _currentOptionIndex, _options, _options.Length))
                _onChange.Invoke((EnumType)Enum.ToObject(typeof(EnumType), _currentOptionIndex));
        }

    }
}