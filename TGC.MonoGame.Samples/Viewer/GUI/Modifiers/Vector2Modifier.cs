using ImGuiNET;
using System;
using System.Numerics;
using MonoGameVector2 = Microsoft.Xna.Framework.Vector2;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Vector2 Modifier that allows for changing a Vector2 value in real time
    /// </summary>
    public class Vector2Modifier : IModifier
    {
        private string Name;

        private Vector2 VectorValue;

        private event Action<MonoGameVector2> OnChange;

        /// <summary>
        ///     Creates a Vector2 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector2 changes</param>
        public Vector2Modifier(string name, Action<MonoGameVector2> onChange)
        {
            Name = name;
            OnChange += onChange;
        }

        /// <summary>
        ///     Creates a Vector2 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector2 changes</param>
        /// <param name="defaultValue">The Vector2 default value</param>
        public Vector2Modifier(string name, Action<MonoGameVector2> onChange, MonoGameVector2 defaultValue)
            : this(name, onChange)
        {
            VectorValue = Convert(defaultValue);
            OnChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Converts a MonoGame Vector2 to a Microsoft Vector2
        /// </summary>
        /// <param name="vector">A MonoGame Vector2 to convert</param>
        /// <returns>The Vector2 converted to the Microsoft format</returns>
        private Vector2 Convert(MonoGameVector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <summary>
        ///     Converts a Microsoft Vector2 to a MonoGame Vector2
        /// </summary>
        /// <param name="vector">A Microsoft Vector2 to convert</param>
        /// <returns>The Vector2 converted to the MonoGame format</returns>
        private MonoGameVector2 Convert(Vector2 vector)
        {
            return new MonoGameVector2(vector.X, vector.Y);
        }

        /// <summary>
        ///     Draws the Vector2 Modifier.
        /// </summary>
        public void Draw()
        {
            bool valueChanged = ImGui.DragFloat2(Name, ref VectorValue);
            if (valueChanged)
                OnChange.Invoke(Convert(VectorValue));
        }

    }
}
