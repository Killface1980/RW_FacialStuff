using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Harmony
{
    using RimWorld;

    using UnityEngine;

    using Verse;

    public static class Class2
    {
        private static Graphic_Shadow shadowGraphic;

        // Verse.PawnRenderer
        public static bool RenderPawnAt(PawnRenderer __instance, Vector3 drawLoc, RotDrawMode bodyDrawType, bool headStump)
        {
            var pawn = __instance.graphics.pawn;
            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }
            if (!pawn.GetCompAnim(out CompBodyAnimator compAnim))
            {
                return true;
            }
            if (pawn.GetPosture() != PawnPosture.Standing)
            {
                return true;
            }
            Thing carriedThing = pawn.carryTracker?.CarriedThing;
            if (carriedThing == null)
            {
                return true;
            }
            HarmonyPatch_PawnRenderer.Prefix(__instance, drawLoc, Quaternion.identity, true,pawn.Rotation, pawn.Rotation, bodyDrawType, false, headStump);
            Vector3 loc = drawLoc;
            bool behind = false;
            bool flip = false;
            if (pawn.CurJob == null
                || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref loc, ref behind, ref flip))
            {
                if (carriedThing is Pawn || carriedThing is Corpse)
                {
                    loc += new Vector3(0.44f, 0f, 0f);
                }
                else
                {
                    loc += new Vector3(0.18f, 0f, 0.05f);
                }
            }
            if (pawn.Rotation == Rot4.North)
            {
                behind = true;
            }
            if (behind)
            {
                loc.y -= HarmonyPatch_PawnRenderer.YOffset_CarriedThing;
            }
            else
            {
                loc.y += HarmonyPatch_PawnRenderer.YOffset_CarriedThing;
            }
            carriedThing.DrawAt(loc, flip);

            bool showHands = compAnim.Props.bipedWithHands && Controller.settings.UseHands;
            if (showHands)
            {
                compAnim.DrawHands(loc, false, true);
            }


            if (pawn.def.race.specialShadowData != null)
            {
                if (shadowGraphic == null)
                {
                    shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
                }
                shadowGraphic.Draw(drawLoc, Rot4.North, pawn, 0f);
            }
            if (__instance.graphics.nakedGraphic != null && __instance.graphics.nakedGraphic.ShadowGraphic != null)
            {
                __instance.graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn, 0f);
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
            }
            //  __instance.DrawDebug();
            return false;
        }

    }
}
