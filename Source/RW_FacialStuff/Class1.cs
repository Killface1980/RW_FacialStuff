using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RW_FacialStuff
{
    using RimWorld;

    using UnityEngine;

    using Verse;

    class Class1
    {
        // Verse.PawnRenderer
        private void RenderPawnInternal(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.0046875f;
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !portrait)
                {
                    __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                    }
                    else
                    {
                        mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                    }
                    List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.0046875f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += 0.01875f;
                        woundOverlays.RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.0281250011f;
                vector.y += 0.0234375f;
            }
            else
            {
                a.y += 0.0234375f;
                vector.y += 0.0281250011f;
            }
            if (__instance.graphics.headGraphic != null)
            {
                Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                if (material != null)
                {
                    Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, material, portrait);
                }
                Vector3 loc2 = rootLoc + b;
                loc2.y += 0.0328125022f;
                bool flag = false;
                if (!portrait || !Prefs.HatsOnlyOnMap)
                {
                    Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++)
                    {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                        {
                            flag = true;
                            Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                            material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                            GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material2, portrait);
                        }
                    }
                }
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                {
                    Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat = __instance.graphics.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat, portrait);
                }
            }
            if (renderBody)
            {
                for (int k = 0; k < __instance.graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = __instance.graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material3 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material3 = __instance.graphics.flasher.GetDamagedMat(material3);
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                    }
                }
            }
            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0)
            {
                Graphics.DrawMesh(mesh, vector, quat, __instance.graphics.packGraphic.MatAt(pawn.Rotation, null), 0);
            }
            if (!portrait)
            {
                DrawEquipment(rootLoc);
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int l = 0; l < wornApparel.Count; l++)
                    {
                        wornApparel[l].DrawWornExtras();
                    }
                }
                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += 0.0421875f;
                statusOverlays.RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
            }
        }
        // Verse.PawnRenderer
        private void DrawEquipment(PawnRenderer __instance, Vector3 rootLoc)
        {
            if (pawn.Dead || !pawn.Spawned)
            {
                return;
            }
            if (pawn.equipment == null || pawn.equipment.Primary == null)
            {
                return;
            }
            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon)
            {
                return;
            }
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                Vector3 a;
                if (stance_Busy.focusTarg.HasThing)
                {
                    a = stance_Busy.focusTarg.Thing.DrawPos;
                }
                else
                {
                    a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
                }
                float num = 0f;
                if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (a - pawn.DrawPos).AngleFlat();
                }
                Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                drawLoc.y += 0.0375f;
                __instance.DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num);
            }
            else if (CarryWeaponOpenly())
            {
                if (pawn.Rotation == Rot4.South)
                {
                    Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 0.0375f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, 143f);
                }
                else if (pawn.Rotation == Rot4.North)
                {
                    Vector3 drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
                    drawLoc3.y = drawLoc3.y;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc3, 143f);
                }
                else if (pawn.Rotation == Rot4.East)
                {
                    Vector3 drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    drawLoc4.y += 0.0375f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc4, 143f);
                }
                else if (pawn.Rotation == Rot4.West)
                {
                    Vector3 drawLoc5 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    drawLoc5.y += 0.0375f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc5, 217f);
                }
            }
        }
        // Verse.PawnRenderer
        private bool CarryWeaponOpenly(PawnRenderer __instance)
        {
            return (pawn.carryTracker == null || pawn.carryTracker.CarriedThing == null) && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }

    }
}
