using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Harmony
{
    [HarmonyPatch(
    typeof(Pawn_HealthTracker),
    "AddHediff",
    new[]
    {
        typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult)
    })]
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
    "RemoveHediff",
    new[]
    {
        typeof(Hediff)
    })]
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
    "RestorePart",
    new[]
    {
        typeof(BodyPartRecord), typeof(Hediff), typeof(bool)
    })]
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
