using FacialStuff.Defs;
using UnityEngine;
using Verse;

namespace FacialStuff.FaceEditor
{
    public partial class Dialog_FaceStyling
    {
        public void DrawBrowPicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(BrowDefs.Count / (float)thisColumns);

            rect3.height = num * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if(rect3.height > rect2.height)
            {
                vector.x -= 16f / Columns;
                vector.y -= 16f / Columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this._scrollPositionBrow, rect3);
            GUI.BeginGroup(rect3);

            for(int i = 0; i < BrowDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawBrowPickerCell(BrowDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBrowPickerCell(BrowDef brow, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = brow.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if(brow == this.NewBrow)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            } else
            {
                if(brow == this._originalBrow)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.color = Color.black;
            GUI.DrawTexture(rect, this.BrowGraphic(brow).MatSouth.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);

            // ReSharper disable once InvertIf
            if(Widgets.ButtonInvisible(rect))
            {
                this.NewBrow = brow;
                this.RemoveColorPicker();
            }
        }
    }
}
