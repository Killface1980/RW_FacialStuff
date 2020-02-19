namespace FacialStuff
{
    using System.Collections.Generic;

    using UnityEngine;

    using Verse;

    public static class WidgetsFS
    {
        // Verse.Widgets
        public static float HorizontalSlider(
            Rect rect,
            float value,
            float leftValue,
            float rightValue,
            bool middleAlignment = false,
            string label = null,
            string leftAlignedLabel = null,
            string rightAlignedLabel = null,
            float roundTo = -1f,
            Dictionary<int, float> keysFloats = null)
        {
            if (middleAlignment || !label.NullOrEmpty())
            {
                rect.y += Mathf.Round((rect.height - 16f) / 2f);
            }

            if (!label.NullOrEmpty())
            {
                rect.y += 5f;
            }

            float num = GUI.HorizontalSlider(rect, value, leftValue, rightValue);
            if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
            {
                TextAnchor anchor = Text.Anchor;
                GameFont font = Text.Font;
                Text.Font = GameFont.Tiny;
                float num2 = (!label.NullOrEmpty()) ? Text.CalcSize(label).y : 18f;
                rect.y = rect.y - num2 + 3f;
                if (!leftAlignedLabel.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperLeft;
                    Widgets.Label(rect, leftAlignedLabel);
                }

                if (!rightAlignedLabel.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    Widgets.Label(rect, rightAlignedLabel);
                }

                if (!label.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(rect, label);
                }

                Text.Anchor = anchor;
                Text.Font = font;
            }

            if (roundTo > 0f)
            {
                num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
            }

            return num;
        }

    }
}
