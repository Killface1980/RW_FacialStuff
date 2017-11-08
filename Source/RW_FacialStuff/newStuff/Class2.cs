using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.newStuff
{
    using JetBrains.Annotations;

    using Verse;

    public static class Class2
    {
        public static bool GetFace([NotNull] this Pawn pawn, [NotNull] out CompFace face)
        {
            face = pawn.GetComp<CompFace>();
            return face != null;
        }

        public static bool HasFace([CanBeNull] this Pawn pawn)
        {
            CompFace face = pawn?.GetComp<CompFace>();
            return face != null;
        }
    }
}
