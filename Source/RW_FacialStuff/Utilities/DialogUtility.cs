using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff.Utilities
{
    public static class DialogUtility
    {
        public const float BottomAreaHeight = 38f;

        private static readonly Vector2 BottomButSize = new Vector2(150f, 38f);

        public static bool DoMiddleButton(Rect innerRect, string label)
        {
            float top = innerRect.height - 38f;
            Rect rect = new Rect(innerRect.width / 2f - BottomButSize.x / 2f, top, BottomButSize.x, BottomButSize.y);
            return Widgets.ButtonText(rect, label);
        }

        public static void DoNextBackButtons(
            Rect innerRect,
            string middleLabel,
            string nextLabel,
            [NotNull] Action backAct,
            [NotNull] Action middleAct,
            [NotNull] Action nextAct)
        {
            float top = innerRect.height - 38f;
            Text.Font = GameFont.Small;
            Rect backRect = new Rect(0f, top, BottomButSize.x, BottomButSize.y);
            if (Widgets.ButtonText(backRect, "Back".Translate()))
            {
                backAct();
            }

            Rect randomRect = new Rect(
                innerRect.width / 2f - BottomButSize.x / 2f,
                top,
                BottomButSize.x,
                BottomButSize.y);
            if (Widgets.ButtonText(randomRect, middleLabel))
            {
                middleAct();
            }

            Rect nextRect = new Rect(innerRect.width - BottomButSize.x, top, BottomButSize.x, BottomButSize.y);
            if (Widgets.ButtonText(nextRect, nextLabel))
            {
                nextAct();
            }
        }
    }
}