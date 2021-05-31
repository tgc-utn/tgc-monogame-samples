using ImGuiNET;
using System;
using System.Numerics;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Button Modifier that calls an action when is pressed
    /// </summary>
    public class ButtonModifier : IModifier
    {
        /// <summary>
        ///     Creates a Button Modifier.
        /// </summary>
        /// <param name="text">The text to display in the button</param>
        /// <param name="onPress">The action to execute when the button is pressed</param>
        /// <param name="enabled">If the button is enabled</param>
        public ButtonModifier(string text, Action onPress, bool enabled)
        {
            Name = text;
            OnPress = onPress;
            Enabled = enabled;
        }

        /// <summary>
        ///     Creates a Button Modifier, enabled by default.
        /// </summary>
        /// <param name="text">The text to display in the button</param>
        /// <param name="onPress">The action to execute when the button is pressed</param>
        public ButtonModifier(string text, Action onPress) : this(text, onPress, true)
        {
        }

        private static Vector4 DisabledColor { get; set; } = new Vector4(0.2f);

        private string Name { get; }

        private Action OnPress { get; set; }

        private bool Enabled { get; set; }

        /// <summary>
        ///     Draws the Button Modifier
        /// </summary>
        public void Draw()
        {
            // If enabled and pressed, invoke the callback
            if (Enabled)
            {
                if (ImGui.Button(Name, new Vector2(ImGui.CalcItemWidth(), ImGui.GetFrameHeight())))
                    OnPress.Invoke();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, DisabledColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, DisabledColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, DisabledColor);
                ImGui.Button(Name, new Vector2(ImGui.CalcItemWidth(), ImGui.GetFrameHeight()));
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
            }
        }

        /// <summary>
        ///     Sets the enabled state of the button
        /// </summary>
        /// <param name="enabled">A boolean indicating if the button is enabled or not</param>
        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }
    }
}