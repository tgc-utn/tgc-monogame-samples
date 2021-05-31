﻿using ImGuiNET;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     An Options Modifier that allows for selecting a value from a list
    /// </summary>
    internal class OptionsModifier : IModifier
    {
        private int _currentOption;

        /// <summary>
        ///     Creates an Options Modifier with a given name, options, default option and an action.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="options">A list of options to be displayed and selected</param>
        /// <param name="defaultOption">The index of the option that is selected by default</param>
        /// <param name="onChange">An action to be called when the selected option changes</param>
        public OptionsModifier(string name, List<string> options, int defaultOption, Action<int, string> onChange) :
            this(name, options.ToArray(), defaultOption, onChange)
        {
        }

        /// <summary>
        ///     Creates an Options Modifier with a given name, options, default option and an action.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="options">An array of options to be displayed and selected</param>
        /// <param name="defaultOption">The index of the option that is selected by default</param>
        /// <param name="onChange">An action to be called when the selected option changes</param>
        public OptionsModifier(string name, string[] options, int defaultOption, Action<int, string> onChange)
        {
            Name = name;
            Options = options;
            _currentOption = defaultOption;
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
                OnChange.Invoke(_currentOption, Options[_currentOption]);
        }

        private event Action<int, string> OnChange;
    }
}