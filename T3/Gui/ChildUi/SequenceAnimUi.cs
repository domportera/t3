﻿using System.Numerics;
using ImGuiNET;
using T3.Core;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Gui.ChildUi.Animators;
using T3.Gui.Styling;
using T3.Operators.Types.Id_94a392e6_3e03_4ccf_a114_e6fafa263b4f;
using UiHelpers;

namespace T3.Gui.ChildUi
{
    public static class SequenceAnimUi
    {
        public static SymbolChildUi.CustomUiResult DrawChildUi(Instance instance, ImDrawListPtr drawList, ImRect screenRect)
        {
            if (!(instance is SequenceAnim sequenceAnim)
                || !ImGui.IsRectVisible(screenRect.Min, screenRect.Max))
                return SymbolChildUi.CustomUiResult.None;

            ImGui.PushID(instance.SymbolChildId.GetHashCode());
            
            // if (RateEditLabel.Draw(ref sequenceAnim.Rate.TypedInputValue.Value,
            //                        screenRect, drawList, nameof(sequenceAnim) + " "))
            // {
            //     sequenceAnim.Rate.Input.IsDefault = false;
            //     sequenceAnim.Rate.DirtyFlag.Invalidate();
            // }

            var isEditActive = false;
            var mousePos = ImGui.GetMousePos();
            var editUnlocked = ImGui.GetIO().KeyCtrl;
            //var highlight = editUnlocked;
            
            // Speed Interaction
            //var speedRect = selectableScreenRect;
            //speedRect.Max.X = speedRect.Min.X +  speedRect.GetWidth() * 0.2f;
            //ImGui.SetCursorScreenPos(speedRect.Min);


            var h = screenRect.GetHeight();
            var w = screenRect.GetWidth();
            if (h < 10 || sequenceAnim._currentSequence == null || sequenceAnim._currentSequence.Count == 0)
            {
                return SymbolChildUi.CustomUiResult.None;
            }
            
            
            
            if (editUnlocked)
            {
                ImGui.SetCursorScreenPos(screenRect.Min);
                ImGui.InvisibleButton("rateButton", screenRect.GetSize());
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
                }

                isEditActive = ImGui.IsItemActive();
            }
            
            

            
            drawList.PushClipRect(screenRect.Min, screenRect.Max, true);
            
            // Draw bins and window
            
            var x = screenRect.Min.X;
            var bottom = screenRect.Max.Y;

            var barCount = sequenceAnim._currentSequence.Count;
            var barWidth = w / barCount;
            var xPeaks = screenRect.Min.X;

            var currentIndex = (int)(sequenceAnim._normalizedBarTime * barCount);
            
            
            ImGui.PushFont(Fonts.FontSmall);
            for (int barIndex = 0; barIndex < barCount; barIndex++)
            {
                var pMin = new Vector2(x , screenRect.Min.Y);
                var pMax = new Vector2(x + barWidth, bottom - 1);

                if (isEditActive && mousePos.X > pMin.X && mousePos.X < pMax.X)
                {
                    sequenceAnim.SetStepValue(sequenceAnim._sequenceIndex, barIndex, 1-((mousePos.Y +3 - screenRect.Min.Y) / (h-6)).Clamp(0, 1));
                }
                
                var highlightFactor = barIndex == currentIndex
                                          ? 1-(sequenceAnim._normalizedBarTime * barCount - barIndex).Clamp(0,1)
                                          : 0;

                var barIntensity = barIndex % 4 == 0 ? 0.4f : 0.1f;

                drawList.AddRectFilled(pMin,
                                       new Vector2(x + 1, bottom - 1),
                                       Color.Black.Fade(barIntensity)
                                      );

                var peak= sequenceAnim._currentSequence[barIndex];
                drawList.AddRectFilled(new Vector2(x + 1, bottom - peak * h - 2),
                                       new Vector2(x + barWidth, bottom-1),
                                       Color.Mix(_inactiveColor, _highlightColor,highlightFactor));
                
                drawList.AddText(pMin + new Vector2(2,0), Color.Black.Fade(barIntensity), "" + (barIndex + 1));
                x += barWidth;
                xPeaks += barWidth;
            }
            ImGui.PopFont();
            
            var min = screenRect.Min + new Vector2(sequenceAnim._normalizedBarTime * w, 0);
            drawList.AddRectFilled(min, 
                                   min + new Vector2(1, h), 
                                   T3Style.FragmentLineColor);
            
            drawList.PopClipRect();
            ImGui.PopID();
            return SymbolChildUi.CustomUiResult.Rendered | SymbolChildUi.CustomUiResult.PreventInputLabels;
        }

        // private static float _dragStartBias;
        // private static float _dragStartRatio;
        
        private static readonly Color _highlightColor = Color.Orange;
        private static readonly Color _inactiveColor = Color.Black.Fade(0.3f);
        
        //private static readonly Vector2[] GraphLinePoints = new Vector2[GraphListSteps];
        private const int GraphListSteps = 80;
    }
}