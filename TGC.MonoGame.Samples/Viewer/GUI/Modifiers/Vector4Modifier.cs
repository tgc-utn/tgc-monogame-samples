using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    public class Vector4Modifier : IModifier
    {
        private string Name;

        private Vector4 VectorValue;

        private event Action<Vector4> OnChange;

        public Vector4Modifier(string name)
        {
            Name = name;
        }

        public Vector4Modifier(string name, Action<Vector4> baseOnChange) : this(name)
        {
            OnChange += baseOnChange;
        }

        public void Draw()
        {
            bool valueChanged = ImGui.DragFloat4(Name, ref VectorValue);
            if (valueChanged)
                OnChange.Invoke(VectorValue);
        }

    }
}
