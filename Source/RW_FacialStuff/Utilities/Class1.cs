using System;
using UnityEngine;
namespace Verse
{
    public static class DialogUtility
    {
        public const float BottomAreaHeight = 38f;
        private static readonly Vector2 BottomButSize = new Vector2(150f, 38f);
        public static void DoNextBackButtons(Rect innerRect, string nextLabel, Action nextAct, Action backAct)
        {
            float top = innerRect.height - 38f;
            Text.Font = GameFont.Small;
            if (backAct != null)
            {
                Rect rect = new Rect(0f, top, DialogUtility.BottomButSize.x, DialogUtility.BottomButSize.y);
                if (Widgets.ButtonText(rect, "Back".Translate(), true, false))
                {
                    backAct();
                }
            }
            if (nextAct != null)
            {
                Rect rect2 = new Rect(innerRect.width - DialogUtility.BottomButSize.x, top, DialogUtility.BottomButSize.x, DialogUtility.BottomButSize.y);
                if (Widgets.ButtonText(rect2, nextLabel, true, false))
                {
                    nextAct();
                }
            }
        }
        public static bool DoMiddleButton(Rect innerRect, string label)
        {
            float top = innerRect.height - 38f;
            Rect rect = new Rect(innerRect.width / 2f - DialogUtility.BottomButSize.x / 2f, top, DialogUtility.BottomButSize.x, DialogUtility.BottomButSize.y);
            return Widgets.ButtonText(rect, label, true, false);
        }
    }
}