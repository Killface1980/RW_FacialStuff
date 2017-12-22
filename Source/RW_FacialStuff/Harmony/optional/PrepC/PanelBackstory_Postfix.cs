namespace FacialStuff.Harmony.Optional.PrepC
{
    using EdB.PrepareCarefully;

    using global::Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.Sound;

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
            if (rect.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.cyan;

                // GUI.color = new Color(0.97647f, 0.97647f, 0.97647f);
            }
            else
            {
                GUI.color = new Color(0.623529f, 0.623529f, 0.623529f);
            }

            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("Buttons/ButtonFace", true));
            string tip = "FacialStuffEditor.EditFace".Translate();
            TooltipHandler.TipRegion(rect, tip);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect, false))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera(null);
                HarmonyPatchesFS.OpenStylingWindow(pawn);
            }
        }
    }
}