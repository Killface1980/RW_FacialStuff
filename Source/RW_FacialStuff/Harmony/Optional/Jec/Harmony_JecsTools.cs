using System;
using System.Linq;
using System.Reflection;
using FacialStuff;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace LightSabers
{
    [StaticConstructorOnStartup]
    internal static class Harmony_JecsTools
    {
        private static readonly bool modCheck;
        private static readonly bool loadedJec;

        static Harmony_JecsTools()
        {
            var harmony = new Harmony("rimworld.facialstuff.jecstools_patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (!modCheck)
            {
                loadedJec = false;
                foreach (ModContentPack ResolvedMod in LoadedModManager.RunningMods)
                {
                    if (loadedJec) break; //Save some loading
                    if (ResolvedMod.Name.Contains("JecsTools"))
                    {
                        Log.Message("FS :: JecsTools Detected.");
                        loadedJec = true;
                    }
                }
                modCheck = true;
            }


            if (loadedJec)
            {
                try
                {
                    ((Action)(() =>
                              {
                                  harmony.Patch(
                                                AccessTools.Method(typeof(HumanBipedDrawer),
                                                                   nameof(HumanBipedDrawer.DrawEquipmentAiming)),
                                                new HarmonyMethod(typeof(Harmony_JecsTools),
                                                                  nameof(DrawEquipmentAimingPreFix)),
                                                new HarmonyMethod(typeof(Harmony_JecsTools),
                                                                  nameof(DrawEquipmentAimingPostFix)));
                              }))();

                }



                catch (Exception e)
                {
                }
            };
        }

        /// <summary>
        ///     Adds another "layer" to the equipment aiming if they have a
        ///     weapon with a CompActivatableEffect.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="eq"></param>
        /// <param name="drawLoc"></param>
        /// <param name="aimAngle"></param>
        public static bool DrawEquipmentAimingPreFix(HumanBipedDrawer __instance, Thing equipment,
                                                     ref Vector3 weaponDrawLoc,
                                                     Vector3 rootLoc,
                                                     ref float aimAngle,
                                                     bool portrait, ref bool flipped)
        {
            if (equipment is ThingWithComps thingWithComps)
            {
                //If the deflector is active, it's already using this code.
                var deflector = thingWithComps.AllComps.FirstOrDefault(y =>
                    y.GetType().ToString() == "CompDeflector.CompDeflector" ||
                    y.GetType().BaseType?.ToString() == "CompDeflector.CompDeflector");
                if (deflector != null)
                {
                    var isAnimatingNow = Traverse.Create(deflector).Property("IsAnimatingNow").GetValue<bool>();
                    if (isAnimatingNow)
                    {
                        return false;
                    }
                }

                //  var compOversizedWeapon = thingWithComps.TryGetComp<CompOversizedWeapon.CompOversizedWeapon>();
                if (equipment.Graphic.drawSize != Vector2.one)
                {
                    var num = aimAngle - 90f;
                    Mesh mesh;
                    if (aimAngle > 20f && aimAngle < 160f)
                    {
                        mesh = MeshPool.plane10;
                        num += equipment.def.equippedAngleOffset;
                    }
                    else if (aimAngle > 200f && aimAngle < 340f)
                    {
                        mesh = MeshPool.plane10Flip;
                        num -= 180f;
                        num -= equipment.def.equippedAngleOffset;
                    }
                    else
                    {
                        mesh = MeshPool.plane10;
                        num += equipment.def.equippedAngleOffset;
                    }
                    num %= 360f;
                    var graphic_StackCount = equipment.Graphic as Graphic_StackCount;
                    Material matSingle;
                    if (graphic_StackCount != null)
                        matSingle = graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle;
                    else
                        matSingle = equipment.Graphic.MatSingle;

                    var s = new Vector3(equipment.def.graphicData.drawSize.x, 1f, equipment.def.graphicData.drawSize.y);
                    var matrix = default(Matrix4x4);
                    matrix.SetTRS(weaponDrawLoc, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh(mesh, matrix, matSingle, 0);

                    __instance.CalculateHandsAiming(weaponDrawLoc, flipped, aimAngle, null);
                    return false;
                }
            }
            //}
            return true;
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
        public static void DrawEquipmentAimingPostFix(HumanBipedDrawer __instance, Thing equipment,
                                                      ref Vector3 weaponDrawLoc,
                                                      ref float aimAngle,
                                                      bool portrait, ref bool flipped)
        {
            Pawn pawn = __instance.Pawn;

            ThingWithComps primary = pawn.equipment?.Primary;
            //Log.Message("2");
            //ThingWithComps thingWithComps = (ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);
            //(ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

            //Log.Message("3");
            CompActivatableEffect.CompActivatableEffect compActivatableEffect =
            primary?.GetComp<CompActivatableEffect.CompActivatableEffect>();
            if (compActivatableEffect?.Graphic == null)
            {
                return;
            }

            if (compActivatableEffect.CurrentState !=
                CompActivatableEffect.CompActivatableEffect.State.Activated)
            {
                return;
            }

            if (equipment is ThingWithComps eqComps)
            {
                ThingComp deflector = eqComps.AllComps.FirstOrDefault(y =>
                                                                    y.GetType().ToString().Contains("Deflect"));
                if (deflector != null)
                {
                    bool isActive = (bool)AccessTools
                                         .Property(deflector.GetType(), "IsAnimatingNow").GetValue(deflector, null);
                    if (isActive)
                    {
                        float numMod = (int)AccessTools
                                            .Property(deflector.GetType(), "AnimationDeflectionTicks")
                                            .GetValue(deflector, null);
                        //float numMod2 = new float();
                        //numMod2 = numMod;
                        if (numMod > 0)
                        {
                            if (!flipped)
                            {
                                aimAngle += (numMod + 1) / 2;
                            }
                            else
                            {
                                aimAngle -= (numMod + 1) / 2;
                            }
                        }
                    }
                }
            }

            aimAngle %= 360f;

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

            Material matSingle = compActivatableEffect.Graphic.MatSingle;
            //if (mesh == null) mesh = MeshPool.GridPlane(thingWithComps.def.graphicData.drawSize);

            Vector3 s = new Vector3(equipment.def.graphicData.drawSize.x, 1f, equipment.def.graphicData.drawSize.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(weaponDrawLoc, Quaternion.AngleAxis(aimAngle, Vector3.up), s);
            Graphics.DrawMesh(!flipped ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);

            //Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
        }
    }
}