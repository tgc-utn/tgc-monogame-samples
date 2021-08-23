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

        private int _currentOption;

        private Action<EnumType> OnChange;

        public OptionsModifier(string name, Action<EnumType> onChange) : this(name, Enum.GetNames(typeof(EnumType)), default(EnumType), onChange)
        {

        }

        public OptionsModifier(string name, EnumType defaultValue, Action<EnumType> onChange) : this(name, Enum.GetNames(typeof(EnumType)), defaultValue, onChange)
        {

        }

        public OptionsModifier(string name, string[] optionNames, EnumType defaultValue, Action<EnumType> onChange)
        {
            Name = name;
            Options = optionNames;
            _currentOption = Convert.ToInt32(defaultValue);
            OnChange = onChange;
        }

        private string Name { get; }

        private string[] Options { get; set; }

        /// <summary>
        ///     Draws the Options Modifier
        /// </summary>
        public void Draw()
        {
            if (ImGui.Combo(Name, ref _currentOption, Options, Options.Length))
                //Option.OnOptionSelected()
                //OnChange.Invoke(_currentOption, Options[_currentOption]);
                OnChange.Invoke((EnumType)Enum.ToObject(typeof(EnumType), _currentOption));
        }

    }
}