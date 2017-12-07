namespace FacialStuff
{
    using JetBrains.Annotations;

    using Verse;

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


    }
}
