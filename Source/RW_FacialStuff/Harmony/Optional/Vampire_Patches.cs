using System.Linq;
using UnityEngine;
using Vampire;
using Verse;

namespace FacialStuff.Harmony.Optional
{
    public static class Vampire_Patches
    {
        public static void Transformed_Postfix(CompVampire __instance)
        {
            if (__instance.IsVampire)
            {
                if (!__instance.Pawn.GetCompFace(out CompFace compFace))
                {
                    return;
                }

                if (__instance.Transformed || __instance.Bloodline?.headGraphicsPath != string.Empty)
                {
                    compFace.Deactivated = true;
                    return;
                }

                compFace.Deactivated = false;
            }
        }

        public static void DrawEquipment_PostFix(HumanBipedDrawer __instance, Vector3 rootLoc, bool portrait)
        {
            Pawn pawn = __instance.Pawn;
            if (pawn.health?.hediffSet?.hediffs == null || pawn.health.hediffSet.hediffs.Count <= 0)
            {
                return;
            }

            Hediff shieldHediff =
            pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
                                                             x.TryGetComp<HediffComp_Shield>() != null);

            if (shieldHediff == null)
            {
                return;
            }

            HediffComp_Shield shield = shieldHediff.TryGetComp<HediffComp_Shield>();
            shield?.DrawWornExtras();
        }
    }
}