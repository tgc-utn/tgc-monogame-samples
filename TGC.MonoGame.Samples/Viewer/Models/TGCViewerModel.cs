using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Samples;
using TGC.MonoGame.Samples.Viewer.GUI.ImGuiNET;
using NumericVector2 = System.Numerics.Vector2;

namespace TGC.MonoGame.Samples.Viewer.Models
{
    /// <summary>
    ///     The model has the logic for creating the sample explorer and also load them.
    /// </summary>
    public class TGCViewerModel
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="game">The game where the explorer is going to be drawn.</param>
        public TGCViewerModel(TGCViewer game)
        {
            _game = game;
            _activeSampleTreeNode = -1;
        }

        /// <summary>
        ///     The viewer where the samples are going to be shown.
        /// </summary>
        private readonly TGCViewer _game;

        /// <summary>
        ///     Graphical user interface.
        /// </summary>
        private ImGuiRenderer _imGuiRenderer;

        /// <summary>
        ///     Samples sorted by category.
        /// </summary>
        private SortedList<string, List<TGCSample>> _samplesByCategory;

        /// <summary>
        ///     Samples available to view.
        /// </summary>
        private Dictionary<string, TGCSample> _samplesByName;

        /// <summary>
        ///     Samples that have already been loaded.
        /// </summary>
        public Dictionary<string, TGCSample> AlreadyLoadedSamples { get; set; }

        /// <summary>
        ///     The active sample.
        /// </summary>
        private TGCSample _activeSample;

        /// <summary>
        ///     The category of the selected sample in the tree.
        /// </summary>
        private int _activeSampleCategoryIndex;

        /// <summary>
        ///     The active sample in the tree.
        /// </summary>
        private int _activeSampleTreeNode;

        /// <summary>
        ///     If the About modal in visible or not.
        /// </summary>
        private bool _aboutVisible;

        /// <summary>
        ///     Initialize imgui to be able to build the menu.
        /// </summary>
        public void LoadImgGUI()
        {
            _imGuiRenderer = new ImGuiRenderer(_game);
            _imGuiRenderer.RebuildFontAtlas();
        }

        /// <summary>
        ///     Loads the sample tree dynamically and groups them by category.
        /// </summary>
        public void LoadTreeSamples()
        {
            _samplesByCategory = new SortedList<string, List<TGCSample>>();
            _samplesByName = new Dictionary<string, TGCSample>();
            AlreadyLoadedSamples = new Dictionary<string, TGCSample>();

            var baseType = typeof(TGCSample);

            foreach (var type in baseType.Assembly.GetTypes())
            {
                if (type.BaseType != baseType || !type.IsClass || !type.IsPublic || type.IsAbstract)
                {
                    continue;
                }
                
                try
                {
                    var sample = (TGCSample) Activator.CreateInstance(type, _game);
                    sample.Visible = false;
                    sample.Enabled = false;

                    if (!string.IsNullOrEmpty(sample.Category))
                    {
                        if (_samplesByCategory.ContainsKey(sample.Category))
                        {
                            _samplesByCategory[sample.Category].Add(sample);
                        }
                        else
                        {
                            _samplesByCategory.Add(sample.Category, new List<TGCSample> {sample});
                        }
                    }

                    _samplesByName.Add(sample.Name, sample);
                }
                catch
                {
                    Console.WriteLine("Could not load sample type: " + type);
                }
            }
        }

        /// <summary>
        ///     Load the welcome sample.
        /// </summary>
        public void LoadWelcomeSample()
        {
            LoadSample(typeof(TGCLogoSample).Name);
        }

        /// <summary>
        ///     Enable the selected sample and disables the others.
        /// </summary>
        /// <param name="sampleName">The name of the sample to load.</param>
        public void LoadSample(string sampleName)
        {
            if (_activeSample != null)
            {
                // Unbind any Texture modifiers from ImGUI
                UnbindModifiers();
                _activeSample.UnloadSampleContent();
                _activeSample.Enabled = false;
                _activeSample.Visible = false;
            }

            var newSample = _samplesByName[sampleName];
            newSample.Visible = true;
            newSample.Enabled = true;
            _activeSample = newSample;

            newSample.Prepare();

            if (!_game.Components.Contains(newSample))
            {
                _game.Components.Add(newSample);
            }
            else
            {
                newSample.Initialize();
                newSample.ReloadContent();
            }

            // Bind the new Texture modifiers if any
            BindModifiers();
        }

