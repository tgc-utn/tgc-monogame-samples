using ImGuiNET;
using System;
using System.Numerics;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Vector4 Modifier that allows for changing a Vector4 value in real time
    /// </summary>
    public class Vector4Modifier : IModifier
    {
        private Vector4 _vectorValue;

        /// <summary>
        ///     Creates a Vector4 Modifier with a given name.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        public Vector4Modifier(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Creates a Vector4 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="baseOnChange">The action that will be called when the Vector4 changes</param>
        public Vector4Modifier(string name, Action<Vector4> baseOnChange) : this(name)
        {
            OnChange += baseOnChange;
        }

        private string Name { get; }

        /// <summary>
        ///     Draws the Vector4 Modifier.
        /// </summary>
        public void Draw()
        {
            var valueChanged = ImGui.DragFloat4(Name, ref _vectorValue);
            if (valueChanged)
                OnChange.Invoke(_vectorValue);
        }

        private event Action<Vector4> OnChange;
    }
}