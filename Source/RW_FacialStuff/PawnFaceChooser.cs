using System.Collections.Generic;
using System.Linq;

using RimWorld;

using Verse;

namespace FacialStuff
{
    using System;

    using FacialStuff.Defs;

    public static class PawnFaceChooser
    {
        public static void RandomBeardDefFor(Pawn pawn, FactionDef factionType, out BeardDef mainBeard, out MoustacheDef moustache)
        {
            if (pawn.gender != Gender.Male)
            {
                mainBeard = BeardDefOf.Beard_Shaved;
                moustache = MoustacheDefOf.Shaved;
                return;
            }
            BeardRoulette(pawn, factionType, out mainBeard, out moustache);
        }

        public static BeardDef RandomBeardDefFor(Pawn pawn, BeardType type)
        {
            IEnumerable<BeardDef> source;
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs
                         where beard.raceList.Contains(pawn.def)
                         where beard.beardType == type
                         select beard;
            }


            if (!source.Any())
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs select beard;
            }


            BeardDef chosenBeard;
            float rand = Rand.Value;
            bool flag = false;

            if (pawn.ageTracker.AgeBiologicalYearsFloat < 19 || rand < 0.1f || pawn.gender == Gender.Female)
            {
                chosenBeard = BeardDefOf.Beard_Shaved;
                flag = true;
            }
            else if (rand < 0.15f)
            {
                chosenBeard = BeardDefOf.Beard_Stubble;
            }
            else
            {
                chosenBeard = source.RandomElementByWeight(beard => BeardChoiceLikelihoodFor(beard, pawn));
            }

            return chosenBeard;
        }

        private static void BeardRoulette(Pawn pawn, FactionDef factionType, out BeardDef mainBeard, out MoustacheDef moustache)
        {
            moustache = MoustacheDefOf.Shaved;
            IEnumerable<BeardDef> source;
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs
                         where beard.raceList.Contains(pawn.def)
                         where beard.hairTags.SharesElementWith(factionType.hairTags)
                         select beard;
            }


            if (!source.Any())
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs select beard;
            }


            BeardDef chosenBeard;
            float rand = Rand.Value;
            bool flag = false;

            if (pawn.ageTracker.AgeBiologicalYearsFloat < 19)
            {
                chosenBeard = BeardDefOf.Beard_Shaved;
                flag = true;
            }
            else if (rand < 0.15f)
            {
                chosenBeard = BeardDefOf.Beard_Shaved;
            }
            else if (rand < 0.35f)
            {
                chosenBeard = BeardDefOf.Beard_Stubble;
            }
            else
            {
                chosenBeard = source.RandomElementByWeight(beard => BeardChoiceLikelihoodFor(beard, pawn));
            }


            mainBeard = chosenBeard;
            if (!flag && mainBeard.beardType != BeardType.FullBeard)
            {
                moustache = MoustacheRoulette(pawn, factionType);
            }

        }

        private static MoustacheDef MoustacheRoulette(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<MoustacheDef> source = from beard in DefDatabase<MoustacheDef>.AllDefs
                                 where beard.raceList.Contains(pawn.def)
                                 where beard.hairTags.SharesElementWith(factionType.hairTags)
                                 select beard;

            if (!source.Any())
            {
                source = from beard in DefDatabase<MoustacheDef>.AllDefs select beard;
            }
            else
            {
                return MoustacheDefOf.Shaved;
            }

            MoustacheDef chosenBeard;

            {
                chosenBeard = source.RandomElementByWeight(beard => TacheChoiceLikelihoodFor(beard, pawn));
            }
            return chosenBeard;
        }

        public static EyeDef RandomEyeDefFor(Pawn pawn, FactionDef factionType)
        {
            // Log.Message("Selecting eyes.");
            IEnumerable<EyeDef> source = from eye in DefDatabase<EyeDef>.AllDefs
                                         where eye.raceList.Contains(pawn.def)
                                         where eye.hairTags.SharesElementWith(factionType.hairTags)
                                         select eye;

            if (!source.Any())
            {
                // Log.Message("No eyes found, defaulting.");
                source = from eye in DefDatabase<EyeDef>.AllDefs select eye;
            }

            EyeDef chosenEyes;

            chosenEyes = source.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));

            // Log.Message("Chosen eyes: " + chosenEyes);
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

        public static WrinkleDef AssignWrinkleDefFor(Pawn pawn)
        {
            IEnumerable<WrinkleDef> source = from wrinkle in DefDatabase<WrinkleDef>.AllDefs
                                             where wrinkle.raceList.Contains(pawn.def)
                                             where wrinkle.hairGender.ToString() == pawn.gender.ToString()  // .SharesElementWith(factionType.hairTags)
                                             select wrinkle;

            WrinkleDef chosenWrinkles = source.FirstOrDefault();

            return chosenWrinkles;
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

        private static float BeardChoiceLikelihoodFor(BeardDef beard, Pawn pawn)
        {

            if (pawn.gender == Gender.None)
            {
                return 0f;
            }

            if (beard.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 37)
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

        private static float TacheChoiceLikelihoodFor(MoustacheDef beard, Pawn pawn)
        {

            if (pawn.gender == Gender.None)
            {
                return 0f;
            }

            if (beard.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 37)
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
        public static float HairChoiceLikelihoodFor(HairDef hair, Pawn pawn)
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