using FacialStuff.AnimatorWindows;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FacialStuff.Harmony
{
    [HarmonyPatch(
        typeof(PawnRenderer), "RenderPawnAt",
        new[]
            {
            typeof(Vector3), typeof( Rot4), typeof(bool)
            })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    public static class HarmonyPatch_RenderPawnAt
    {
        public const float YOffset_Head = 0.02734375f;

        private const float YOffset_OnHead = 0.03125f;

        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Status = 0.04296875f;

        public const float YOffset_Wounds = 0.01953125f;

        public const float YOffsetInterval_Clothes = 0.00390625f;

        private static FieldInfo PawnHeadOverlaysFieldInfo;

        private static Type PawnRendererType;

        // private static FieldInfo PawnFieldInfo;
        public static bool Prefix(PawnRenderer __instance, ref Vector3 drawLoc, Rot4? rotOverride, bool neverAimWeapon)
        {
            Pawn pawn = __instance.graphics.pawn;
            if (!pawn.RaceProps.Humanlike && !Controller.settings.UsePaws)
            {
                return true;
            }

            CompBodyAnimator compAnim = pawn.GetCompAnim();
            if (compAnim == null)
            {
                return true;
            }

            if (pawn.IsChild() || pawn.GetCompAnim().Deactivated)
                {
                    return true;
                }
            

            Rot4 bodyFacing = rotOverride ?? pawn.Rotation;
            Rot4 headFacing = bodyFacing;
            if (HarmonyPatchesFS.AnimatorIsOpen())
            {
                bodyFacing = MainTabWindow_BaseAnimator.BodyRot;
                headFacing = MainTabWindow_BaseAnimator.HeadRot;
            }

            return true;
        }
    }
}