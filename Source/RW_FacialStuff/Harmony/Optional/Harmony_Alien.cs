namespace FacialStuff.Harmony.Optional
{
    using System;

    using global::Harmony;

    using Verse;

    [StaticConstructorOnStartup]
    internal static class Harmony_Alien
    {
        static Harmony_Alien()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.alien_patch");

            try
            {
                ((Action)(() =>
                    {
                        if (AccessTools.Method("AlienRace.AlienPartGenerator".GetType(),
                                "GetAlienHead") == null)
                        {
                            return;
                        }

                        // Type patchType = "AlienRace.HarmoyPatches".GetType();
                        Type patchType = typeof(Alien_Patches);
                        harmony.Patch(
                            AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.GetPawnMesh)),
                            new HarmonyMethod(patchType, "GetPawnMesh"),
                            null,
                            null);

                        harmony.Patch(
                            AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.GetPawnHairMesh)),
                            new HarmonyMethod(patchType, "GetPawnHairMesh"),
                            null,
                            null);

                        harmony.Patch(
                            AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.DrawAddons)),
                            null,
                            new HarmonyMethod(patchType, "DrawAddons"),
                            null);
                    }))();
            }
            catch (Exception e)
            {

            }


        }

    }
}