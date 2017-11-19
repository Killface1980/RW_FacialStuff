namespace FacialStuff
{
    using JetBrains.Annotations;

    using Verse;

    public static class PawnExtensions
    {
        public static bool GetCompFace([NotNull] this Pawn pawn, [CanBeNull] out CompFace compFace)
        {
            compFace = pawn.GetComp<CompFace>();
            return compFace != null;
        }

        public static bool GetPawnFace([NotNull] this Pawn pawn, [NotNull] out PawnFace pawnFace)
        {
            PawnFace face = pawn.GetComp<CompFace>().PawnFace;
            if (face != null)
            {
                pawnFace = face;
                return true;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            pawnFace = null;

            return false;
        }

        public static bool HasCompFace([CanBeNull] this Pawn pawn)
        {
            CompFace compFace = pawn?.GetComp<CompFace>();
            return compFace != null;
        }

        public static bool HasPawnFace([CanBeNull] this Pawn pawn)
        {
            CompFace compFace = pawn?.GetComp<CompFace>();
            if (compFace != null)
            {
                PawnFace face = compFace.PawnFace;
                if (face != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
