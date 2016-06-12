using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RW_FacialHair
{
    public static class PawnBeardChooser
    {
        public static BeardDef RandomBeardDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs
                                           where beard.BeardTags.SharesElementWith(factionType.hairTags)
                                           select beard;
            return source.RandomElementByWeight((BeardDef beard) => PawnBeardChooser.BeardChoiceLikelihoodFor(beard, pawn));
        }

        public static TacheDef RandomTacheDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<TacheDef> source = from tache in DefDatabase<TacheDef>.AllDefs
                                           where tache.BeardTags.SharesElementWith(factionType.hairTags)
                                           select tache;
            return source.RandomElementByWeight((TacheDef tache) => PawnBeardChooser.TacheChoiceLikelihoodFor(tache, pawn));
        }

        private static float BeardChoiceLikelihoodFor(BeardDef beard, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (beard.hairGender)
                {
                    case HairGender.Male:
                        return 70f;
                    case HairGender.MaleUsually:
                        return 30f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.FemaleUsually:
                        return 5f;
                    case HairGender.Female:
                        return 1f;
                }
            }
            if (pawn.gender == Gender.Female)
            {
                switch (beard.hairGender)
                {
                    case HairGender.Male:
                        return 1f;
                    case HairGender.MaleUsually:
                        return 5f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.FemaleUsually:
                        return 30f;
                    case HairGender.Female:
                        return 70f;
                }
            }
            Log.Error(string.Concat(new object[]
            {
                "Unknown hair likelihood for ",
                beard,
                " with ",
                pawn
            }));
            return 0f;
        }

        private static float TacheChoiceLikelihoodFor(TacheDef tache, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (tache.hairGender)
                {
                    case HairGender.Male:
                        return 70f;
                    case HairGender.MaleUsually:
                        return 30f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.FemaleUsually:
                        return 5f;
                    case HairGender.Female:
                        return 1f;
                }
            }
            if (pawn.gender == Gender.Female)
            {
                switch (tache.hairGender)
                {
                    case HairGender.Male:
                        return 1f;
                    case HairGender.MaleUsually:
                        return 5f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.FemaleUsually:
                        return 30f;
                    case HairGender.Female:
                        return 70f;
                }
            }
            Log.Error(string.Concat(new object[]
            {
                "Unknown hair likelihood for ",
                tache,
                " with ",
                pawn
            }));
            return 0f;
        }

    }
}
