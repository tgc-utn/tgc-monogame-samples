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
        /// <param name="game">The game where the explorer is going to be draw.</param>
        public TGCViewerModel(TGCViewer game)
        {
            Game = game;
            ActiveSampleTreeNode = -1;
        }

        /// <summary>
        ///     The viewer where the samples are going to be shown.
        /// </summary>
        private TGCViewer Game { get; }

        /// <summary>
        ///     Graphical user interface.
        /// </summary>
        public ImGuiRenderer ImGuiRenderer { get; set; }

        /// <summary>
        ///     Samples sorted by category.
        /// </summary>
        public SortedList<string, List<TGCSample>> SamplesByCategory { get; set; }

        /// <summary>
        ///     Samples available to view.
        /// </summary>
        public Dictionary<string, TGCSample> SamplesByName { get; set; }

        /// <summary>
        ///     Samples that have already been loaded.
        /// </summary>
        public Dictionary<string, TGCSample> AlreadyLoadedSamples { get; set; }

        /// <summary>
        ///     The active sample.
        /// </summary>
        private TGCSample ActiveSample { get; set; }

        /// <summary>
        ///     The category of the selected sample in the tree.
        /// </summary>
        private int ActiveSampleCategoryIndex { get; set; }

        /// <summary>
        ///     The active sample in the tree.
        /// </summary>
        private int ActiveSampleTreeNode { get; set; }

        /// <summary>
        ///     Initialize imgui to be able to build the menu.
        /// </summary>
        public void LoadImgGUI()
        {
            ImGuiRenderer = new ImGuiRenderer(Game);
            ImGuiRenderer.RebuildFontAtlas();
        }

        /// <summary>
        ///     Loads the sample tree dynamically and groups them by category.
        /// </summary>
        public void LoadTreeSamples()
        {
            SamplesByCategory = new SortedList<string, List<TGCSample>>();
            SamplesByName = new Dictionary<string, TGCSample>();
            AlreadyLoadedSamples = new Dictionary<string, TGCSample>();

            var baseType = typeof(TGCSample);

            foreach (var type in baseType.Assembly.GetTypes())
            {
                if (type.BaseType != baseType || !type.IsClass || !type.IsPublic || type.IsAbstract) continue;
                try
                {
                    var sample = (TGCSample) Activator.CreateInstance(type, Game);
                    sample.Visible = false;
                    sample.Enabled = false;

                    if (!string.IsNullOrEmpty(sample.Category))
                    {
                        if (SamplesByCategory.ContainsKey(sample.Category))
                            SamplesByCategory[sample.Category].Add(sample);
                        else
                            SamplesByCategory.Add(sample.Category, new List<TGCSample> {sample});
                    }

                    SamplesByName.Add(sample.Name, sample);
                }
                catch
                {
                    Console.WriteLine("Could not load sample type: " + type);
                }
            }
        }

        /// <summary>
        ///     Load the welcome sample
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
            if (ActiveSample != null)
            {
                ActiveSample.UnloadSampleContent();
                ActiveSample.Enabled = false;
                ActiveSample.Visible = false;
            }

            var newSample = SamplesByName[sampleName];
            newSample.Visible = true;
            newSample.Enabled = true;
            ActiveSample = newSample;

            if (!Game.Components.Contains(newSample))
            {
                Game.Components.Add(newSample);
            }
            else
            {
                newSample.Initialize();
                newSample.ReloadContent();
            }
        }

        /// <summary>
        ///     Draw the sample explorer.
        /// </summary>
        /// <param name="gameTime">Holds the time state of a <see cref="Game" />.</param>
        public void DrawSampleExplorer(GameTime gameTime)
        {
            // Call BeforeLayout first to set things up
            ImGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout(SamplesByCategory);

            // Call AfterLayout now to finish up and draw all the things
            ImGuiRenderer.AfterLayout();
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
                    if (ImGui.MenuItem("Close", "Ctrl+W")) Game.Exit();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Full screen")) Game.Graphics.ToggleFullScreen();

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
                        /* Do stuff */
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (ImGui.CollapsingHeader("TGCLogoSample information", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("Name: " + ActiveSample.Name);
                ImGui.TextWrapped("Description: " + ActiveSample.Description);
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

                            if (ActiveSampleCategoryIndex == i && ActiveSampleTreeNode == j)
                                nodeFlags |= ImGuiTreeNodeFlags.Selected;

                            ImGui.TreeNodeEx(new IntPtr(j), nodeFlags, sample.Name);
                            if (ImGui.IsItemClicked())
                            {
                                ActiveSampleCategoryIndex = i;
                                ActiveSampleTreeNode = j;
                                LoadSample(sample.Name);
                            }

                            j++;
                        }

                        ImGui.TreePop();
                    }

                    i++;
                }
            }

            ImGui.End();
        }

        /// <summary>
        ///     Unloads every sample.
        /// </summary>
        public void Dispose()
        {
            foreach (var sample in SamplesByName)
                if (Game.Components.Contains(sample.Value))
                    sample.Value.Dispose();
        }
    }
}