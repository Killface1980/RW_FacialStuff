using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Drawer
{
    using FacialStuff.Defs;

    using Verse;
    using Verse.AI;

    public class QuadrupedDrawer : HumanBipedDrawer
    {
        public override void Tick(Rot4 bodyFacing, PawnGraphicSet graphics)
        {
            base.Tick(bodyFacing, graphics);

            this.isMoving = this.CompAnimator.BodyAnimator.IsMoving(out this.movedPercent);
            var curve = bodyFacing.IsHorizontal ? this.walkCycle.BodyOffsetZ : this.walkCycle.BodyOffsetVerticalZ;
            this.BodyWobble = curve.Evaluate(this.movedPercent);

            if (this.CompAnimator.AnimatorOpen)
            {
                this.walkCycle = this.CompAnimator.walkCycle;
            }
            else if (this.Pawn.CurJob != null)
            {
                // Todo: create cycles
                switch (this.Pawn.CurJob.locomotionUrgency)
                {
                    case LocomotionUrgency.None:
                    case LocomotionUrgency.Amble:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                    case LocomotionUrgency.Walk:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                    case LocomotionUrgency.Jog:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                    case LocomotionUrgency.Sprint:
                        this.walkCycle = WalkCycleDefOf.Quadruped_Walk;
                        break;
                }
            }

        }

    }
}
