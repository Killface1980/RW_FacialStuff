namespace PawnPlus.Harmony
{
    using HarmonyLib;

    using Verse;

    [HarmonyPatch(
    typeof(Pawn_HealthTracker),
    nameof(Pawn_HealthTracker.AddHediff),
    typeof(Hediff),
    typeof(BodyPartRecord),
    typeof(DamageInfo),
    typeof(DamageWorker.DamageResult))]
    class HarmonyPatch_Pawn_HealthTracker_AddHediff
	{
        public static void Postfix(Hediff hediff)
		{
            Pawn pawn = hediff.pawn;
            if(pawn == null)
			{
                return;
			}

            if(hediff?.Part == null)
            {
                return;
            }

            if(pawn.GetCompFace(out CompFace compFace))
			{
                compFace.NotifyBodyPartHediffGained(hediff.Part, hediff);
			}
		}
	}

    [HarmonyPatch(
    typeof(Pawn_HealthTracker),
    nameof(Pawn_HealthTracker.RemoveHediff),
    typeof(Hediff))]
    class HarmonyPatch_Pawn_HealthTracker_RemoveHediff
    {
        public static void Prefix(Hediff hediff)
        {
            Pawn pawn = hediff.pawn;
            if(pawn == null)
            {
                return;
            }

            if(hediff?.Part == null)
            {
                return;
            }

            if(pawn.GetCompFace(out CompFace compFace))
            {
                compFace.NotifyBodyPartHediffLost(hediff.Part, hediff);
            }
        }
    }

    [HarmonyPatch(
    typeof(Pawn_HealthTracker),
    nameof(Pawn_HealthTracker.RestorePart),
    typeof(BodyPartRecord),
    typeof(Hediff),
    typeof(bool))]
    class HarmonyPatch_Pawn_HealthTracker_RestorePart
    {
        public static void Prefix(Pawn_HealthTracker __instance, BodyPartRecord part)
        {
            if(part == null)
            {
                return;
            }

            Pawn pawn = __instance.hediffSet.pawn;
            if(pawn == null)
            {
                return;
            }

            if(pawn.GetCompFace(out CompFace compFace))
            {
                compFace.NotifyBodyPartRestored(part);
            }
        }
    }
}
