namespace PawnPlus.Harmony
{
    using HarmonyLib;

    using RimWorld;

    using Verse;

    [HarmonyPatch(
    typeof(Pawn_InteractionsTracker),
    nameof(Pawn_InteractionsTracker.TryInteractWith))]
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
                compFace.SetHeadTarget(recipient, IHeadBehavior.TargetType.SocialRecipient);
            }

            if(recipient.GetCompFace(out CompFace recipientFace))
            {
                recipientFace.SetHeadTarget(initiator, IHeadBehavior.TargetType.SocialInitiator);
            }
        }
    }
}
