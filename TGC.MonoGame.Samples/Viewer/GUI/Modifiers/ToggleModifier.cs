using ImGuiNET;
using System;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Toggle Modifier that enables or disable a boolean value
    /// </summary>
    public class ToggleModifier : IModifier
    {
        private bool _checked;

        private string Name { get; set; }

        private Action<bool> OnChange { get; set; }

        /// <summary>
        ///     Creates a Toggle Modifier with a given name and action
        /// </summary>
        /// <param name="name">The name of the Toggle Modifier</param>
        /// <param name="onChange">An action to be called when the value of the modifier changes</param>
        public ToggleModifier(string name, Action<bool> onChange)
        {
            Name = name;
            OnChange = onChange;
        }

        /// <summary>
        ///     Creates a Toggle Modifier with a given name, action and default value
        /// </summary>
        /// <param name="name">The name of the Toggle Modifier</param>
        /// <param name="onChange">An action to be called when the value of the modifier changes</param>
        /// <param name="defaultValue">The default value that this modifier will have</param>
        public ToggleModifier(string name, Action<bool> onChange, bool defaultValue)
        {
            Name = name;
            OnChange = onChange;
            _checked = defaultValue;
            OnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Draws the Toggle Modifier
        /// </summary>
        public void Draw()
        {
            if (ImGui.Checkbox(Name, ref _checked))
                OnChange.Invoke(_checked);
        }

    }
}