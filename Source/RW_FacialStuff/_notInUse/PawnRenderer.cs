using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    public class FS_PawnRenderer : PawnRenderer
    {
        public extern FS_PawnRenderer(Pawn pawn);

        private const float CarriedThingDrawAngle = 16f;

        private const float AltInterval = 0.005f;

        private const int NumAltIntervals = 10;

        private const float UpHeadOffset = 0.34f;

        private Pawn pawn;

        private Graphic_Shadow shadowGraphic;

        private PawnHeadOverlays statusOverlays;

        private PawnWoundDrawer woundOverlays;

        private static readonly float[] HorHeadOffsets = new float[]
        {
            0f,
            0.04f,
            0.1f,
            0.09f,
            0.1f,
            0.09f
        };

        // Verse.PawnRenderer
        private Rot4 LayingFacing()
        {
            if (pawn.GetPosture() == PawnPosture.LayingFaceUp)
            {
                return Rot4.South;
            }
            if (pawn.RaceProps.Humanlike)
            {
                switch (pawn.thingIDNumber % 4)
                {
                    case 0:
                        return Rot4.South;
                    case 1:
                        return Rot4.South;
                    case 2:
                        return Rot4.East;
                    case 3:
                        return Rot4.West;
                }
            }
            else
            {
                switch (pawn.thingIDNumber % 4)
                {
                    case 0:
                        return Rot4.South;
                    case 1:
                        return Rot4.East;
                    case 2:
                        return Rot4.West;
                    case 3:
                        return Rot4.West;
                }
            }
            return Rot4.Random;
        }


        private bool CarryWeaponOpenly()
        {
            return (pawn.carrier == null || pawn.carrier.CarriedThing == null) && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }



        // Verse.PawnRenderer
        private void DrawEquipment(Vector3 rootLoc)
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
            rootLoc.y += 0.0449999981f;
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
                Vector3 b = new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                DrawEquipmentAiming(pawn.equipment.Primary, rootLoc + b, num);
            }
            else if (CarryWeaponOpenly())
            {
                if (pawn.Rotation == Rot4.South)
                {
                    Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, -0.22f);
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
                }
                else if (pawn.Rotation == Rot4.East)
                {
                    Vector3 drawLoc2 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, 143f);
                }
                else if (pawn.Rotation == Rot4.West)
                {
                    Vector3 drawLoc3 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc3, 217f);
                }
            }
        }

        // Verse.PawnRenderer
        public void RenderFacialPawnAt(Vector3 drawLoc, RotDrawMode bodyDrawType = RotDrawMode.Fresh)
        {
            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }
            if (pawn.GetPosture() == PawnPosture.Standing)
            {
                RenderPawnInternal(drawLoc, Quaternion.identity, true, bodyDrawType);
                if (pawn.carrier != null)
                {
                    Thing carriedThing = pawn.carrier.CarriedThing;
                    if (carriedThing != null)
                    {
                        Vector3 vector = drawLoc;
                        vector.y += 0.0449999981f;
                        Vector2 vector2;
                        if (pawn.CurJob != null && pawn.jobs.curDriver.TryGetCarriedThingDrawPos(out vector2))
                        {
                            vector.x = vector2.x;
                            vector.z = vector2.y;
                        }
                        else if (carriedThing is Pawn || carriedThing is Corpse)
                        {
                            vector.x += 0.5f;
                        }
                        else
                        {
                            vector += new Vector3(0.18f, 0f, 0.05f);
                        }
                        carriedThing.DrawAt(vector);
                    }
                }
                if (pawn.def.race.specialShadowData != null)
                {
                    if (shadowGraphic == null)
                    {
                        shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
                    }
                    shadowGraphic.Draw(drawLoc, Rot4.North, pawn);
                }
                if (graphics.nakedGraphic != null && graphics.nakedGraphic.ShadowGraphic != null)
                {
                    graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn);
                }
            }
            else
            {
                Rot4 rot = LayingFacing();
                Building_Bed building_Bed = pawn.CurrentBed();
                bool renderBody;
                Quaternion quat;
                Vector3 loc;
                if (building_Bed != null && pawn.RaceProps.Humanlike)
                {
                    renderBody = building_Bed.def.building.bed_showSleeperBody;
                    Rot4 rotation = building_Bed.Rotation;
                    rotation.AsInt += 2;
                    quat = rotation.AsQuat;
                    AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 12);
                    Vector3 a = pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                    float d = -BaseHeadOffsetAt(Rot4.South).z;
                    Vector3 a2 = rotation.FacingCell.ToVector3();
                    loc = a + a2 * d;
                    loc.y -= 0.005f;
                }
                else
                {
                    renderBody = true;
                    loc = drawLoc;
                    if (pawn.Downed || pawn.Dead)
                    {
                        quat = Quaternion.AngleAxis(wiggler.downedAngle, Vector3.up);
                    }
                    else if (pawn.RaceProps.Humanlike)
                    {
                        quat = rot.AsQuat;
                    }
                    else
                    {
                        Rot4 rot2 = Rot4.West;
                        int num = pawn.thingIDNumber % 2;
                        if (num != 0)
                        {
                            if (num == 1)
                            {
                                rot2 = Rot4.East;
                            }
                        }
                        else
                        {
                            rot2 = Rot4.West;
                        }
                        quat = rot2.AsQuat;
                    }
                }
                RenderPawnInternal(loc, quat, renderBody, rot, rot, bodyDrawType);
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
            }
      //      DrawDebug();
        }

        private void RenderPawnInternal(Vector3 loc, Quaternion quat, bool renderBody, RotDrawMode draw = RotDrawMode.Fresh)
        {
            RenderPawnInternal(loc, quat, renderBody, pawn.Rotation, pawn.Rotation, draw);
        }

        private void RenderPawnInternal(Vector3 loc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = RotDrawMode.Fresh)
        {
            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            if (renderBody)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                }
                else
                {
                    mesh = graphics.nakedGraphic.MeshAt(bodyFacing);
                }
                List<Material> list = graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                for (int i = 0; i < list.Count; i++)
                {
                    Material damagedMat = graphics.flasher.GetDamagedMat(list[i]);
                    Graphics.DrawMesh(mesh, loc, quat, damagedMat, 0);
                    loc.y += 0.005f;
                }
                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    woundOverlays.RenderOverBody(loc, mesh, quat);
                    loc.y += 0.005f;
                }
            }
            float y;
            float y2;
            if (bodyFacing == Rot4.North)
            {
                y = loc.y;
                y2 = loc.y + 0.02f;
            }
            else
            {
                y = loc.y + 0.02f;
                y2 = loc.y;
            }
            loc.y += 0.01f;
            if (graphics.headGraphic != null)
            {
                loc.y = y;
                Vector3 b = quat * BaseHeadOffsetAt(headFacing);
                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                Material material = graphics.HeadMatAt(headFacing, bodyDrawType);
                Graphics.DrawMesh(mesh2, loc + b, quat, material, 0);
                loc.y += 0.005f;
                bool flag = false;
                Mesh mesh3 = graphics.HairMeshSet.MeshAt(headFacing);
                List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
          //    for (int j = 0; j < apparelGraphics.Count; j++)
          //    {
          //        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
          //        {
          //            flag = true;
          //            Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
          //            material2 = graphics.flasher.GetDamagedMat(material2);
          //            Graphics.DrawMesh(mesh3, loc + b, quat, material2, 0);
          //            loc.y += 0.005f;
          //        }
          //    }
          //    if (!flag && bodyDrawType != RotDrawMode.Dessicated)
          //    {
          //        Mesh mesh4 = graphics.HairMeshSet.MeshAt(headFacing);
          //        Material material3 = graphics.HairMatAt(headFacing);
          //        Graphics.DrawMesh(mesh4, loc + b, quat, material3, 0);
          //        loc.y += 0.005f;
          //    }
            }
            if (renderBody)
            {
                loc.y = y2;
                for (int k = 0; k < graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material4 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material4 = graphics.flasher.GetDamagedMat(material4);
                        Graphics.DrawMesh(mesh, loc, quat, material4, 0);
                        loc.y += 0.005f;
                    }
                }
            }
            DrawEquipment(loc);
            loc.y += 0.005f;
            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int l = 0; l < wornApparel.Count; l++)
                {
                    wornApparel[l].DrawWornExtras();
                }
            }
            statusOverlays.RenderStatusOverlays(loc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
        }

    }
}
