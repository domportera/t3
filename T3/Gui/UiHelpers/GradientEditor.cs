﻿using System;
using System.Numerics;
using ImGuiNET;
using T3.Core;
using T3.Core.DataTypes;
using T3.Gui.Graph;
using T3.Gui.Interaction;
using UiHelpers;

namespace T3.Gui.UiHelpers
{
    public static class GradientEditor
    {
        /// <summary>
        /// Draw a gradient control that returns true, if gradient has been modified
        /// </summary>
        public static bool Draw(Gradient gradient, ImDrawListPtr drawList, ImRect areaOnScreen)
        {
            var modified = false;
            drawList.AddRect(areaOnScreen.Min, areaOnScreen.Max, Color.Black);
            drawList.AddRectFilled(areaOnScreen.Min, areaOnScreen.Max, new Color(0.15f, 0.15f, 0.15f, 1));

            // Draw Gradient background
            {
                drawList.PushClipRect(areaOnScreen.Min, areaOnScreen.Max);
                var lineColor = new Color(0f, 0f, 0f, 0.2f);
                var stripeOffset = GraphCanvas.Current == null ? 16f : (8f * GraphCanvas.Current.Scale.X);
                var lineWidth = stripeOffset / 2.7f;

                var h = areaOnScreen.GetHeight();
                var stripeCount = (int)((areaOnScreen.GetWidth() + h + 3 * lineWidth) / stripeOffset);
                var p = areaOnScreen.Min - new Vector2(h + lineWidth, +lineWidth);
                var offset = new Vector2(h + 2 * lineWidth,
                                         h + 2 * lineWidth);

                for (var i = 0; i < stripeCount; i++)
                {
                    drawList.AddLine(p, p + offset, lineColor, lineWidth);
                    p.X += stripeOffset;
                }

                drawList.PopClipRect();
            }

            //gradient.Steps.OrderBy(o => o.NormalizedPosition);
            gradient.Steps.Sort((x, y) => x.NormalizedPosition.CompareTo(y.NormalizedPosition));

            // Draw Gradient
            var lastColor = ImGui.ColorConvertFloat4ToU32(gradient.Steps[0].Color);
            var lastPos = areaOnScreen.Min;
            var maxPos = areaOnScreen.Max;
            foreach (var step in gradient.Steps)
            {
                var color = ImGui.ColorConvertFloat4ToU32(step.Color);
                maxPos.X = areaOnScreen.Min.X + areaOnScreen.GetWidth() * step.NormalizedPosition;
                drawList.AddRectFilledMultiColor(lastPos,
                                                 maxPos,
                                                 lastColor,
                                                 color,
                                                 color,
                                                 lastColor);
                lastPos.X = maxPos.X;
                lastColor = color;
            }

            if (lastPos.X < areaOnScreen.Max.X)
            {
                drawList.AddRectFilled(lastPos, areaOnScreen.Max, lastColor);
            }

            // Draw handles
            var anyHandleHovered = false;
            if (areaOnScreen.GetHeight() >= RequiredHeightForHandles)
            {
                Gradient.Step removedStep = null;
                foreach (var step in gradient.Steps)
                {
                    ImGui.PushID(step.Id.GetHashCode());
                    var handleArea = GetHandleAreaForPosition(step.NormalizedPosition);

                    // Interaction
                    ImGui.SetCursorScreenPos(handleArea.Min);
                    ImGui.InvisibleButton("gradientStep", new Vector2(StepHandleSize.X, areaOnScreen.GetHeight()));

                    if (ImGui.IsItemHovered())
                        anyHandleHovered = true;

                    var draggedOutside = false;
                    if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
                    {
                        draggedOutside = ImGui.GetMousePos().Y > areaOnScreen.Max.Y + 50;

                        step.NormalizedPosition = ((ImGui.GetMousePos().X - areaOnScreen.Min.X) / areaOnScreen.GetWidth()).Clamp(0, 1);
                        modified = true;
                    }

                    // Draw handle
                    if (draggedOutside)
                    {
                        handleArea.Min.Y += 10;
                        handleArea.Max.Y += 10;
                    }

                    if (ImGui.IsItemDeactivated())
                    {
                        var mouseOutsideThresholdAfterDrag = ImGui.GetMousePos().Y > areaOnScreen.Max.Y + 50;
                        if (mouseOutsideThresholdAfterDrag && gradient.Steps.Count > 1)
                            removedStep = step;
                    }

                    drawList.AddRectFilled(handleArea.Min, handleArea.Max, ImGui.ColorConvertFloat4ToU32(step.Color));
                    drawList.AddRect(handleArea.Min, handleArea.Max, Color.Black);
                    drawList.AddRect(handleArea.Min + Vector2.One, handleArea.Max - Vector2.One, Color.White);

                    if (ImGui.IsItemHovered()
                        && ImGui.IsMouseReleased(0)
                        && ImGui.GetIO().MouseDragMaxDistanceAbs[0].LengthSquared() < 2
                        && !ImGui.IsPopupOpen("##colorEdit"))
                        ImGui.OpenPopup("##colorEdit");

                    if (ImGui.BeginPopupContextItem("##colorEdit"))
                    {
                        anyHandleHovered = true;
                        modified = ImGui.ColorPicker4("edit", ref step.Color,
                                                      ImGuiColorEditFlags.Float | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreview);
                        ImGui.EndPopup();
                    }

                    ImGui.PopID();
                }

                if (removedStep != null)
                    gradient.Steps.Remove(removedStep);

                // Insert new range
                if (areaOnScreen.GetHeight() > MinInsertHeight)
                {
                    var insertRangeMin = new Vector2(areaOnScreen.Min.X, areaOnScreen.Max.Y - StepHandleSize.Y);
                    ImGui.SetCursorScreenPos(insertRangeMin);

                    var normalizedPosition = (ImGui.GetMousePos().X - insertRangeMin.X) / areaOnScreen.GetWidth();

                    if (ImGui.InvisibleButton("insertRange", areaOnScreen.Max - insertRangeMin))
                    {
                        gradient.Steps.Add(new Gradient.Step()
                                               {
                                                   NormalizedPosition = normalizedPosition,
                                                   Id = Guid.NewGuid(),
                                                   Color = gradient.Sample(normalizedPosition)
                                               });
                        modified = true;
                    }

                    if (ImGui.IsItemHovered() && !ImGui.IsItemActive() && !anyHandleHovered)
                    {
                        var handleArea = GetHandleAreaForPosition(normalizedPosition);
                        drawList.AddRect(handleArea.Min + Vector2.One, handleArea.Max - Vector2.One, new Color(1f, 1f, 1f, 0.4f));
                    }
                }
            }

            return modified;

            ImRect GetHandleAreaForPosition(float normalizedPosition)
            {
                var x = areaOnScreen.Min.X - StepHandleSize.X / 2f + areaOnScreen.GetWidth() * normalizedPosition;
                return new ImRect(new Vector2(x, areaOnScreen.Max.Y - StepHandleSize.Y), new Vector2(x + StepHandleSize.X, areaOnScreen.Max.Y + 2));
            }
        }

        private const float RequiredHeightForHandles = 20;
        private const int MinInsertHeight = 20;
        public static readonly Vector2 StepHandleSize = new Vector2(10, 20);
    }
}