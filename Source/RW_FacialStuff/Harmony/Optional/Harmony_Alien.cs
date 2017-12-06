namespace FacialStuff.Harmony.Optional
{
    using System;

    using global::Harmony;

    using Verse;

    [StaticConstructorOnStartup]
    internal static class Harmony_Alien
    {
        public static bool aliensSighted;
        static Harmony_Alien()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.alien_patch");

            try
            {
                ((Action)(() =>
                    {
                        if (AccessTools.Method(
                                typeof(AlienRace.AlienPartGenerator),
                                nameof(AlienRace.AlienPartGenerator.GetAlienHead)) == null)
                        {
                            return;
                        }

                        aliensSighted = true;

                      //  Type patchType = "AlienRace.HarmoyPatches".GetType();
                        Type patchType = typeof(AlienFaces);
                        harmony.Patch(
                            AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.GetPawnMesh)),
                            new HarmonyMethod(patchType, nameof(AlienFaces.GetPawnMesh)),
                            null,
                            null);

                        harmony.Patch(
                            AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.GetPawnHairMesh)),
                            new HarmonyMethod(patchType, nameof(AlienFaces.GetPawnHairMesh)), 
                            null,
                            null);

                        harmony.Patch(
                            AccessTools.Method(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.DrawAddons)),
                            new HarmonyMethod(patchType, nameof(AlienFaces.DrawAddons)),
                            null,
                            null);
                    }))();
            }

            catch (Exception e)
            {

            }
        }
    }
}