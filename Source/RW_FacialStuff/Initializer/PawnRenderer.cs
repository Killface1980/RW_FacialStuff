using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;

namespace RW_FacialStuff
{
    public class PawnRenderer
    {
        private const float CarriedThingDrawAngle = 16f;

        private const float AltInterval = 0.005f;

        private const int NumAltIntervals = 10;

        private const float UpHeadOffset = 0.34f;

        private Pawn pawn;
        public PawnDownedWiggler wiggler;
        public PawnGraphicSetModded graphicsFS;

        private PawnHeadOverlays statusOverlays;

        private PawnWoundDrawer woundOverlays;

        private Graphic_Shadow shadowGraphic;

        private static readonly float[] HorHeadOffsets = {
            0f,
            0.04f,
            0.1f,
            0.09f,
            0.1f,
            0.09f
        };
        [Detour(typeof(Verse.PawnRenderer), bindingFlags = (BindingFlags.Instance | BindingFlags.NonPublic))]
        public PawnRenderer(Pawn pawn)
        {
            this.pawn = pawn;
            wiggler = new PawnDownedWiggler(pawn);
            statusOverlays = new PawnHeadOverlays(pawn);
            woundOverlays = new PawnWoundDrawer(pawn);
            graphicsFS = new PawnGraphicSetModded(pawn);
        }

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

        public void RenderPawnAt(Vector3 drawLoc, RotDrawMode bodyDrawType = RotDrawMode.Fresh)
        {
            if (!graphicsFS.AllResolved)
            {
                graphicsFS.ResolveAllGraphics();
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
                if (graphicsFS.nakedGraphic != null && graphicsFS.nakedGraphic.ShadowGraphic != null)
                {
                    graphicsFS.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn);
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
                    AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 14);
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
                    if (!pawn.Dead)
                    {
                        loc.y = Altitudes.AltitudeFor(AltitudeLayer.LayingPawn);
                    }
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
                RenderPawnInternal(loc, quat, renderBody, rot, rot, bodyDrawType, false);
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
            }
        }

        private void RenderPawnInternal(Vector3 loc, Quaternion quat, bool renderBody, RotDrawMode draw = RotDrawMode.Fresh)
        {
            RenderPawnInternal(loc, quat, renderBody, pawn.Rotation, pawn.Rotation, draw, false);
        }

        private void RenderPawnInternal(Vector3 loc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = RotDrawMode.Fresh, bool portrait = false)
        {
            if (!graphicsFS.AllResolved)
            {
                graphicsFS.ResolveAllGraphics();
            }
            Mesh bodyMesh = null;
            if (renderBody)
            {
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && graphicsFS.dessicatedGraphic != null && !portrait)
                {
                    graphicsFS.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        bodyMesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                    }
                    else
                    {
                        bodyMesh = graphicsFS.nakedGraphic.MeshAt(bodyFacing);
                    }
                    List<Material> list = graphicsFS.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = graphicsFS.flasher.GetDamagedMat(list[i]);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.005f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        woundOverlays.RenderOverBody(loc, bodyMesh, quat, portrait);
                        loc.y += 0.005f;
                    }
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
            if (graphicsFS.headGraphic != null)
            {
                var pawnSave = MapComponent_FacialStuff.GetCache(pawn);

                loc.y = y;
                Vector3 b = quat * BaseHeadOffsetAt(headFacing);
                Mesh headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                Material headMatAt = graphicsFS.HeadMatAt(headFacing, bodyDrawType);
                GenDraw.DrawMeshNowOrLater(headMesh, loc + b, quat, headMatAt, portrait);
                loc.y += 0.005f;
                if (pawnSave.optimized)
                {
           //       //
           //       //    // Eyes
           //       Mesh mesh401 = graphicsFS.EyeMeshSet.MeshAt(headFacing);
           //       //
           //       Material mat200 = graphicsFS.EyeMatAt(headFacing);
           //       GenDraw.DrawMeshNowOrLater(mesh401, loc + b, quat, mat200, portrait);
           //           loc.y += 0.005f;
           //       //
           //     //mat200 = BrowMatAt(headFacing);
           //     //GenDraw.DrawMeshNowOrLater(mesh401, loc + b, quat, mat200, portrait);
           //       //    loc.y += 0.005f;
           //
                }
                bool flag = false;
                List<ApparelGraphicRecord> apparelGraphics = graphicsFS.apparelGraphics;
                Building_Bed building_Bed = pawn.CurrentBed();
                for (int j = 0; j < apparelGraphics.Count; j++)
                {
                    if (building_Bed == null)
                    {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                        {
                            flag = true;

                            Material apparelMatAt = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                            apparelMatAt = graphicsFS.flasher.GetDamagedMat(apparelMatAt);
                            Mesh apparelMeshAt = graphicsFS.HairMeshSet.MeshAt(headFacing);
                            GenDraw.DrawMeshNowOrLater(apparelMeshAt, loc + b, quat, apparelMatAt, portrait);
                            loc.y += 0.005f;
                        }
                    }
                }
                if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = graphicsFS.HairMeshSet.MeshAt(headFacing);
                    Material mat2 = graphicsFS.HairMatAt(headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh4, loc + b, quat, mat2, portrait);
                    loc.y += 0.005f;
                }
            }
            if (renderBody)
            {
                loc.y = y2;
                for (int k = 0; k < graphicsFS.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = graphicsFS.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material2 = graphicsFS.flasher.GetDamagedMat(material2);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, material2, portrait);
                        loc.y += 0.005f;
                    }
                }
            }
            if (!portrait)
            {
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
        public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            float num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material matSingle;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }
            Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
        }
        public Vector3 BaseHeadOffsetAt(Rot4 rotation)
        {
            float num = PawnRenderer.HorHeadOffsets[(int)this.pawn.story.BodyType];
            switch (rotation.AsInt)
            {
                case 0:
                    return new Vector3(0f, 0f, 0.34f);
                case 1:
                    return new Vector3(num, 0f, 0.34f);
                case 2:
                    return new Vector3(0f, 0f, 0.34f);
                case 3:
                    return new Vector3(-num, 0f, 0.34f);
                default:
                    Log.Error("BaseHeadOffsetAt error in " + this.pawn);
                    return Vector3.zero;
            }
        }
        private bool CarryWeaponOpenly()
        {
            return (pawn.carrier == null || pawn.carrier.CarriedThing == null) && (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }

    }
}
