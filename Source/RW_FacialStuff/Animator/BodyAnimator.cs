using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Animator
{
    using Verse;

    public class BodyAnimator
    {
        public float cyclePercent = 0;

        public bool Finished;

        private Pawn pawn;

        public BodyAnimator(Pawn p)
        {
            this.pawn = p;
        }

        public void AnimatorTick()
        {
            if (this.pawn.pather.Moving)
            {
                float left = this.pawn.pather.nextCellCostLeft;
                float total = this.pawn.pather.nextCellCostTotal;
                this.cyclePercent = left / total;

                this.Finished = false;
            }
            else
            {
                this.Finished = true;
            }
        }

    }
}
