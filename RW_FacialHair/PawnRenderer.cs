using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;


namespace RW_FacialHair
{
    public class PawnBeardRenderer : PawnRenderer
    {
#pragma warning disable CS0824 // Konstruktor ist extern markiert
        public extern PawnBeardRenderer();
#pragma warning restore CS0824 // Konstruktor ist extern markiert

        private Pawn pawn;

        private PawnWoundDrawer woundOverlays;

        private PawnHeadOverlays statusOverlays;

        public new PawnGraphicHairSet graphics;


      //  public  PawnBeardRenderer(Pawn pawn)
      //  {
      //      this.pawn = pawn;
      //      this.wiggler = new PawnDownedWiggler(pawn);
      //      this.statusOverlays = new PawnHeadOverlays(pawn);
      //      this.woundOverlays = new PawnWoundDrawer(pawn);
      //      this.graphics = new PawnGraphicHairSet(pawn);
      //  }
        private void RenderPawnInternal(Vector3 loc, Quaternion quat, bool renderBody, RotDrawMode draw = RotDrawMode.Fresh)
        {
            this.RenderPawnInternal(loc, quat, renderBody, this.pawn.Rotation, this.pawn.Rotation, draw);
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
                //

                Mesh mesh4_beard = graphics.HairMeshSet.MeshAt(headFacing);
                Material material3_beard = graphics.BeardMatAt(headFacing);
                Graphics.DrawMesh(mesh4_beard, loc + b, quat, material3_beard, 0);
                loc.y += 0.005f;

                List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
                for (int j = 0; j < apparelGraphics.Count; j++)
                {
                    if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        flag = true;
                        Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                        material2 = graphics.flasher.GetDamagedMat(material2);
                        Graphics.DrawMesh(mesh3, loc + b, quat, material2, 0);
                        loc.y += 0.005f;
                    }
                }
                if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = graphics.HairMeshSet.MeshAt(headFacing);
                    Material material3 = graphics.HairMatAt(headFacing);
                    Graphics.DrawMesh(mesh4, loc + b, quat, material3, 0);
                    loc.y += 0.005f;
                }
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

        private bool CarryWeaponOpenly()
        {
            return (this.pawn.carrier == null || this.pawn.carrier.CarriedThing == null) && (this.pawn.Drafted || (this.pawn.CurJob != null && this.pawn.CurJob.def.alwaysShowWeapon) || (this.pawn.mindState.duty != null && this.pawn.mindState.duty.def.alwaysShowWeapon));
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

    }
}
