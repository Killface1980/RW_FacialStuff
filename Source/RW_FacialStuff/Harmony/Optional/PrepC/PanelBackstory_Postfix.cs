using EdB.PrepareCarefully;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FacialStuff.Harmony.Optional.PrepC
{
    public static class PanelBackstory_Postfix
    {
        [HarmonyPostfix]
        public static void AddFaceEditButton(PanelBackstory __instance, State state)
        {
            Rect panelRect = __instance.PanelRect;
            Pawn pawn = state.CurrentPawn.Pawn;

            if (!pawn.HasCompFace())
            {
                return;
            }

            Rect rect = new Rect(panelRect.width - 90f, 9f, 25f, 25f);
            GUI.color = rect.Contains(Event.current.mousePosition)
                        ? Color.cyan
                        : new Color(0.623529f, 0.623529f, 0.623529f);

            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("Buttons/ButtonFace"));
            string tip = "FacialStuffEditor.EditFace".Translate();
            TooltipHandler.TipRegion(rect, tip);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera();
                HarmonyPatchesFS.OpenStylingWindow(pawn);
            }
        }
    }
}