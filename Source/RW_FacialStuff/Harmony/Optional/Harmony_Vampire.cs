namespace FacialStuff.Harmony.Optional
{
    using System;

    using global::Harmony;

    using Verse;

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
                                typeof(Vampire.VampireUtility),
                                nameof(Vampire.VampireUtility.AdjustTimeTables)) == null)
                        {
                            return;
                        }

                        harmony.Patch(
                            AccessTools.Method(
                                typeof(Vampire.CompVampire),
                                nameof(Vampire.CompVampire.PostExposeData)),
                            null,
                            new HarmonyMethod(typeof(Vampire_Patches), nameof(Vampire_Patches.Transformed_Postfix)),
                            null);

                        harmony.Patch(
                            AccessTools.Method(
                                typeof(Vampire.CompVampire), "set_CurrentForm"),
                            null,
                            new HarmonyMethod(typeof(Vampire_Patches), nameof(Vampire_Patches.Transformed_Postfix)),
                            null);

                        harmony.Patch(
                            AccessTools.Method(
                                typeof(Vampire.CompVampire),
                                nameof(Vampire.CompVampire.InitializeVampirism)),
                            null,
                            new HarmonyMethod(typeof(Vampire_Patches), nameof(Vampire_Patches.Transformed_Postfix)),
                            null);


                    }))();
            }
            catch (TypeLoadException)
            {
            }
        }
    }
}