namespace FacialStuff.Harmony.optional.PrepC
{
    using System;
    using System.Reflection;

    using FacialStuff;
    using FacialStuff.FaceStyling_Bench;

    using global::Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;
    using Verse.Sound;

    public static class PageConfigureStartingPawns_Postfix
    {
        private static FieldInfo PawnFieldInfo;
        private static Type PageConfigureStartingPawnsType;

        private static void GetReflections()
        {
            if (PageConfigureStartingPawnsType != null)
            {
                return;
            }

            PageConfigureStartingPawnsType = typeof(Page_ConfigureStartingPawns);

            PawnFieldInfo = PageConfigureStartingPawnsType.GetField("curPawn", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyPostfix]
        public static void AddFaceEditButton(Page_ConfigureStartingPawns __instance)
        {
            GetReflections();

            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

            CompFace face = pawn.TryGetComp<CompFace>();
            if (face == null)
            {
                return;
            }

            // Shitty Transpiler, doin' it on my own
            Rect rect = new Rect(540f, 92f, 25f, 25f);
            if (rect.Contains(Event.current.mousePosition))
            {
                GUI.color = new Color(0.97647f, 0.97647f, 0.97647f);
            }
            else
            {
                GUI.color = new Color(0.623529f, 0.623529f, 0.623529f);
            }


            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("Buttons/ButtonFace", true));
            string tip = "FacialStuffEditor.FaceStylerTitle".Translate();
            TooltipHandler.TipRegion(rect, tip);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect, false))
            {
                SoundDefOf.TickLow.PlayOneShotOnCamera(null);
                Find.WindowStack.Add(new DialogFaceStyling(pawn));
            }
        }
    }
}
