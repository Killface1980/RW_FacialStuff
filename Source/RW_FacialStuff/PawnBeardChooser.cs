using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RW_FacialStuff
{
    public class PawnFaceChooser
    {

        public static SideburnDef RandomSideburnDefFor(Pawn pawn, FactionDef factionType)
        {
            SideburnDef chosenSideburn;

            IEnumerable<SideburnDef> source = from sideburn in DefDatabase<SideburnDef>.AllDefs
                                              where sideburn.hairTags.SharesElementWith(factionType.hairTags) && sideburn.crownType.Equals(pawn.story.crownType)
                                              select sideburn;

            if (UnityEngine.Random.Range(10, 30) > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenSideburn = DefDatabase<SideburnDef>.GetNamed("Sideburn_Average_Shaved");
            else
            {
                chosenSideburn = source.RandomElementByWeight((SideburnDef sideburn) => SideburnChoiceLikelihoodFor(sideburn, pawn));
            }
            return chosenSideburn;

        }

        public static TacheDef RandomTacheDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<TacheDef> source = from tache in DefDatabase<TacheDef>.AllDefs
                                           where tache.hairTags.SharesElementWith(factionType.hairTags) && tache.crownType.Equals(pawn.story.crownType)
                                           select tache;

            TacheDef chosenTache;
            if (UnityEngine.Random.Range(20, 40) > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenTache = DefDatabase<TacheDef>.GetNamed("Moustache_Average_Shaved");
            else
                chosenTache = source.RandomElementByWeight((TacheDef tache) => TacheChoiceLikelihoodFor(tache, pawn));

            return chosenTache;
        }

        public static BeardDef RandomBeardDefFor(Pawn pawn, FactionDef factionType)
        {


            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs
                                           where beard.hairTags.SharesElementWith(factionType.hairTags) && beard.crownType.Equals(pawn.story.crownType)
                                           select beard;

            BeardDef chosenBeard;

            if (UnityEngine.Random.Range(30, 50) > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenBeard = DefDatabase<BeardDef>.GetNamed("Beard_Average_Shaved");
            else
                chosenBeard = source.RandomElementByWeight((BeardDef beard) => BeardChoiceLikelihoodFor(beard, pawn));


            return chosenBeard;
        }

        public static EyeDef RandomEyeDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<EyeDef> source = from eye in DefDatabase<EyeDef>.AllDefs
                                         where eye.hairTags.SharesElementWith(factionType.hairTags) && eye.crownType.Equals(pawn.story.crownType)
                                         select eye;

            EyeDef chosenEyes;

            chosenEyes = source.RandomElementByWeight((EyeDef eye) => EyeChoiceLikelihoodFor(eye, pawn));


            return chosenEyes;
        }

        private static float EyeChoiceLikelihoodFor(EyeDef eye, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (eye.hairGender)
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
                switch (eye.hairGender)
                {
                    case HairGender.Female:
                        return 70f;
                    case HairGender.FemaleUsually:
                        return 30f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.MaleUsually:
                        return 5f;
                    case HairGender.Male:
                        return 1f;
                }
            }

            Log.Error(string.Concat(new object[]
            {
               "Unknown hair likelihood for ",
               eye,
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