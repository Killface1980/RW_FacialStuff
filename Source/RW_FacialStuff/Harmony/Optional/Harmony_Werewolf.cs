using System;
using HarmonyLib;
using Verse;
using Werewolf;

namespace FacialStuff.Harmony.Optional
{
    [StaticConstructorOnStartup]
    internal static class Harmony_Werewolf
    {
        static Harmony_Werewolf()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("rimworld.facialstuff.werewolf_patch");

            try
            {
                ((Action)(() =>
                    {
                        if (AccessTools.Method(
                                typeof(WerewolfUtility),
                                nameof(WerewolfUtility.IsClean)) == null)
                        {
                            return;
                        }

                        harmony.Patch(
                            AccessTools.Method(typeof(CompWerewolf), nameof(CompWerewolf.TransformInto)),
                            new HarmonyMethod(typeof(Werewolf_Patches), nameof(Werewolf_Patches.TransformInto_Prefix)),
                            null);

                        harmony.Patch(
                            AccessTools.Method(typeof(CompWerewolf), nameof(CompWerewolf.TransformBack)),
                            null,
                            new HarmonyMethod(
                                typeof(Werewolf_Patches),
                                nameof(Werewolf_Patches.TransformBack_Postfix)));
                    }))();
            }
            catch (TypeLoadException)
            {
            }
        }
    }
}