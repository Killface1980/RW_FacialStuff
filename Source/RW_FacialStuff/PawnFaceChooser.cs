using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RW_FacialStuff.Defs;
using Verse;

namespace RW_FacialStuff
{
    public class PawnFaceChooser
    {


        public static BeardDef RandomBeardDefFor(Pawn pawn, FactionDef factionType)
        {


            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs
                                           where beard.hairTags.SharesElementWith(factionType.hairTags)
                                           select beard;

            BeardDef chosenBeard;

            //            if (UnityEngine.Random.Range(30, 50) > pawn.ageTracker.AgeBiologicalYearsFloat)
            if (19 > pawn.ageTracker.AgeBiologicalYearsFloat)
                chosenBeard = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            else
                chosenBeard = source.RandomElementByWeight(beard => BeardChoiceLikelihoodFor(beard, pawn));


            return chosenBeard;
        }

        public static EyeDef RandomEyeDefFor(Pawn pawn, FactionDef factionType)
        {

            IEnumerable<EyeDef> source = from eye in DefDatabase<EyeDef>.AllDefs
                                         where eye.hairTags.SharesElementWith(factionType.hairTags)
                                         select eye;
            EyeDef chosenEyes;

            switch (pawn.story.traits.DegreeOfTrait(TraitDef.Named("NaturalMood")))
            {
                case 2:
                    {
                        var filtered = source.Where(x => x.label.Contains("Nice"));
                        chosenEyes = filtered.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));
                        return chosenEyes;
                    }
                case 1:
                    {
                        var filtered = source.Where(x => x.label.Contains("Aware"));
                        chosenEyes = filtered.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));
                        return chosenEyes;
                    }
                case 0:
                    {
                        var filtered = source.Where(x => !x.label.Contains("Depressed") && !x.label.Contains("Tired"));
                        chosenEyes = filtered.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));
                        return chosenEyes;
                    }
                case -1:
                    {
                        var filtered = source.Where(x => x.label.Contains("Tired"));
                        chosenEyes = filtered.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));
                        return chosenEyes;
                    }
                case -2:
                    {
                        var filtered = source.Where(x => x.label.Contains("Depressed"));
                        chosenEyes = filtered.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));
                        return chosenEyes;
                    }

            }

            chosenEyes = source.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));

            return chosenEyes;
        }

        public static WrinkleDef AssignWrinkleDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<WrinkleDef> source = from wrinkle in DefDatabase<WrinkleDef>.AllDefs
                                             where wrinkle.hairGender.ToString() == pawn.gender.ToString()  //.SharesElementWith(factionType.hairTags)
                                             select wrinkle;

            var chosenWrinkles = source.FirstOrDefault();

            return chosenWrinkles;
        }

        // RimWorld.PawnHairChooser
        public static HairDef RandomHairDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                          where hair.hairTags.SharesElementWith(factionType.hairTags)
                                          select hair;
            return source.RandomElementByWeight(hair => HairChoiceLikelihoodFor(hair, pawn));
        }


        public static LipDef RandomLipDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<LipDef> source = from lip in DefDatabase<LipDef>.AllDefs
                                         where lip.hairTags.SharesElementWith(factionType.hairTags)
                                         select lip;

            LipDef chosenLips = source.RandomElementByWeight(lip => LipChoiceLikelihoodFor(lip, pawn));


            return chosenLips;
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

            Log.Error(string.Concat("Unknown hair likelihood for ", eye, " with ", pawn));
            return 0f;
        }

        private static float LipChoiceLikelihoodFor(LipDef lip, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (lip.hairGender)
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
                switch (lip.hairGender)
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

            Log.Error(string.Concat("Unknown hair likelihood for ", lip, " with ", pawn));
            return 0f;
        }

        private static float BeardChoiceLikelihoodFor(BeardDef beard, Pawn pawn)
        {

            if (pawn.gender == Gender.None)
            {
                return 0f;
            }

            if (beard.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 40)
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

            Log.Error(string.Concat("Unknown hair likelihood for ", beard, " with ", pawn));
            return 0f;
        }

        // RimWorld.PawnHairChooser
        private static float HairChoiceLikelihoodFor(HairDef hair, Pawn pawn)
        {

            if (pawn.gender == Gender.None)
            {
                return 100f;
            }

            if (pawn.gender == Gender.Male)
            {

                if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 40)
                {
                    return 0f;
                }

                if (pawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                {
                    switch (hair.hairGender)
                    {
                        case HairGender.Male:
                            return 10f;
                        case HairGender.MaleUsually:
                            return 5f;
                        case HairGender.Any:
                            return 20f;
                        case HairGender.FemaleUsually:
                            return 40f;
                        case HairGender.Female:
                            return 80f;
                    }
                }

                switch (hair.hairGender)
                {
                    case HairGender.Male:
                        return 80f;
                    case HairGender.MaleUsually:
                        return 40f;
                    case HairGender.Any:
                        return 50f;
                    case HairGender.FemaleUsually:
                        return 0f;
                    case HairGender.Female:
                        return 0f;
                }
            }
            if (pawn.gender == Gender.Female)
            {
                if (hair.hairTags.Contains("MaleOnly"))
                {
                    return 0f;
                }

                if (pawn.story.traits.HasTrait(TraitDef.Named("Gay")))
                {
                    switch (hair.hairGender)
                    {
                        case HairGender.Male:
                            return 80f;
                        case HairGender.MaleUsually:
                            return 40f;
                        case HairGender.Any:
                            return 20f;
                        case HairGender.FemaleUsually:
                            return 5f;
                        case HairGender.Female:
                            return 10f;
                    }
                }

                switch (hair.hairGender)
                {
                    case HairGender.Male:
                        return 0f;
                    case HairGender.MaleUsually:
                        return 0f;
                    case HairGender.Any:
                        return 50f;
                    case HairGender.FemaleUsually:
                        return 40f;
                    case HairGender.Female:
                        return 80f;
                }
            }
            Log.Error(string.Concat("Unknown hair likelihood for ", hair, " with ", pawn));
            return 0f;
        }

    }
}