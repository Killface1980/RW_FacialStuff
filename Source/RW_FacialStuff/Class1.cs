using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FacialStuff.Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Utilities
{
    // Dummy class to see the changes needed in IL
    class Class1
    {
        private Pawn pawn;
        private PawnDownedWiggler wiggler;
        private PawnGraphicSet graphics;
        private Graphic_Shadow shadowGraphic;

        // Verse.PawnRenderer
        public void RenderPawnAt(Vector3 drawLoc, RotDrawMode bodyDrawType, bool headStump)
        {
            if (!this.graphics.AllResolved)
            {
                this.graphics.ResolveAllGraphics();
            }
            if (this.pawn.GetPosture() == PawnPosture.Standing)
            {
                this.RenderPawnInternal(drawLoc, Quaternion.identity, true, bodyDrawType, headStump);
                if (this.pawn.carryTracker != null)
                {
                    Thing carriedThing = this.pawn.carryTracker.CarriedThing;
                    if (carriedThing != null)
                    {
                        Vector3 vector = drawLoc;
                        bool flag = false;
                        bool flip = false;
                        if (this.pawn.CurJob == null || !this.pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref vector, ref flag, ref flip))
                        {
                            if (carriedThing is Pawn || carriedThing is Corpse)
                            {
                                vector += new Vector3(0.44f, 0f, 0f);
                            }
                            else
                            {
                                vector += new Vector3(0.18f, 0f, 0.05f);
                            }
                        }
                        if (flag)
                        {
                            vector.y -= 0.0390625f;
                        }
                        else
                        {
                            vector.y += 0.0390625f;
                        }
                        carriedThing.DrawAt(vector, flip);
                        //Todo: Maybe change y, kick hands in here
                    }
                }
                if (this.pawn.def.race.specialShadowData != null)
                {
                    if (this.shadowGraphic == null)
                    {
                        this.shadowGraphic = new Graphic_Shadow(this.pawn.def.race.specialShadowData);
                    }
                    this.shadowGraphic.Draw(drawLoc, Rot4.North, this.pawn, 0f);
                }
                if (this.graphics.nakedGraphic != null && this.graphics.nakedGraphic.ShadowGraphic != null)
                {
                    this.graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, this.pawn, 0f);
                }
            }
            else
            {
                Rot4 rot = this.LayingFacing();
                Building_Bed building_Bed = this.pawn.CurrentBed();
                bool renderBody;
                Quaternion quat;
                Vector3 rootLoc;
                if (building_Bed != null && this.pawn.RaceProps.Humanlike)
                {
                    renderBody = building_Bed.def.building.bed_showSleeperBody;
                    Rot4 rotation = building_Bed.Rotation;
                    rotation.AsInt += 2;
                    quat = rotation.AsQuat;
                    AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 15);
                    Vector3 vector2 = this.pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                    Vector3 vector3 = vector2;
                    vector3.y += 0.02734375f;
                    float d = -this.BaseHeadOffsetAt(Rot4.South).z;
                    Vector3 a = rotation.FacingCell.ToVector3();
                    rootLoc = vector2 + a * d;
                    rootLoc.y += 0.0078125f;
                }
                else
                {
                    renderBody = true;
                    rootLoc = drawLoc;
                    if (!this.pawn.Dead && this.pawn.CarriedBy == null)
                    {
                        rootLoc.y = Altitudes.AltitudeFor(AltitudeLayer.LayingPawn) + 0.0078125f;
                    }
                    if (this.pawn.Downed || this.pawn.Dead)
                    {
                        quat = Quaternion.AngleAxis(this.wiggler.downedAngle, Vector3.up);
                    }
                    else if (this.pawn.RaceProps.Humanlike)
                    {
                        quat = rot.AsQuat;
                    }
                    else
                    {
                        Rot4 rot2 = Rot4.West;
                        int num = this.pawn.thingIDNumber % 2;
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
                this.RenderPawnInternal(rootLoc, quat, renderBody, rot, rot, bodyDrawType, false, headStump);
            }
            if (this.pawn.Spawned && !this.pawn.Dead)
            {
                this.pawn.stances.StanceTrackerDraw();
                this.pawn.pather.PatherDraw();
            }
            this.DrawDebug();
        }

        private Rot4 LayingFacing()
        {
            throw new NotImplementedException();
        }

        private Vector3 BaseHeadOffsetAt(Rot4 south)
        {
            throw new NotImplementedException();
        }

        private void DrawDebug()
        {
            throw new NotImplementedException();
        }

        private void RenderPawnInternal(Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 rot1, Rot4 rot2, RotDrawMode bodyDrawType, bool v, bool headStump)
        {
            throw new NotImplementedException();
        }

        private void RenderPawnInternal(Vector3 drawLoc, Quaternion identity, bool p2, RotDrawMode bodyDrawType, bool headStump)
        {
            

        }
    }
}
