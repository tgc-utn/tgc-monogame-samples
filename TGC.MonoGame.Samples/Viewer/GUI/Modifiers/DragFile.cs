using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    public class DragFile : IModifier
    {
        const int ModalFlags = 0;

        private string Title;

        private bool oldVisibility;

        private int selection;

        private string currentPath;

        private bool currentPathIsDir;


        public string P;



        private string[] ItemsInScope;
        private int FirstFileIndex;


        public DragFile(string title)
        {
            Title = title;
        }

        public void Draw()
        {
            var result = Draw(true, out P);
        }

        private bool Draw(bool isVisible, out string outPath)
        {
            bool result = false;
            outPath = "";
            if (oldVisibility != isVisible)
            {
                oldVisibility = isVisible;
                //Visiblity has changed.

                if (isVisible)
                {
                    //Only run when the visibility state changes to visible.

                    //Reset the path to the initial path.
                    currentPath = Directory.GetCurrentDirectory();
                    currentPathIsDir = true;

                    //Update paths based on current path
                    PopulateItems();

                    //Make the modal visible.
                    //ImGui::OpenPopup(m_title);
                    ImGui.OpenPopup(Title);
                }

            }


            bool isOpen = true;
            if (ImGui.BeginPopupModal(Title, ref isOpen, ImGuiWindowFlags.Modal))
            {
                if (ImGui.ListBox("##", ref selection, ItemsInScope, ItemsInScope.Length, 10))
                {
                    //Update current path to the selected list item.
                    currentPath = ItemsInScope[selection];
                    currentPathIsDir = selection < FirstFileIndex;

                    //If the selection is a directory, repopulate the list with the contents of that directory.
                    if (currentPathIsDir)
                    {
                        PopulateItems();
                    }

                }


                //Auto resize text wrap to popup width.
                ImGui.PushItemWidth(-1f);
                ImGui.TextWrapped(currentPath + "        ");
                ImGui.PopItemWidth();

                ImGui.Spacing();
                ImGui.SameLine(ImGui.GetWindowWidth() - 30);

                // Make the "Select" button look / act disabled if the current selection is a directory.
                if (currentPathIsDir)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 1f));

                    ImGui.Button("Select");

                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Button("Select"))
                    {
                        ImGui.CloseCurrentPopup();

                        outPath = currentPath;
                        result = true;
                    }

                }

                ImGui.EndPopup();

            }
            return result;
        }

        private void PopulateItems()
        {
            //Update paths based on current path
            var directories = Directory.GetDirectories(currentPath);
            var files = Directory.GetFiles(currentPath);

            ItemsInScope = new string[directories.Length + files.Length];
            Array.Copy(directories, ItemsInScope, directories.Length);
            Array.Copy(files, 0, ItemsInScope, directories.Length, files.Length);
            FirstFileIndex = directories.Length;
        }

    }


    struct File
    {
        string alias;
        string path;
    }
}
