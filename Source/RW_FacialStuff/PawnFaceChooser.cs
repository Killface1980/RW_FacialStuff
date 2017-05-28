using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RW_FacialStuff.Defs;
using Verse;

namespace RW_FacialStuff
{
    public static class PawnFaceChooser
    {


        public static BeardDef RandomBeardDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs
                                           where beard.raceList.Contains(pawn.def)
                                           where beard.hairTags.SharesElementWith(factionType.hairTags)
                                           select beard;

            if (!source.Any())
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs
                         select beard;
            }

            BeardDef chosenBeard;
            float rand = Rand.Value;
            //            if (UnityEngine.Random.Range(30, 50) > pawn.ageTracker.AgeBiologicalYearsFloat)
            if (pawn.ageTracker.AgeBiologicalYearsFloat < 19|| rand < 0.1f || pawn.gender == Gender.Female)
            {
                chosenBeard = DefDatabase<BeardDef>.GetNamed("Beard_Shaved");
            }
            else if (rand < 0.25f)
            {
                chosenBeard = DefDatabase<BeardDef>.GetNamed("Beard_Stubble");
            }

            else
                chosenBeard = source.RandomElementByWeight(beard => BeardChoiceLikelihoodFor(beard, pawn));

            return chosenBeard;
        }

        public static EyeDef RandomEyeDefFor(Pawn pawn, FactionDef factionType)
        {

            IEnumerable<EyeDef> source = from eye in DefDatabase<EyeDef>.AllDefs
                                         where eye.raceList.Contains(pawn.def)
                                         where eye.hairTags.SharesElementWith(factionType.hairTags)
                                         select eye;

            if (!source.Any())
            {
                source = from eye in DefDatabase<EyeDef>.AllDefs
                         select eye;
            }

            EyeDef chosenEyes;

            chosenEyes = source.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));

            return chosenEyes;
        }

        public static BrowDef RandomBrowDefFor(Pawn pawn, FactionDef factionType)
        {

            IEnumerable<BrowDef> source = from brow in DefDatabase<BrowDef>.AllDefs
                                          where brow.raceList.Contains(pawn.def)
                                          where brow.hairTags.SharesElementWith(factionType.hairTags)
                                          select brow;

            if (!source.Any())
            {
                source = from brow in DefDatabase<BrowDef>.AllDefs
                         select brow;
            }
            BrowDef chosenBrows;

            switch (pawn.story.traits.DegreeOfTrait(TraitDef.Named("NaturalMood")))
            {
                case 2:
                    {
                        IEnumerable<BrowDef> filtered = source.Where(x => x.label.Contains("Nice"));
                        chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                        return chosenBrows;
                    }
                case 1:
                    {
                        IEnumerable<BrowDef> filtered = source.Where(x => x.label.Contains("Aware"));
                        chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                        return chosenBrows;
                    }
                case 0:
                    {
                        IEnumerable<BrowDef> filtered = source.Where(x => !x.label.Contains("Depressed") && !x.label.Contains("Tired"));
                        chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                        return chosenBrows;
                    }
                case -1:
                    {
                        IEnumerable<BrowDef> filtered = source.Where(x => x.label.Contains("Tired"));
                        chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                        return chosenBrows;
                    }
                case -2:
                    {
                        IEnumerable<BrowDef> filtered = source.Where(x => x.label.Contains("Depressed"));
                        chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                        return chosenBrows;
                    }

            }

            chosenBrows = source.RandomElementByWeight(brow => BrowChoiceLikelihoodFor(brow, pawn));

            return chosenBrows;
        }

        public static WrinkleDef AssignWrinkleDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<WrinkleDef> source = from wrinkle in DefDatabase<WrinkleDef>.AllDefs
                                             where wrinkle.raceList.Contains(pawn.def)
                                             where wrinkle.hairGender.ToString() == pawn.gender.ToString()  //.SharesElementWith(factionType.hairTags)
                                             select wrinkle;

            WrinkleDef chosenWrinkles = source.FirstOrDefault();

            return chosenWrinkles;
        }

        [Detour(typeof(PawnHairChooser), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        public static HairDef RandomHairDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                          where hair.hairTags.SharesElementWith(factionType.hairTags)
                                          select hair;

            return source.RandomElementByWeight(hair => HairChoiceLikelihoodFor(hair, pawn));
        }

        public static MouthDef RandomMouthDefFor(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<MouthDef> source = from mouthDef in DefDatabase<MouthDef>.AllDefs
                                           where mouthDef.raceList.Contains(pawn.def)
                                           where mouthDef.hairTags.SharesElementWith(factionType.hairTags)
                                           select mouthDef;

            if (!source.Any())
            {
                source = from mouthDef in DefDatabase<MouthDef>.AllDefs
                         select mouthDef;
            }

            MouthDef chosenMouths;

            switch (pawn.story.traits.DegreeOfTrait(TraitDef.Named("NaturalMood")))
            {
                case 2:
                    {
                        IEnumerable<MouthDef> filtered = source.Where(x => x.label.Contains("Smile"));
                        chosenMouths = filtered.RandomElementByWeight(mouthDef => MouthChoiceLikelihoodFor(mouthDef, pawn));
                        return chosenMouths;
                    }
                case 1:
                    {
                        IEnumerable<MouthDef> filtered = source.Where(x => x.label.Contains("Default"));
                        chosenMouths = filtered.RandomElementByWeight(mouthDef => MouthChoiceLikelihoodFor(mouthDef, pawn));
                        return chosenMouths;
                    }
                case 0:
                    {
                        IEnumerable<MouthDef> filtered = source.Where(x => !x.label.Contains("Default") && !x.label.Contains("Smile") && !x.label.Contains("Big") && !x.label.Contains("Sad"));
                        chosenMouths = filtered.RandomElementByWeight(mouthDef => MouthChoiceLikelihoodFor(mouthDef, pawn));
                        return chosenMouths;
                    }

                case -1:
                    {
                        IEnumerable<MouthDef> filtered = source.Where(x => x.label.Contains("Big"));
                        chosenMouths = filtered.RandomElementByWeight(mouthDef => MouthChoiceLikelihoodFor(mouthDef, pawn));
                        return chosenMouths;
                    }
                case -2:
                    {
                        IEnumerable<MouthDef> filtered = source.Where(x => x.label.Contains("Sad"));
                        chosenMouths = filtered.RandomElementByWeight(mouthDef => MouthChoiceLikelihoodFor(mouthDef, pawn));
                        return chosenMouths;
                    }
            }
            chosenMouths = source.RandomElementByWeight(mouthDef => MouthChoiceLikelihoodFor(mouthDef, pawn));
            return chosenMouths;


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

        private static float BrowChoiceLikelihoodFor(BrowDef brow, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (brow.hairGender)
                {
                    case HairGender.Male:
                        return 70f;
                    case HairGender.MaleUsually:
                        return 30f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.FemaleUsually:
                        return 0f;
                    case HairGender.Female:
                        return 0f;
                }
            }

            if (pawn.gender == Gender.Female)
            {
                switch (brow.hairGender)
                {
                    case HairGender.Female:
                        return 70f;
                    case HairGender.FemaleUsually:
                        return 30f;
                    case HairGender.Any:
                        return 60f;
                    case HairGender.MaleUsually:
                        return 0f;
                    case HairGender.Male:
                        return 0f;
                }
            }

            Log.Error(string.Concat("Unknown hair likelihood for ", brow, " with ", pawn));
            return 0f;
        }

        private static float MouthChoiceLikelihoodFor(MouthDef mouth, Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }
            if (pawn.gender == Gender.Male)
            {
                switch (mouth.hairGender)
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
                switch (mouth.hairGender)
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

            Log.Error(string.Concat("Unknown hair likelihood for ", mouth, " with ", pawn));
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