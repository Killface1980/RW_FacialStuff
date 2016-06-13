using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RW_FacialHair
{
    public class PawnBeardChooser
    {

        public static SideburnDef RandomSideburnDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<SideburnDef> source = from sideburn in DefDatabase<SideburnDef>.AllDefs
                                              where sideburn.hairTags.SharesElementWith(factionType.hairTags)
                                              select sideburn;

            SideburnDef chosenSideburn;
            if (UnityEngine.Random.Range(10, 30) > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenSideburn = DefDatabase<SideburnDef>.GetNamed("Shaved");
            else
                chosenSideburn = source.RandomElementByWeight((SideburnDef sideburn) => PawnBeardChooser.SideburnChoiceLikelihoodFor(sideburn, pawn));
            return chosenSideburn;
        }

        public static TacheDef RandomTacheDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<TacheDef> source = from tache in DefDatabase<TacheDef>.AllDefs
                                           where tache.hairTags.SharesElementWith(factionType.hairTags)
                                           select tache;

            TacheDef chosenTache;
            if (UnityEngine.Random.Range(20, 40) > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenTache = DefDatabase<TacheDef>.GetNamed("Shaved");
            else
                chosenTache = source.RandomElementByWeight((TacheDef tache) => PawnBeardChooser.TacheChoiceLikelihoodFor(tache, pawn));
            return chosenTache;
        }

        public static BeardDef RandomBeardDefFor(Pawn pawn, FactionDef factionType)
        {
            //      if (_saveableBeard.texPath != null)
            //          return _saveableBeard;

            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs
                                           where beard.hairTags.SharesElementWith(factionType.hairTags)
                                           select beard;

            BeardDef chosenBeard;

            if (UnityEngine.Random.Range(30, 50) > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenBeard = DefDatabase<BeardDef>.GetNamed("Shaved");
            else
                chosenBeard = source.RandomElementByWeight((BeardDef beard) => PawnBeardChooser.BeardChoiceLikelihoodFor(beard, pawn));
            //      _saveableBeard = chosenBeard;

            return chosenBeard;
        }


        private static float SideburnChoiceLikelihoodFor(SideburnDef sideburn, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 0f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (sideburn.hairGender)
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
                return 0f;
            }

            Log.Error(string.Concat(new object[]
            {
                "Unknown hair likelihood for ",
                sideburn,
                " with ",
                pawn
            }));
            return 0f;
        }

        private static float TacheChoiceLikelihoodFor(TacheDef tache, Pawn pawn)
        {

            if (pawn.gender == Gender.None)
            {
                return 0f;
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
                return 0f;
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

        private static float BeardChoiceLikelihoodFor(BeardDef beard, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 0f;
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
                return 0f;
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



    }
}