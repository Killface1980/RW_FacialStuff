using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Harmony.Optional
{
    using AlienRace;

    using UnityEngine;

    using Verse;

    public static class Alien_Patches
    {
        public static bool GetPawnMesh(ref Mesh __result, bool portrait, Pawn pawn, Rot4 facing, bool wantsBody)
        {
            if (pawn.def is ThingDef_AlienRace alienProps)
            {
                __result = portrait
                               ? wantsBody
                                     ? alienProps.alienRace.generalSettings.alienPartGenerator.bodyPortraitSet
                                         .MeshAt(facing)
                                     : alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet
                                         .MeshAt(facing)
                               : wantsBody
                                   ? alienProps.alienRace.generalSettings.alienPartGenerator.bodySet.MeshAt(facing)
                                   : alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(facing);
            }
            else
            {
                __result = wantsBody ? MeshPool.humanlikeBodySet.MeshAt(facing) : MeshPool.humanlikeHeadSet.MeshAt(facing);
            }

            return false;
        }

        public static bool GetPawnHairMesh(ref Mesh __result, bool portrait, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics)
        {
            __result = pawn.def is ThingDef_AlienRace alienProps
                           ? (pawn.story.crownType == CrownType.Narrow
                                  ? (portrait
                                         ? alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetNarrow
                                         : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetNarrow)
                                  : (portrait
                                         ? alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetAverage
                                         : alienProps.alienRace.generalSettings.alienPartGenerator.hairSetAverage))
                           .MeshAt(headFacing)
                           : graphics.HairMeshSet.MeshAt(headFacing);
            return false;
        }

        public static void DrawAddons(bool portrait, Pawn pawn, Vector3 vector)
        {

            if (pawn.def is ThingDef_AlienRace alienProps)
            {

                List<BodyAddon> addons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
                AlienComp alienComp = pawn.GetComp<AlienComp>();
                for (int i = 0; i < addons.Count; i++)
                {
                    BodyAddon ba = addons[i];


                    if (ba.CanDrawAddon(pawn))
                    {
                        Mesh mesh = portrait ? ba.addonPortraitMeshFlipped : ba.addonMesh;

                        Rot4 rotation = pawn.Rotation;
                        if (portrait) rotation = Rot4.South;
                        RotationOffset offset = rotation == Rot4.South
                                                    ? ba.offsets.front
                                                    : rotation == Rot4.North
                                                        ? ba.offsets.back
                                                        : ba.offsets.side;

                        // Log.Message("front: " + (offset == ba.offsets.front).ToString() + "\nback: " + (offset == ba.offsets.back).ToString() + "\nside :" + (offset == ba.offsets.side).ToString());
                        Vector2 bodyOffset = offset?.bodyTypes?.FirstOrDefault(to => to.BodyTypeDef == pawn.story.bodyType)
                                                 ?.offset ?? Vector2.zero;
                        Vector2 crownOffset =
                            offset?.crownTypes?.FirstOrDefault(to => to.crownType == alienComp.crownType)?.offset
                            ?? Vector2.zero;

                        // front 0.42f, -0.3f, -0.22f
                        // back     0f,  0.3f, -0.55f
                        // side -0.42f, -0.3f, -0.22f   
                        float MoffsetX = 0.42f;
                        float MoffsetZ = -0.22f;
                        float MoffsetY = ba.inFrontOfBody ? 0.3f : -0.3f;
                        float num = ba.angle;

                        if (rotation == Rot4.North)
                        {
                            MoffsetX = 0f;
                            MoffsetY = !ba.inFrontOfBody ? 0.3f : -0.3f;
                            MoffsetZ = -0.55f;
                            num = 0;
                        }

                        MoffsetX += bodyOffset.x + crownOffset.x;
                        MoffsetZ += bodyOffset.y + crownOffset.y;

                        if (rotation == Rot4.East)
                        {
                            MoffsetX = -MoffsetX;
                            num = -num; // Angle
                            mesh = ba.addonMeshFlipped;
                        }

                        Vector3 scaleVector = new Vector3(MoffsetX, MoffsetY, MoffsetZ);
                        scaleVector.x *= 1f + (1f - (portrait
                                                         ? alienProps.alienRace.generalSettings.alienPartGenerator
                                                             .customPortraitDrawSize
                                                         : alienProps.alienRace.generalSettings.alienPartGenerator
                                                             .customDrawSize).x);
                        scaleVector.z *= 1f + (1f - (portrait
                                                         ? alienProps.alienRace.generalSettings.alienPartGenerator
                                                             .customPortraitDrawSize
                                                         : alienProps.alienRace.generalSettings.alienPartGenerator
                                                             .customDrawSize).y);

                        GenDraw.DrawMeshNowOrLater(
                            mesh,
                            vector + scaleVector,
                            Quaternion.AngleAxis(num, Vector3.up),
                            alienComp.addonGraphics[i].MatAt(rotation),
                            portrait);
                    }
                }
            }
        }

    }
}
