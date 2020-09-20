using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PawnPlus.Harmony
{
    [HarmonyPatch(
    typeof(Pawn_InteractionsTracker),
    "TryInteractWith")]
    class HarmonyPatch_Pawn_InteractionsTracker
	{
        public static void Postfix(bool __result, Pawn ___pawn, Pawn recipient)
		{
            // ___pawn is the private member variable "pawn" in Pawn_InteractionsTracker class
            Pawn initiator = ___pawn;
            if(!__result || initiator == null || recipient == null)
            {
                return;
            }
            if(initiator.GetCompFace(out CompFace compFace))
            {
                compFace.HeadBehavior.SetTarget(recipient, IHeadBehavior.TargetType.SocialRecipient);
            }
            if(recipient.GetCompFace(out CompFace recipientFace))
            {
                recipientFace.HeadBehavior.SetTarget(initiator, IHeadBehavior.TargetType.SocialInitiator);
            }
        }
    }
}
