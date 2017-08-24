namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Collections.Generic;
    using System.Linq;

    using EdB.PrepareCarefully;

    using FacialStuff;

    using global::Harmony;

    using Verse;

    public static class DialogManageImplantsPatch
    {
        private static Pawn pawn;

        [HarmonyPostfix]
        public static void InitializeWithPawn(CustomPawn __instance)
        {
            pawn = __instance.Pawn;
        }

        [HarmonyPostfix]
        public static void UpdatePawnForImpplants()
        {
            pawn?.Drawer?.renderer?.graphics.ResolveAllGraphics();
        }

    }
}
