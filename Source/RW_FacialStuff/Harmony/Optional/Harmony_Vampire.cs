using System;
using Harmony;
using Vampire;
using Verse;

namespace FacialStuff.Harmony.Optional
{
    [StaticConstructorOnStartup]
    internal static class Harmony_Vampire
    {
        static Harmony_Vampire()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.vampire_patch");

            try
            {
                ((Action)(() =>
                    {
                        if (AccessTools.Method(
                                typeof(VampireUtility),
                                nameof(VampireUtility.AdjustTimeTables)) == null)
                        {
                            return;
                        }

                        harmony.Patch(
                            AccessTools.Method(
                                typeof(CompVampire),
                                nameof(CompVampire.PostExposeData)),
                            null,
                            new HarmonyMethod(typeof(Vampire_Patches), nameof(Vampire_Patches.Transformed_Postfix)));

                        harmony.Patch(
                            AccessTools.Method(
                                typeof(CompVampire), "set_CurrentForm"),
                            null,
                            new HarmonyMethod(typeof(Vampire_Patches), nameof(Vampire_Patches.Transformed_Postfix)));

                        harmony.Patch(
                            AccessTools.Method(
                                typeof(CompVampire),
                                nameof(CompVampire.InitializeVampirism)),
                            null,
                            new HarmonyMethod(typeof(Vampire_Patches), nameof(Vampire_Patches.Transformed_Postfix)));

                    }))();
            }
            catch (TypeLoadException)
            {
            }
        }
    }
}