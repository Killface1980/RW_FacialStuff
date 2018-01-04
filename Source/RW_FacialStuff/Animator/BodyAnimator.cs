using Verse;
using Verse.AI;

namespace FacialStuff.Animator
{
    public class BodyAnimator
    {
        // public float cyclePercent = 0;

        // public bool Finished;
        private readonly Pawn _pawn;

        private readonly CompBodyAnimator _compAnim;

        public BodyAnimator(Pawn p, CompBodyAnimator compAnim)
        {
            this._pawn          = p;
            this._compAnim = compAnim;
        }

        // Verse.PawnTweener
        public bool IsMoving(out float movedPercent)
        {
            movedPercent = 0f;

            if (this._compAnim.AnimatorOpen)
            {
                movedPercent = MainTabWindow_Animator.AnimationPercent;
                return MainTabWindow_Animator.isMoving;
            }

            Pawn_PathFollower pather = this._pawn.pather;
            if (pather == null)
            {
                return false;
            }

            if (!pather.Moving)
            {
                return false;
            }

            if (this._pawn.stances.FullBodyBusy)
            {
                return false;
            }

            if (pather.BuildingBlockingNextPathCell() != null)
            {
                return false;
            }

            if (pather.NextCellDoorToManuallyOpen() != null)
            {
                return false;
            }

            if (pather.WillCollideWithPawnOnNextPathCell())
            {
                return false;
            }

            movedPercent = 1f - pather.nextCellCostLeft / pather.nextCellCostTotal;
            return true;
        }

        public void AnimatorTick()
        {
            // if (this.pawn.pather.Moving)
            // {
            // float left = this.pawn.pather.nextCellCostLeft;
            // float total = this.pawn.pather.nextCellCostTotal;
            // this.cyclePercent = 1f - left / total;
            // this.Finished = false;
            // }
            // else
            // {
            // this.Finished = true;
            // }
        }
    }
}