using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Animator
{
    using Verse;

    public class BodyAnimator
    {
        public int swingCounter = 30;

        public bool Finished;

        private Pawn pawn;

        public BodyAnimator(Pawn p)
        {
            this.pawn = p;
        }

        public void AnimatorTick()
        {
            if (this.pawn.pather.Moving || this.swingCounter % 30 != 0)
            {
                //  var speed = this.pawn.mov
                this.swingCounter++;
                if (this.swingCounter > 60)
                {
                    this.swingCounter = 1;
                }
                this.Finished = false;
            }
            else
            {
                this.Finished = true;
            }
        }

    }
}
