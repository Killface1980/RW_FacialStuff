using System;
using System.Linq;
using System.Reflection;
using FacialStuff;
using HarmonyLib;
using ProjectJedi;
using UnityEngine;
using Verse;

namespace LightSabers
{
    [StaticConstructorOnStartup]
    internal static class Harmony_Lightsabers
    {


        private static readonly bool modCheck;
        private static readonly bool loadedJedi;

        static Harmony_Lightsabers()
        {
            var harmony = new Harmony("rimworld.facialstuff.jecstools_lightsabers");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (!modCheck)
            {
                loadedJedi = false;
                foreach (ModContentPack ResolvedMod in LoadedModManager.RunningMods)
                {
                    if (loadedJedi) break; //Save some loading
                    if (ResolvedMod.Name.Contains("Lightsabers"))
                    {
                        Log.Message("FS :: Lightsabers Detected.");
                        loadedJedi = true;
                    }
                }
                modCheck = true;
            }


            if (loadedJedi)
            {
                try
                {
                    ((Action) (() =>
                               {
                                   harmony.Patch(
                                                 AccessTools.Method(typeof(HumanBipedDrawer),
                                                                    nameof(HumanBipedDrawer.DrawEquipment)),
                                                 null,
                                                 new HarmonyMethod(typeof(Harmony_Lightsabers),
                                                                   nameof(DrawEquipment_PostFix)));
                               }))();
                }
                catch (Exception e)
                {
                }
            }

        }

        // ProjectJedi
        public static void DrawEquipment_PostFix(HumanBipedDrawer __instance, Vector3 rootLoc, bool portrait)
        {
            Pawn pawn = __instance.Pawn;
            if (pawn.health?.hediffSet?.hediffs == null || pawn.health.hediffSet.hediffs.Count <= 0)
            {
                return;
            }

            Hediff shieldHediff =
            pawn.health.hediffSet.hediffs.FirstOrDefault(x =>
                                                             x.TryGetComp<HediffComp_Shield>() != null);

            HediffComp_Shield shield = shieldHediff?.TryGetComp<HediffComp_Shield>();
            shield?.DrawWornExtras();
        }
    }
}