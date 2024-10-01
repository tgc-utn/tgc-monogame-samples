using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Numerics;
using TGC.MonoGame.Samples.Viewer.GUI.ImGuiNET;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    /// <summary>
    ///     A Texture Modifier that displays a bound texture
    /// </summary>
    internal class TextureModifier : IModifier
    {
        private static readonly Vector4 BorderColor = new (0.3f, 0.3f, 0.3f, 0.7f);

        private readonly string _name;

        private readonly Texture2D _texture;

        private IntPtr _textureReference;

        /// <summary>
        ///     Creates a Texture Modifier using a name and a texture to bind.
        /// </summary>
        /// <param name="name">The name of the texture that will show in the GUI</param>
        /// <param name="texture">The texture to be bound to this object</param>
        public TextureModifier(string name, Texture2D texture)
        {
            _name = name;
            _texture = texture;
        }

        /// <summary>
        ///     Draws the Texture Modifier with a border
        /// </summary>
        public void Draw()
        {
            ImGui.Spacing();
            if (ImGui.CollapsingHeader(_name, ImGuiTreeNodeFlags.DefaultOpen))
                ImGui.Image(_textureReference,
                    // Size
                    new Vector2(ImGui.CalcItemWidth(), ImGui.CalcItemWidth()),
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One,
                    // Border Color
                    BorderColor);
        }

        /// <summary>
        ///     Binds the texture that this modifier will show to ImGUI.
        /// </summary>
        /// <param name="renderer">The ImGUI Renderer that will bind the texture to ImGUI</param>
        public void Bind(ImGuiRenderer renderer)
        {
            _textureReference = renderer.BindTexture(_texture);
        }

        /// <summary>
        ///     Releases the texture contained by this modifier.
        /// </summary>
        /// <param name="renderer">The ImGUI Renderer that will unbind the texture from ImGUI</param>
        public void Unbind(ImGuiRenderer renderer)
        {
            renderer.UnbindTexture(_textureReference);
        }
    }
}