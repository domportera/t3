﻿using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using T3.Core.Logging;
using T3.graph;
using T3.Gui.Commands;
using T3.Gui.Graph.Interaction;
using T3.Gui.Graph.Rendering;
using T3.Gui.Interaction;
using T3.Gui.Interaction.PresetSystem;
using T3.Gui.Interaction.Timing;
using T3.Gui.Selection;
using T3.Gui.Styling;
using T3.Gui.UiHelpers;
using T3.Gui.Windows;
using T3.Operators.Types.Id_eff2ffff_dc39_4b90_9b1c_3c0a9a0108c6;

namespace T3.Gui
{
    public class T3Ui
    {
        static T3Ui()
        {
            var operatorsAssembly = Assembly.GetAssembly(typeof(Operators.Types.Id_5d7d61ae_0a41_4ffa_a51d_93bab665e7fe.Value));
            UiModel = new UiModel(operatorsAssembly);
            _userSettings = new UserSettings();
            _projectSettings = new ProjectSettings();
            BeatTiming = new BeatTiming();
            WindowManager = new WindowManager();
        }

        public void Draw()
        {
            OpenedPopUpName = string.Empty;
            PresetSystem.Update();

            SelectionManager.ProcessNewFrame();
            SrvManager.FreeUnusedTextures();
            WindowManager.Draw();
            BeatTiming.Update();

            SingleValueEdit.StartNextFrame();

            SwapHoveringBuffers();
            TriggerGlobalActionsFromKeyBindings();
            DrawAppMenu();
            TableTest();
        }

        private void TableTest()
        {
            ImGui.Begin("Test");
            const float width = 60;
            ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing,2);

            ImGui.PushFont(Fonts.FontSmall);
            FieldInfo[] members = typeof(Population).GetFields();

            // List Header 
            foreach (var fi in members)
            {
                if (fi.FieldType == typeof(float))
                {
                    ImGui.Selectable(" " + fi.Name, false, ImGuiSelectableFlags.None, new Vector2(width, 30));
                }
                else if (fi.FieldType == typeof(Vector4))
                {
                    bool isFirst = true;
                    foreach(var c in new[] { ".x", ".y", ".z", ".w" })
                    {
                        ImGui.Selectable((isFirst ? " " + fi.Name : "_")  + "\n" + c, false, ImGuiSelectableFlags.None, new Vector2(width, 30));
                        ImGui.SameLine();
                        isFirst = false;
                    }
                }

                ImGui.SameLine();
            }

            ImGui.NewLine();

            // Values
            for (var objectIndex = 0; objectIndex < _testPopulations.Length; objectIndex++)
            {
                ImGui.PushID(objectIndex);
                var obj = _testPopulations[objectIndex];
                
                for (var fieldIndex = 0; fieldIndex < members.Length; fieldIndex++)
                {
                    FieldInfo fi = members[fieldIndex];
                    var o = fi.GetValue(obj);
                    if (o is float f)
                    {
                        DrawFloatManipulation(ref f, fieldIndex);
                    }
                    else if (o is Vector4 vector4)
                    {
                        DrawFloatManipulation(ref vector4.X, fieldIndex * 100 + 0);
                        DrawFloatManipulation(ref vector4.Y, fieldIndex * 100 + 1);
                        DrawFloatManipulation(ref vector4.Z, fieldIndex * 100 + 2);
                        DrawFloatManipulation(ref vector4.W, fieldIndex * 100 + 3);
                    }
                    else
                    {
                        ImGui.SetNextItemWidth(width);
                        ImGui.Text("?");
                        ImGui.SameLine();
                    }
                }

                ImGui.NewLine();
                ImGui.PopID();
            }

            ImGui.PopFont();
            ImGui.PopStyleVar();
            ImGui.End();

            void DrawFloatManipulation(ref float f, int index=0)
            {
                ImGui.PushID(index);
                ImGui.SetNextItemWidth(width);
                ImGui.DragFloat("##sdf", ref f);
                ImGui.SameLine();
                ImGui.PopID();
            }
        }

        
        
        private IEnumerable<float> GetVectorFloats(Vector4 vector4)
        {
            yield return vector4.X;
            yield return vector4.Y;
            yield return vector4.Z;
            yield return vector4.W;
        } 

