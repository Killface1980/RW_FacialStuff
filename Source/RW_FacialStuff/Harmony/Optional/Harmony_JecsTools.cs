using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using Verse;

namespace FacialStuff.Harmony.Optional
{
    [StaticConstructorOnStartup]
    internal static class Harmony_JecsTools
    {
        public static bool jecIsActive;
        private static bool modCheck;

        static Harmony_JecsTools()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.jecstools_patch");
            if (!modCheck)
            {
                foreach (ModContentPack ResolvedMod in LoadedModManager.RunningMods)
                {
                    if (ResolvedMod.Name.Contains("JecsTools"))
                    {
                        jecIsActive = true;
                        break;
                    }
                }

                modCheck = true;
            }
        }
        // CompActivatableEffect.HarmonyCompActivatableEffect
        /// <summary>
        ///     Adds another "layer" to the equipment aiming if they have a
        ///     weapon with a CompActivatableEffect.
        /// </summary>
        /// <param name="eq"></param>
        /// <param name="drawLoc"></param>
        /// <param name="aimAngle"></param>
        /// <param name="pawn"></param>
        /// <param name="__instance"></param>
        public static void DrawEquipmentAimingPostFix(Thing eq, Vector3 drawLoc,
                                                      float aimAngle, Pawn pawn)
        {
            var pawn_EquipmentTracker = pawn.equipment;
            if (pawn_EquipmentTracker != null)
            {
                //Log.Message("2");
                //ThingWithComps thingWithComps = (ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);
                var thingWithComps =
                    pawn_EquipmentTracker
                        .Primary; //(ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

                if (thingWithComps != null)
                {
                    //Log.Message("3");
                    var compActivatableEffect = thingWithComps.GetComp<CompActivatableEffect.CompActivatableEffect>();
                    if (compActivatableEffect != null)
                        if (compActivatableEffect.Graphic != null)
                            if (compActivatableEffect.CurrentState == CompActivatableEffect.CompActivatableEffect.State.Activated)
                            {
                                var num = aimAngle - 90f;
                                var flip = false;

                                if (aimAngle > 20f && aimAngle < 160f)
                                {
                                    //mesh = MeshPool.GridPlaneFlip(thingWithComps.def.graphicData.drawSize);
                                    num += eq.def.equippedAngleOffset;
                                }
                                else if (aimAngle > 200f && aimAngle < 340f)
                                {
                                    //mesh = MeshPool.GridPlane(thingWithComps.def.graphicData.drawSize);
                                    flip = true;
                                    num -= 180f;
                                    num -= eq.def.equippedAngleOffset;
                                }
                                else
                                {
                                    //mesh = MeshPool.GridPlaneFlip(thingWithComps.def.graphicData.drawSize);
                                    num += eq.def.equippedAngleOffset;
                                }

                                if (eq is ThingWithComps eqComps)
                                {
                                    var deflector = eqComps.AllComps.FirstOrDefault(y =>
                                        y.GetType().ToString().Contains("Deflect"));
                                    if (deflector != null)
                                    {
                                        var isActive = (bool)AccessTools
                                            .Property(deflector.GetType(), "IsAnimatingNow").GetValue(deflector, null);
                                        if (isActive)
                                        {
                                            float numMod = (int)AccessTools
                                                .Property(deflector.GetType(), "AnimationDeflectionTicks")
                                                .GetValue(deflector, null);
                                            //float numMod2 = new float();
                                            //numMod2 = numMod;
                                            if (numMod > 0)
                                                if (!flip) num += (numMod + 1) / 2;
                                                else num -= (numMod + 1) / 2;
                                        }
                                    }
                                }
                                num %= 360f;

                                //ThingWithComps eqComps = eq as ThingWithComps;
                                //if (eqComps != null)
                                //{
                                //    ThingComp deflector = eqComps.AllComps.FirstOrDefault<ThingComp>((ThingComp y) => y.GetType().ToString() == "CompDeflector.CompDeflector");
                                //    if (deflector != null)
                                //    {
                                //        float numMod = (float)((int)AccessTools.Property(deflector.GetType(), "AnimationDeflectionTicks").GetValue(deflector, null));
                                //        //Log.ErrorOnce("NumMod " + numMod.ToString(), 1239);
                                //numMod = (numMod + 1) / 2;
                                //if (subtract) num -= numMod;
                                //else num += numMod;
                                //    }
                                //}

                                var matSingle = compActivatableEffect.Graphic.MatSingle;
                                //if (mesh == null) mesh = MeshPool.GridPlane(thingWithComps.def.graphicData.drawSize);

                                var s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                                var matrix = default(Matrix4x4);
                                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), s);
                                if (!flip) Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, 0);
                                else Graphics.DrawMesh(MeshPool.plane10Flip, matrix, matSingle, 0);
                                //Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
                            }
                }
            }
        }
    }
}
