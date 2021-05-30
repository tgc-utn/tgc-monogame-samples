using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    class ModifierController
    {
        private List<IModifier> Modifiers;


        /// <summary>
        ///     Adds a Button Modifier.
        /// </summary>
        /// <param name="text">The text to display in the button</param>
        /// <param name="onPress">The action to execute when the button is pressed</param>
        /// <param name="enabled">If the button is enabled</param>
        public void AddButtonModifier(string text, Action onPress, bool enabled = true)
        {
            Modifiers.Add(new ButtonModifier(text, onPress, enabled));
        }

        /// <summary>
        ///     Adds a Color Modifier with a given name, action on change and a default Color.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">An action to be called when the Color changes</param>
        /// <param name="defaultColor">The Color that the Color Modifier starts with</param>
        public void AddColorModifier(string name, Action<Color> onChange, Color defaultColor)
        {
            Modifiers.Add(new ColorModifier(name, onChange, defaultColor));
        }

        /// <summary>
        ///     Adds a Color Modifier with a given name, an <see cref="EffectParameter"/>, and a default Color.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Color as value</param>
        /// <param name="defaultColor">The Color that the Color Modifier starts with</param>
        public void AddColorModifier(string name, EffectParameter effectParameter, Color defaultColor)
        {
            Modifiers.Add(new ColorModifier(name, effectParameter, defaultColor));
        } 

        /// <summary>
        ///     Adds a Float Modifier with a given name, an action on change and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="onChange">The action to be called when the value changes</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public void AddFloatModifier(string name, Action<float> onChange, float defaultValue)
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
        public void AddFloatModifier(string name, Action<float> onChange, float defaultValue, float min, float max)
        {
            Modifiers.Add(new FloatModifier(name, onChange, defaultValue, min, max));
        }

        /// <summary>
        ///     Adds a Float Modifier with a given name, an <see cref="EffectParameter"/> and a default value.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="effectParameter">An <see cref="EffectParameter"/> that will recieve the Float as value</param>
        /// <param name="defaultValue">The default value for the modifier</param>
        public void AddFloatModifier(string name, EffectParameter effectParameter, float defaultValue)
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
        public void AddFloatModifier(string name, EffectParameter effectParameter, float defaultValue, float min, float max)
        {
            Modifiers.Add(new FloatModifier(name, effectParameter, defaultValue, min, max));
        }


        /// <summary>
        ///     Adds an Options Modifier with a given name, options, default option and an action.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="options">A list of options to be displayed and selected</param>
        /// <param name="defaultOption">The index of the option that is selected by default</param>
        /// <param name="onChange">An action to be called when the selected option changes</param>
        public void AddOptionsModifier(string name, List<string> options, int defaultOption, Action<int, string> onChange)
        {
            Modifiers.Add(new OptionsModifier(name, options, defaultOption, onChange));
        }

        /// <summary>
        ///     Adds an Options Modifier with a given name, options, default option and an action.
        /// </summary>
        /// <param name="name">The name of the modifier that will show on the GUI</param>
        /// <param name="options">An array of options to be displayed and selected</param>
        /// <param name="defaultOption">The index of the option that is selected by default</param>
        /// <param name="onChange">An action to be called when the selected option changes</param>
        public void AddOptionsModifier(string name, string[] options, int defaultOption, Action<int, string> onChange)
        {
            Modifiers.Add(new OptionsModifier(name, options, defaultOption, onChange));
        }



        /// <summary>
        ///     Adds a Texture Modifier using a name and a texture to bind.
        /// </summary>
        /// <param name="name">The name of the texture that will show in the GUI</param>
        /// <param name="texture">The texture to be bound to this object</param>
        public void AddTextureModifier(string name, Texture2D texture)
        {
            Modifiers.Add(new TextureModifier(name, texture));
        }

        /// <summary>
        ///     Adds a Toggle Modifier with a given name, action and default value
        /// </summary>
        /// <param name="name">The name of the Toggle Modifier</param>
        /// <param name="onChange">An action to be called when the value of the modifier changes</param>
        /// <param name="defaultValue">The default value that this modifier will have</param>
        public void AddToggleModifier(string name, Action<bool> onChange, bool defaultValue)
        {
            Modifiers.Add(new ToggleModifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Vector2 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector2 changes</param>
        public void AddVectorModifier(string name, Action<Vector2> onChange) 
        {
            Modifiers.Add(new Vector2Modifier(name, onChange));
        }

        /// <summary>
        ///     Creates a Vector3 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        /// <param name="defaultValue">The Vector3 default value</param>
        public void AddVectorModifier(string name, Action<Vector2> onChange, Vector2 defaultValue)
        {
            Modifiers.Add(new Vector2Modifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Vector3 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        public void AddVectorModifier(string name, Action<Vector3> onChange)
        {
            Modifiers.Add(new Vector3Modifier(name, onChange));
        }

        /// <summary>
        ///     Creates a Vector3 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector3 changes</param>
        /// <param name="defaultValue">The Vector3 default value</param>
        public void AddVectorModifier(string name, Action<Vector3> onChange, Vector3 defaultValue)
        {
            Modifiers.Add(new Vector3Modifier(name, onChange, defaultValue));
        }

        /// <summary>
        ///     Adds a Vector4 Modifier with a given name and action.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector4 changes</param>
        public void AddVectorModifier(string name, Action<Vector4> onChange)
        {
            Modifiers.Add(new Vector4Modifier(name, onChange));
        }

        /// <summary>
        ///     Creates a Vector4 Modifier with a given name, action and default value.
        /// </summary>
        /// <param name="name">The name that will show in the GUI</param>
        /// <param name="onChange">The action that will be called when the Vector4 changes</param>
        /// <param name="defaultValue">The Vector4 default value</param>
        public void AddVector4Modifier(string name, Action<Vector4> onChange, Vector4 defaultValue)
        {
            Modifiers.Add(new Vector4Modifier(name, onChange, defaultValue));
        }




        public void Draw()
        {
            for (int index = 0; index < Modifiers.Count; index++)
                Modifiers[index].Draw();
        }
    }
}
