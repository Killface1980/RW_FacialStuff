using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienFace
{
    using System.Reflection;

    using FacialStuff;
    using FacialStuff.FaceEditor;

    using Harmony;

    using Verse;

    using FacialStuff.Harmony;

    using global::AlienRace;

    [StaticConstructorOnStartup]
   public static class HarmonyPatchesAlien
    {
        static HarmonyPatchesAlien()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.alienface.patches");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            harmony.Patch(
                AccessTools.Method(typeof(HarmonyPatchesFS), nameof(HarmonyPatchesFS.OpenStylingWindow)),
                new HarmonyMethod(typeof(HarmonyPatchesAlien), nameof(OpenFSDialog_Prefix)),
                null);
        }

        public static bool OpenFSDialog_Prefix(Pawn pawn)
        {
            if (pawn.def is ThingDef_AlienRace alienProp)
            {
                Find.WindowStack.Add(new Dialog_AlienFaceStyling(pawn, alienProp));
                return false;
            }
            return true;
        }
    }
}
