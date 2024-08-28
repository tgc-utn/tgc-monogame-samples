using ImGuiNET;
using System;
using System.IO;
using System.Numerics;

namespace TGC.MonoGame.Samples.Viewer.GUI.Modifiers
{
    public class DragFile : IModifier
    {
        private const int ModalFlags = 0;

        private string _title;

        private bool _oldVisibility;

        private int _selection;

        private string _currentPath;

        private bool _currentPathIsDir;

        public string P;

        private string[] _itemsInScope;
        private int _firstFileIndex;

        public DragFile(string title)
        {
            _title = title;
        }

        public void Draw()
        {
            var result = Draw(true, out P);
        }

        private bool Draw(bool isVisible, out string outPath)
        {
            bool result = false;
            outPath = "";
            if (_oldVisibility != isVisible)
            {
                _oldVisibility = isVisible;
                //Visiblity has changed.

                if (isVisible)
                {
                    //Only run when the visibility state changes to visible.

                    //Reset the path to the initial path.
                    _currentPath = Directory.GetCurrentDirectory();
                    _currentPathIsDir = true;

                    //Update paths based on current path
                    PopulateItems();

                    //Make the modal visible.
                    //ImGui::OpenPopup(m_title);
                    ImGui.OpenPopup(_title);
                }
            }

            bool isOpen = true;
            if (ImGui.BeginPopupModal(_title, ref isOpen, ImGuiWindowFlags.Modal))
            {
                if (ImGui.ListBox("##", ref _selection, _itemsInScope, _itemsInScope.Length, 10))
                {
                    //Update current path to the selected list item.
                    _currentPath = _itemsInScope[_selection];
                    _currentPathIsDir = _selection < _firstFileIndex;

                    //If the selection is a directory, repopulate the list with the contents of that directory.
                    if (_currentPathIsDir)
                    {
                        PopulateItems();
                    }
                }

                //Auto resize text wrap to popup width.
                ImGui.PushItemWidth(-1f);
                ImGui.TextWrapped(_currentPath + "        ");
                ImGui.PopItemWidth();

                ImGui.Spacing();
                ImGui.SameLine(ImGui.GetWindowWidth() - 30);

                // Make the "Select" button look / act disabled if the current selection is a directory.
                if (_currentPathIsDir)
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

                        outPath = _currentPath;
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
            var directories = Directory.GetDirectories(_currentPath);
            var files = Directory.GetFiles(_currentPath);

            _itemsInScope = new string[directories.Length + files.Length];
            Array.Copy(directories, _itemsInScope, directories.Length);
            Array.Copy(files, 0, _itemsInScope, directories.Length, files.Length);
            _firstFileIndex = directories.Length;
        }
    }

    internal struct File
    {
        private string alias;
        private string path;
    }
}
