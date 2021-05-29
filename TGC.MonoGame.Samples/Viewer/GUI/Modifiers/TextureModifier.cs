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
        private static Vector4 BorderColor = new Vector4(0.3f, 0.3f, 0.3f, 0.7f);

        private Texture2D Texture;

        private IntPtr TextureReference;

        private string Name;

        /// <summary>
        ///     Creates a Texture Modifier using a name and a texture to bind.
        /// </summary>
        /// <param name="name">The name of the texture that will show in the GUI</param>
        /// <param name="texture">The texture to be bound to this object</param>
        public TextureModifier(string name, Texture2D texture)
        {
            Name = name;
            Texture = texture;
        }

        /// <summary>
        ///     Binds the texture that this modifier will show to ImGUI.
        /// </summary>
        /// <param name="renderer">The ImGUI Renderer that will bind the texture to ImGUI</param>
        public void Bind(ImGuiRenderer renderer)
        {
            TextureReference = renderer.BindTexture(Texture);
        }

        /// <summary>
        ///     Releases the texture contained by this modifier.
        /// </summary>
        /// <param name="renderer">The ImGUI Renderer that will unbind the texture from ImGUI</param>
        public void Unbind(ImGuiRenderer renderer)
        {
            renderer.UnbindTexture(TextureReference);
        }

        /// <summary>
        ///     Draws the Texture Modifier with a border
        /// </summary>
        public void Draw()
        {
            ImGui.Spacing();
            if (ImGui.CollapsingHeader(Name, ImGuiTreeNodeFlags.DefaultOpen))
                ImGui.Image(TextureReference,
                    // Size
                    new Vector2(ImGui.CalcItemWidth(), ImGui.CalcItemWidth()),
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One,
                    // Border Color
                    BorderColor);
        }
    }
}
