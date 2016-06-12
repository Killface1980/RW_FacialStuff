using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RW_FacialHair
{
    public class PawnBeardChooser 
    {


        public static BeardDef RandomBeardDefFor(Pawn pawn, FactionDef factionType)
        {
    //      if (_saveableBeard.texPath != null)
    //          return _saveableBeard;

            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs
                                           where beard.hairTags.SharesElementWith(factionType.hairTags)
                                           select beard;

            var chosenBeard = source.RandomElementByWeight((BeardDef beard) => PawnBeardChooser.BeardChoiceLikelihoodFor(beard, pawn));
      //      _saveableBeard = chosenBeard;

        return chosenBeard;
        }

        public static SideburnDef RandomSideburnDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<SideburnDef> source = from sideburn in DefDatabase<SideburnDef>.AllDefs
                                           where sideburn.hairTags.SharesElementWith(factionType.hairTags)
                                           select sideburn;
            return source.RandomElementByWeight((SideburnDef sideburn) => PawnBeardChooser.SideburnChoiceLikelihoodFor(sideburn, pawn));
        }


        public static TacheDef RandomTacheDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<TacheDef> source = from tache in DefDatabase<TacheDef>.AllDefs
                                           where tache.hairTags.SharesElementWith(factionType.hairTags)
                                           select tache;
            return source.RandomElementByWeight((TacheDef tache) => PawnBeardChooser.TacheChoiceLikelihoodFor(tache, pawn));
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


    }
}
