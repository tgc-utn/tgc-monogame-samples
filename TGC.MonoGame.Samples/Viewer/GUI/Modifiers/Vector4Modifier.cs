using ImGuiNET;
using System;
using System.Numerics;
using MonoGameVector4 = Microsoft.Xna.Framework.Vector4;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Vector4 Modifier that allows for changing a Vector4 value in real time
    /// </summary>
    public class Vector4Modifier : IModifier
    {
        private string Name { get; set; }

        private Vector4 VectorValue { get; set; }

        private event Action<MonoGameVector4> OnChange { get; set; }

        /// <summary>
        ///     Creates a Vector4 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector4 changes</param>
        public Vector4Modifier(string name, Action<MonoGameVector4> onChange)
        {
            Name = name;
            OnChange += onChange;
        }

        /// <summary>
        ///     Creates a Vector4 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector4 changes</param>
        /// <param name="defaultValue">The Vector4 default value</param>
        public Vector4Modifier(string name, Action<MonoGameVector4> onChange, MonoGameVector4 defaultValue)
            : this(name, onChange)
        {
            VectorValue = Convert(defaultValue);
            OnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Converts a MonoGame Vector4 to a Microsoft Vector4
        /// </summary>
        /// <param name="vector">A MonoGame Vector4 to convert</param>
        /// <returns>The Vector4 converted to the Microsoft format</returns>
        private Vector4 Convert(MonoGameVector4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        ///     Converts a Microsoft Vector4 to a MonoGame Vector4
        /// </summary>
        /// <param name="vector">A Microsoft Vector4 to convert</param>
        /// <returns>The Vector4 converted to the MonoGame format</returns>
        private MonoGameVector4 Convert(Vector4 vector)
        {
            return new MonoGameVector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        ///     Draws the Vector4 Modifier.
        /// </summary>
        public void Draw()
        {
            var valueChanged = ImGui.DragFloat4(Name, ref VectorValue);
            if (valueChanged)
                OnChange.Invoke(Convert(VectorValue));
        }

    }
}
