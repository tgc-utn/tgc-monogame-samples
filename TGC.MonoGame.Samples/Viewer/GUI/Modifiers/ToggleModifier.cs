using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Toggle Modifier that enables or disable a boolean value
    /// </summary>
    public class ToggleModifier : IModifier
    {
        private string Name;

        private Action<bool> OnChange;

        private bool Checked;


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
            Checked = defaultValue;
            OnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Draws the Toggle Modifier
        /// </summary>
        public void Draw()
        {
            if (ImGui.Checkbox(Name, ref Checked))
                OnChange.Invoke(Checked);
        }
    }
}
