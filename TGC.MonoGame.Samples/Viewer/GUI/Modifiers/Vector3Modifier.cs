using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Numerics;
using MonoGameVector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Vector3 Modifier that allows for changing a Vector3 value in real time
    /// </summary>
    public class Vector3Modifier : IModifier
    {
        private Vector3 _vectorValue;

        private string _name;

        private Action<MonoGameVector3> _onChange;

        /// <summary>
        ///     Creates a Vector3 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        public Vector3Modifier(string name, Action<MonoGameVector3> onChange)
        {
            _name = name;
            _onChange += onChange;
        }

        /// <summary>
        ///     Creates a Vector3 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        /// <param name="defaultValue">The Vector3 default value</param>
        public Vector3Modifier(string name, Action<MonoGameVector3> onChange, MonoGameVector3 defaultValue)
            : this(name, onChange)
        {
            _vectorValue = Convert(defaultValue);
            _onChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Creates a Vector3 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter" /> that will recieve the Vector3 as value</param>
        /// <param name="defaultValue">The Vector3 default value</param>
        public Vector3Modifier(string name, EffectParameter effectParameter, MonoGameVector3 defaultValue)
        : this(name, (vector) => effectParameter.SetValue(vector))
        {
            _vectorValue = Convert(defaultValue);
            _onChange.Invoke(defaultValue);
        }

        /// <summary>
        ///     Converts a MonoGame Vector3 to a Microsoft Vector3
        /// </summary>
        /// <param name="vector">A MonoGame Vector3 to convert</param>
        /// <returns>The Vector3 converted to the Microsoft format</returns>
        private Vector3 Convert(MonoGameVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        ///     Converts a Microsoft Vector3 to a MonoGame Vector3
        /// </summary>
        /// <param name="vector">A Microsoft Vector3 to convert</param>
        /// <returns>The Vector3 converted to the MonoGame format</returns>
        private MonoGameVector3 Convert(Vector3 vector)
        {
            return new MonoGameVector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        ///     Draws the Vector3 Modifier.
        /// </summary>
        public void Draw()
        {
            var valueChanged = ImGui.DragFloat3(_name, ref _vectorValue);
            if (valueChanged)
                _onChange.Invoke(Convert(_vectorValue));
        }
    }
}
