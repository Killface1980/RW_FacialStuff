using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using static FacialStuff.Offsets;

namespace FacialStuff.Harmony
{
    // Not working, no idea what it is for.
    //[HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawPos), MethodType.Getter)]
    public static class DrawPos_Patch
    {
        public static Vector3 offset = Vector3.zero;
        public static bool offsetEnabled = false;

        public static void Postfix(ref Vector3 __result)
        {
            if (offsetEnabled)
            {
                __result = offset;
            }
        }
    }

    // ReSharper disable once InconsistentNaming

}