namespace FacialStuff
{
    using JetBrains.Annotations;

    using RimWorld;

    using Verse;
    using Verse.AI;

    public static class PawnExtensions
    {
        public static bool GetCompFace([NotNull] this Pawn pawn, [NotNull] out CompFace compFace)
        {
            compFace = pawn.GetComp<CompFace>();
            return compFace != null;
        }

        public static bool GetPawnFace([NotNull] this Pawn pawn, [NotNull] out PawnFace pawnFace)
        {
            pawnFace = null;

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return false;
            }

            PawnFace face = compFace.PawnFace;
            if (face != null)
            {
                pawnFace = face;
                return true;
            }

            return false;
        }

        public static bool HasCompFace([CanBeNull] this Pawn pawn)
        {
            CompFace compFace = pawn?.GetComp<CompFace>();
            return compFace != null;
        }

        public static bool HasPawnFace([NotNull] this Pawn pawn)
        {
            if (pawn.GetCompFace(out CompFace compFace))
            {
                PawnFace face = compFace.PawnFace;
                return face != null;
            }

            return false;
        }

        public static bool Fleeing(this Pawn pawn)
        {
            Job job = pawn.CurJob;
            return pawn.MentalStateDef == MentalStateDefOf.PanicFlee
                   || (job != null && (job.def == JobDefOf.Flee || job.def == JobDefOf.FleeAndCower));
        }
}
}
