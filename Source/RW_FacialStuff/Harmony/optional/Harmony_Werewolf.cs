namespace FacialStuff.Harmony.optional
{
    using System;
    using System.Linq;

    using global::Harmony;

    using Verse;

    [StaticConstructorOnStartup]
    class Harmony_Werewolf
    {
        static Harmony_Werewolf()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.werewolf_patch");

            try
            {
                ((Action)(() =>
                    {
                        if (AccessTools.Method(
                                typeof(Werewolf.WerewolfUtility),
                                nameof(Werewolf.WerewolfUtility.IsClean)) != null)
                        {
                            harmony.Patch(
                                AccessTools.Method(typeof(Werewolf.CompWerewolf), "TransformInto"),
                                new HarmonyMethod(
                                    typeof(Werewolf_Patches),
                                    nameof(Werewolf_Patches.TransformInto_Prefix)),
                                null);
                            harmony.Patch(
                                AccessTools.Method(typeof(Werewolf.CompWerewolf), "TransformBack"),
                                null,
                                new HarmonyMethod(
                                    typeof(Werewolf_Patches),
                                    nameof(Werewolf_Patches.TransformBack_Postfix)));
                        }
                    }))();
            }
            catch (TypeLoadException)
            {
            }
        }
    }
}
