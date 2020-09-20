/*
 * MIT License
 *
 * Copyright (c) [2017] [Travis Offtermatt]
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs;
using UnityEngine;
using Verse;

namespace FacialStuff.FaceEditor.UI.Util
{
    [StaticConstructorOnStartup]
    public static class WidgetUtil
    {
        public static readonly Vector2 NavButtonSize = new Vector2(30f, 30f);

        public static Texture2D NextTexture;

        public static Texture2D PreviousTexture;

        private static GUIStyle _middleCenterGuiStyle;

        public static GUIStyle MiddleCenter
        {
            get
            {
                if (_middleCenterGuiStyle != null)
                {
                    return _middleCenterGuiStyle;
                }

                _middleCenterGuiStyle = GUI.skin.label;
                _middleCenterGuiStyle.alignment = TextAnchor.MiddleCenter;

                return _middleCenterGuiStyle;
            }
        }

        public static float SelectionRowHeight => NavButtonSize.y;

        public static void AddSelectorWidget(
            float left,
            float top,
            float width,
            string label,
            ASelectionWidgetDto selectionWidgetDto)
        {
            const float buffer = 5f;

            Rect rect = new Rect(left, top, width, SelectionRowHeight);
            GUI.BeginGroup(rect);
            GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            left = 0;
            if (label != null)
            {
                // Text.Anchor = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(0, 0, 75, SelectionRowHeight), label);
                left = 80;
            }

            Text.Anchor = TextAnchor.MiddleCenter;

            Rect previousButtonRect = new Rect(left, 0, NavButtonSize.x, NavButtonSize.y);
            if (GUI.Button(previousButtonRect, PreviousTexture))
            {
                selectionWidgetDto.DecreaseIndex();
            }

            Rect labelRect = new Rect(
                NavButtonSize.x + buffer + left,
                0,
                rect.width - 2 * NavButtonSize.x - 2 * buffer - left,
                NavButtonSize.y);
            GUI.Label(labelRect, selectionWidgetDto.SelectedItemLabel, MiddleCenter);

            GUI.color = Color.grey;
            Widgets.DrawBox(labelRect);

            Rect nextButtonRect = new Rect(rect.width - NavButtonSize.x, 0, NavButtonSize.x, NavButtonSize.y);
            if (GUI.Button(nextButtonRect, NextTexture))
            {
                selectionWidgetDto.IncreaseIndex();
            }

            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }
}