        private static Population[] _testPopulations =
            {
                new Population
                    {
                        occurationRatio = 3,
                        trail = new Vector4(1, 2, 2, 1),
                        preferredLevel = new Vector4(1, 2, 2, 1),
                        lookDistance = 2,
                        lookRotation = 3,
                        moveDistance = 5,
                        moveRotation = 2,
                        acceleration = 1,
                        friction = 4
                    },
                new Population
                    {
                        occurationRatio = 3,
                        trail = new Vector4(1, 2, 2, 1),
                        preferredLevel = new Vector4(1, 2, 2, 1),
                        lookDistance = 2,
                        lookRotation = 3,
                        moveDistance = 5,
                        moveRotation = 2,
                        acceleration = 1,
                        friction = 4
                    },
            };

        public struct Population
        {
            public float occurationRatio;
            public Vector4 trail; // Vec4 that could also include negative trails (for consumption)
            public Vector4 preferredLevel; // Vec4 that should also include negative values (for avoiding)
            public float lookDistance;
            public float lookRotation;
            public float moveDistance;
            public float moveRotation;
            public float acceleration;
            public float friction;
        }

        private void TriggerGlobalActionsFromKeyBindings()
        {
            if (KeyboardBinding.Triggered(UserActions.Undo))
            {
                UndoRedoStack.Undo();
            }
            else if (KeyboardBinding.Triggered(UserActions.Redo))
            {
                UndoRedoStack.Redo();
            }
            else if (KeyboardBinding.Triggered(UserActions.Save))
            {
                Task.Run(Save);
            }
        }

        private void DrawAppMenu()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6, 6));
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save"))
                    {
                        Task.Run(Save); // Async save
                    }

                    if (ImGui.MenuItem("Quit"))
                    {
                        Application.Exit();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "CTRL+Z", false, UndoRedoStack.CanUndo))
                    {
                        UndoRedoStack.Undo();
                    }

                    if (ImGui.MenuItem("Redo", "CTRL+Y", false, UndoRedoStack.CanRedo))
                    {
                        UndoRedoStack.Redo();
                    }

                    ImGui.Separator();
                    //if (ImGui.MenuItem("Cut", "CTRL+X")) { }
                    //if (ImGui.MenuItem("Copy", "CTRL+C")) { }
                    //if (ImGui.MenuItem("Paste", "CTRL+V")) { }

                    if (ImGui.MenuItem("Fix File references", ""))
                    {
                        FileReferenceOperations.FixOperatorFilepathsCommand_Executed();
                    }

                    if (ImGui.BeginMenu("Bookmarks"))
                    {
                        GraphBookmarkNavigation.DrawBookmarksMenu();
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                WindowManager.DrawWindowsMenu();

                _statusErrorLine.Draw();
                ImGui.EndMainMenuBar();
            }

            ImGui.PopStyleVar(2);
        }

        private readonly object _saveLocker = new object();
        private readonly Stopwatch _saveStopwatch = new Stopwatch();

        private void Save()
        {
            lock (_saveLocker)
            {
                _saveStopwatch.Restart();

                UiModel.Save();

                _saveStopwatch.Stop();
                Log.Debug($"Saving took {_saveStopwatch.ElapsedMilliseconds}ms.");
            }
        }

        public static void AddHoveredId(Guid id)
        {
            _hoveredIdsForNextFrame.Add(id);
        }

        private static void SwapHoveringBuffers()
        {
            HoveredIdsLastFrame = _hoveredIdsForNextFrame;
            _hoveredIdsForNextFrame = new HashSet<Guid>();
        }

        private static HashSet<Guid> _hoveredIdsForNextFrame = new HashSet<Guid>();
        public static HashSet<Guid> HoveredIdsLastFrame { get; private set; } = new HashSet<Guid>();

        private readonly StatusErrorLine _statusErrorLine = new StatusErrorLine();
        public static readonly UiModel UiModel;
        private static UserSettings _userSettings;
        private static ProjectSettings _projectSettings;
        public static readonly PresetSystem PresetSystem = new PresetSystem();
        public static readonly BeatTiming BeatTiming;
        public static readonly WindowManager WindowManager;

        public static string OpenedPopUpName; // This is reset on Frame start and can be useful for allow context menu to stay open even if a
        // later context menu would also be opened. There is probably some ImGui magic to do this probably. 

        public static IntPtr NotDroppingPointer = new IntPtr(0);
        public static bool DraggingIsInProgress = false;
        public static bool ShowSecondaryRenderWindow => WindowManager.ShowSecondaryRenderWindow;
        public const string FloatNumberFormat = "{0:F2}";

        [Flags]
        public enum EditingFlags
        {
            None = 0,
            ExpandVertically = 1 << 1,
            PreventMouseInteractions = 1 << 2,
            PreventZoomWithMouseWheel = 1 << 3,
            PreventPanningWithMouse = 1 << 4,
        }
    }
}