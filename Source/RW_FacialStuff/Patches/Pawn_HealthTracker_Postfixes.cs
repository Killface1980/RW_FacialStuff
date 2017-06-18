using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RW_FacialStuff.Detouring
{
    using System.Reflection;

    using Harmony;

    using RimWorld;

    using Verse;

    public static class Pawn_HealthTracker_Patches
    {
        [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) })]
        public static class AddHediff_Postfix
        {
            private static Type PawnRendererType = null;
            private static FieldInfo PawnFieldInfo;

            private static void GetReflections()
            {
                if (PawnRendererType == null)
                {
                    PawnRendererType = typeof(Pawn_HealthTracker);
                    PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }

            [HarmonyPostfix]
            public static void CheckBionic(
                Pawn_HealthTracker __instance,
                Hediff hediff,
                BodyPartRecord part = null,
                DamageInfo? dinfo = null)
            {
                if (part == null)
                    return;

                GetReflections();


                AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
                if (addedPartProps != null)
                {
                    if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye || part.def == BodyPartDefOf.Jaw)
                    {
                        Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                }
            }

        }


        [HarmonyPatch(typeof(Pawn_HealthTracker), "RestorePart")]
        public static class RestorePart_Postfix
        {
            private static Type PawnRendererType = null;
            private static FieldInfo PawnFieldInfo;

            private static void GetReflections()
            {
                if (PawnRendererType == null)
                {
                    PawnRendererType = typeof(Pawn_HealthTracker);
                    PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }


            [HarmonyPostfix]
            public static void CheckBionic_RestorePart(
                Pawn_HealthTracker __instance,
                BodyPartRecord part, Hediff diffException = null, bool checkStateChange = true)
            {
                GetReflections();

                Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

                if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye || part.def == BodyPartDefOf.Head)
                {
                    //     AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
                    //     if (addedPartProps != null && addedPartProps.isBionic)
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();

                }
            }

        }
    }
}