        /// <summary>
        ///     Binds the Modifiers from the current sample to be used by ImGUI.
        /// </summary>
        private void BindModifiers()
        {
            _activeSample.BindModifiers(_imGuiRenderer);
        }

        /// <summary>
        ///     Releases the Modifier bindings from the current sample.
        /// </summary>
        private void UnbindModifiers()
        {
            _activeSample.UnbindModifiers(_imGuiRenderer);
        }

        /// <summary>
        ///     Draw the sample explorer.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="_game" />.</param>
        public void DrawSampleExplorer(GameTime gameTime)
        {
            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout(_samplesByCategory);

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }

        /// <summary>
        ///     Create the GUI components.
        ///     Example at https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp
        /// </summary>
        /// <param name="samplesByCategory">Sample ordered list to load into sample tree.</param>
        private void ImGuiLayout(SortedList<string, List<TGCSample>> samplesByCategory)
        {
            ImGui.SetNextWindowPos(new NumericVector2(0, 0));
            ImGui.SetNextWindowSize(new NumericVector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 6f,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100));

            ImGui.Begin("TGC samples explorer", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Close", "Ctrl+W"))
                    {
                        _game.Exit();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Full screen"))
                    {
                        _game.Graphics.ToggleFullScreen();
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("FPS"))
                    {
                        /* Do stuff */
                    }

                    if (ImGui.MenuItem("Cartesian axes"))
                    {
                        /* Do stuff */
                    }

                    if (ImGui.MenuItem("Fixed render loop"))
                    {
                        /* Do stuff */
                    }

                    if (ImGui.MenuItem("Wireframe"))
                    {
                        /* Do stuff */
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("Reset view setting"))
                    {
                        /* Do stuff */
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Tools"))
                {
                    if (ImGui.MenuItem("Settings"))
                    {
                        /* Do stuff */
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem("Documentation"))
                    {
                        /* Do stuff */
                    }

                    if (ImGui.MenuItem("About"))
                    {
                        _aboutVisible = !_aboutVisible;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (ImGui.CollapsingHeader("TGCLogoSample information", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("Name: " + _activeSample.Name);
                ImGui.TextWrapped("Description: " + _activeSample.Description);
                ImGui.TextWrapped(
                    $"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)");
            }

            if (ImGui.CollapsingHeader("Samples", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var i = 0;
                foreach (var entry in samplesByCategory)
                {
                    if (ImGui.TreeNode(entry.Key))
                    {
                        var j = 0;
                        foreach (var sample in entry.Value)
                        {
                            var nodeFlags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen |
                                            ImGuiTreeNodeFlags.Bullet;

                            if (_activeSampleCategoryIndex == i && _activeSampleTreeNode == j)
                            {
                                nodeFlags |= ImGuiTreeNodeFlags.Selected;
                            }

                            ImGui.TreeNodeEx(new IntPtr(j), nodeFlags, sample.Name);
                            if (ImGui.IsItemClicked())
                            {
                                _activeSampleCategoryIndex = i;
                                _activeSampleTreeNode = j;
                                LoadSample(sample.Name);
                            }

                            j++;
                        }

                        ImGui.TreePop();
                    }

                    i++;
                }
            }

            if (_aboutVisible)
            {
                ShowAboutWindow();
            }

            DrawModifiers();

            ImGui.End();
        }

        /// <summary>
        ///     Draws the Modifiers for the current sample
        /// </summary>
        private void DrawModifiers()
        {
            _activeSample.ModifierController.Draw();
        }

        /// <summary>
        ///     About Window / ShowAboutWindow()
        /// </summary>
        private void ShowAboutWindow()
        {
            ImGui.Begin("About",
                ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.Text("MonoGame samples made by TGC UTN Group.");
            ImGui.Text("With <3 from Argentine.");
            ImGui.Spacing();

            if (ImGui.Button("Close"))
            {
                _aboutVisible = !_aboutVisible;
            }

            ImGui.End();
        }

        /// <summary>
        ///     Unloads every sample.
        /// </summary>
        public void Dispose()
        {
            foreach (var sample in _samplesByName)
            {
                if (_game.Components.Contains(sample.Value))
                {
                    sample.Value.Dispose();
                }
            }
        }
    }
}