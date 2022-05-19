using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Viewer.GUI.ImGuiNET;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    public class ModifierController
    {
        private List<IModifier> Modifiers { get; set; }

        private List<TextureModifier> TextureModifiers { get; set; }


        /// <summary>
        ///     Constructs a ModifierController, in charge for managing and drawing Modifiers
        /// </summary>
        public ModifierController()
        {
            Modifiers = new List<IModifier>();
            TextureModifiers = new List<TextureModifier>();
        }

        /// <summary>
        ///     Adds a Button Modifier.
        /// </summary>
        /// <param name="text">The text to display in the button</param>
        /// <param name="onPress">The action to execute when the button is pressed</param>
        /// <param name="enabled">If the button is enabled</param>
        public void AddButton(string text, Action onPress, bool enabled = true)
        {
            Modifiers.Add(new ButtonModifier(text, onPress, enabled));
        }

        /// <summary>
        ///     Adds a Color Modifier with a given name, action on change and a default Color.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">An action to be called when the Color changes</param>
        /// <param name="defaultColor">The Color that the Color Modifier starts with</param>
        public void AddColor(string name, Action<Color> onChange, Color defaultColor)
        {
            Modifiers.Add(new ColorModifier(name, onChange, defaultColor));
        }

        /// <summary>
        ///     Adds a Color Modifier with a given name, an <see cref="EffectParameter"/>, and a default Color.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Color as value</param>
        /// <param name="defaultColor">The Color that the Color Modifier starts with</param>
        public void AddColor(string name, EffectParameter effectParameter, Color defaultColor)
        {
            Modifiers.Add(new ColorModifier(name, effectParameter, defaultColor));
        } 

        /// <summary>
        ///     Adds a Float Modifier with a given name, an action on change and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public void AddFloat(string name, Action<float> onChange, float defaultValue)
        {
            Modifiers.Add(new FloatModifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Float Modifier with a given name, an action on change, a default value, and a minimum and maximum values for the float.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        /// <param name="min">The minimum value for the modifier</param>
        /// <param name="max">The maximum value for the modifier</param>
        public void AddFloat(string name, Action<float> onChange, float defaultValue, float min, float max)
        {
            Modifiers.Add(new FloatModifier(name, onChange, defaultValue, min, max));
        }

        /// <summary>
        ///     Adds a Float Modifier with a given name, an <see cref="EffectParameter"/> and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public void AddFloat(string name, EffectParameter effectParameter, float defaultValue)
        {
            Modifiers.Add(new FloatModifier(name, effectParameter, defaultValue));
        }

        /// <summary>
        ///     Adds a Float Modifier with a given name, an <see cref="EffectParameter"/>, a default value, and a minimum and maximum values for the float.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        /// <param name="min">The minimum value for the modifier</param>
        /// <param name="max">The maximum value for the modifier</param>
        public void AddFloat(string name, EffectParameter effectParameter, float defaultValue, float min, float max)
        {
            Modifiers.Add(new FloatModifier(name, effectParameter, defaultValue, min, max));
        }



        /// <summary>
        ///     Adds an Options Modifiers with a name and option change listener.
        ///     This way to construct an Options Modifier uses the enum names as option names.
        ///     The default option is the first one on the enum type.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public void AddOptions<EnumType>(string name, Action<EnumType> onChange) where EnumType : Enum
        {
            Modifiers.Add(new OptionsModifier<EnumType>(name, onChange));
        }

        /// <summary>
        ///     Adds an Options Modifiers with a name, default option value and option change listener.
        ///     This way to construct an Options Modifier uses the enum names as option names.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="defaultValue">The default option value</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public void AddOptions<EnumType>(string name, EnumType defaultValue, Action<EnumType> onChange) where EnumType : Enum
        {
            Modifiers.Add(new OptionsModifier<EnumType>(name, defaultValue, onChange));
        }

        /// <summary>
        ///     Adds an Options Modifiers with a name, option names, default option value and option change listener.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="optionNames">The sorted option names array</param>
        /// <param name="defaultValue">The default option value</param>
        /// <param name="onChange">An action to be called when the option value changes</param>
        public void AddOptions<EnumType>(string name, string[] optionNames, EnumType defaultValue, Action<EnumType> onChange) where EnumType : Enum
        {
            Modifiers.Add(new OptionsModifier<EnumType>(name, optionNames, defaultValue, onChange));
        }


        /// <summary>
        ///     Adds a Texture Modifier using a name and a texture to bind.
        /// </summary>
        /// <param name="name">The name of the texture that will show in the GUI</param>
        /// <param name="texture">The texture to be bound to this object</param>
        public void AddTexture(string name, Texture2D texture)
        {
            var textureModifier = new TextureModifier(name, texture);
            Modifiers.Add(textureModifier);
            TextureModifiers.Add(textureModifier);
        }

        /// <summary>
        ///     Adds a Toggle Modifier with a given name, action and default value
        /// </summary>
        /// <param name="name">The name of the Toggle Modifier</param>
        /// <param name="onChange">An action to be called when the value of the modifier changes</param>
        /// <param name="defaultValue">The default value that this modifier will have</param>
        public void AddToggle(string name, Action<bool> onChange, bool defaultValue)
        {
            Modifiers.Add(new ToggleModifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Vector2 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector2 changes</param>
        public void AddVector(string name, Action<Vector2> onChange) 
        {
            Modifiers.Add(new Vector2Modifier(name, onChange));
        }

        /// <summary>
        ///     Adds a Vector2 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Vector3 as value</param>
        /// <param name="defaultValue">The default value that this modifier will have</param>
        public void AddVector(string name, EffectParameter effectParameter, Vector3 defaultValue)
        {
            Modifiers.Add(new Vector3Modifier(name, effectParameter, defaultValue));
        }


        /// <summary>
        ///     Creates a Vector3 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        /// <param name="defaultValue">The Vector3 default value</param>
        public void AddVector(string name, Action<Vector2> onChange, Vector2 defaultValue)
        {
            Modifiers.Add(new Vector2Modifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Vector3 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        public void AddVector(string name, Action<Vector3> onChange)
        {
            Modifiers.Add(new Vector3Modifier(name, onChange));
        }

        /// <summary>
        ///     Creates a Vector3 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        /// <param name="defaultValue">The Vector3 default value</param>
        public void AddVector(string name, Action<Vector3> onChange, Vector3 defaultValue)
        {
            Modifiers.Add(new Vector3Modifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Vector4 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector4 changes</param>
        public void AddVector(string name, Action<Vector4> onChange)
        {
            Modifiers.Add(new Vector4Modifier(name, onChange));
        }

        /// <summary>
        ///     Creates a Vector4 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector4 changes</param>
        /// <param name="defaultValue">The Vector4 default value</param>
        public void AddVector(string name, Action<Vector4> onChange, Vector4 defaultValue)
        {
            Modifiers.Add(new Vector4Modifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Clears the previously built modifiers.
        /// </summary>
        public void Clear()
        {
            Modifiers.Clear();
            TextureModifiers.Clear();
        }

        /// <summary>
        ///     Binds the Modifiers that need to be bound to the ImGuiRenderer.
        /// </summary>
        /// <param name="renderer">The ImGuiRenderer to bind the modifiers to</param>
        public void Bind(ImGuiRenderer renderer)
        {
            var count = TextureModifiers.Count;
            for (var index = 0; index < count; index++)
                TextureModifiers[index].Bind(renderer);
        }

        /// <summary>
        ///     Unbinds the Modifiers that were previously bound to the ImGuiRenderer.
        /// </summary>
        /// <param name="renderer">The ImGuiRenderer to unbind the modifiers</param>
        public void Unbind(ImGuiRenderer renderer)
        {
            var count = TextureModifiers.Count;
            for (var index = 0; index < count; index++)
                TextureModifiers[index].Unbind(renderer);
        }

        /// <summary>
        ///     Draws the modifiers
        /// </summary>
        public void Draw()
        {
            ImGui.Spacing();

            var count = Modifiers.Count;
            if (count > 0 && ImGui.CollapsingHeader("Modifiers", ImGuiTreeNodeFlags.DefaultOpen))
                for (var index = 0; index < count; index++)
                    Modifiers[index].Draw();
        }
    }
}
