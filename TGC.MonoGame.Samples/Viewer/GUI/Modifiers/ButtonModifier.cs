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
        private static Vector4 _disabledColor = new (0.2f);

        private string _name;

        private Action _onPress;

        private bool _enabled;


        /// <summary>
        ///     Creates a Button Modifier.
        /// </summary>
        /// <param name="text">The text to display in the button</param>
        /// <param name="onPress">The action to execute when the button is pressed</param>
        /// <param name="enabled">If the button is enabled</param>
        public ButtonModifier(string text, Action onPress, bool enabled)
        {
            _name = text;
            _onPress = onPress;
            _enabled = enabled;
        }

        /// <summary>
        ///     Creates a Button Modifier, enabled by default.
        /// </summary>
        /// <param name="text">The text to display in the button</param>
        /// <param name="onPress">The action to execute when the button is pressed</param>
        public ButtonModifier(string text, Action onPress) : this(text, onPress, true)
        {
        }

        /// <summary>
        ///     Draws the Button Modifier
        /// </summary>
        public void Draw()
        {
            // If enabled and pressed, invoke the callback
            if (_enabled)
            {
                if (ImGui.Button(_name, new Vector2(ImGui.CalcItemWidth(), ImGui.GetFrameHeight())))
                    _onPress.Invoke();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, _disabledColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, _disabledColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _disabledColor);
                ImGui.Button(_name, new Vector2(ImGui.CalcItemWidth(), ImGui.GetFrameHeight()));
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
            _enabled = enabled;
        }
    }
}