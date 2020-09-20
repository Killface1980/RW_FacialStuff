using PawnPlus.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.FaceEditor
{
	public partial class Dialog_FaceStyling
	{
        public void DrawEyePicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(_eyeDefs.Count / (float)thisColumns);

            rect3.height = num * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if(rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref _scrollPositionEye, rect3);
            GUI.BeginGroup(rect3);

            for(int i = 0; i < _eyeDefs.Count; i++)
            {
                int num2 = i / thisColumns;
                int num3 = i % thisColumns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                DrawEyePickerCell(_eyeDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawEyePickerCell(EyeDef eye, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = eye.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if(eye == NewEye)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            } else
            {
                if(eye == _originalEye)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.DrawTexture(rect, RightEyeGraphic(eye).MatSouth.mainTexture);
            GUI.DrawTexture(rect, LeftEyeGraphic(eye).MatSouth.mainTexture);

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if(Widgets.ButtonInvisible(rect))
            {
                NewEye = eye;
                RemoveColorPicker();
            }
        }
    }
}
