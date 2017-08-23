namespace FacialStuff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Defs;
    using FacialStuff.Enums;

    using JetBrains.Annotations;

    using RimWorld;

    using Verse;

    public static class PawnFaceChooser
    {

        #region Public Methods

        [JetBrains.Annotations.NotNull]
        public static WrinkleDef AssignWrinkleDefFor([NotNull] Pawn pawn)
        {
            IEnumerable<WrinkleDef> source = from wrinkle in DefDatabase<WrinkleDef>.AllDefs
                                             where wrinkle.raceList.Contains(pawn.def)
                                             where wrinkle.hairGender.ToString()
                                                   == pawn.gender.ToString() // .SharesElementWith(factionType.hairTags)
                                             select wrinkle;

            WrinkleDef chosenWrinkles = source.FirstOrDefault();

            return chosenWrinkles;
        }

        // RimWorld.PawnHairChooser
        public static float HairChoiceLikelihoodFor([NotNull] HairDef hair, [NotNull] Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }

            if (Controller.settings.UseWeirdHairChoices)
            {
                // The more advanced, the weirder they get
                if (pawn.Faction.def.techLevel >= TechLevel.Neolithic)
                {
                    switch (pawn.Faction.def.techLevel)
                    {
                        case TechLevel.Undefined: break;
                        case TechLevel.Animal: break;

                        // More traditional gender roles
                        case TechLevel.Neolithic:
                            if (pawn.gender == Gender.Male)
                            {
                                if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 21)
                                {
                                    return 0f;
                                }

                                switch (hair.hairGender)
                                {
                                    case HairGender.Male: return 70f;
                                    case HairGender.MaleUsually: return 40f;
                                    case HairGender.Any: return 30f;
                                    case HairGender.FemaleUsually: return 3f;
                                    case HairGender.Female: return 1f;
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
                                    case HairGender.Male: return 1f;
                                    case HairGender.MaleUsually: return 3f;
                                    case HairGender.Any: return 30f;
                                    case HairGender.FemaleUsually: return 40f;
                                    case HairGender.Female: return 70f;
                                }
                            }

                            break;

                        // Fashion victims
                        case TechLevel.Medieval:
                            if (pawn.gender == Gender.Male)
                            {
                                if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 21)
                                {
                                    return 0f;
                                }

                                switch (hair.hairGender)
                                {
                                    case HairGender.Male: return 70f;
                                    case HairGender.MaleUsually: return 30f;
                                    case HairGender.Any: return 40f;
                                    case HairGender.FemaleUsually: return 5f;
                                    case HairGender.Female: return 1f;
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
                                    case HairGender.Male: return 1f;
                                    case HairGender.MaleUsually: return 5f;
                                    case HairGender.Any: return 40f;
                                    case HairGender.FemaleUsually: return 30f;
                                    case HairGender.Female: return 70f;
                                }
                            }

                            break;

                        // High affection to unisex
                        case TechLevel.Industrial:
                            if (pawn.gender == Gender.Male)
                            {
                                if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 21)
                                {
                                    return 0f;
                                }

                                switch (hair.hairGender)
                                {
                                    case HairGender.Male: return 60f;
                                    case HairGender.MaleUsually: return 40f;
                                    case HairGender.Any: return 70f;
                                    case HairGender.FemaleUsually: return 10f;
                                    case HairGender.Female: return 1f;
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
                                    case HairGender.Male: return 1f;
                                    case HairGender.MaleUsually: return 10f;
                                    case HairGender.Any: return 70f;
                                    case HairGender.FemaleUsually: return 40f;
                                    case HairGender.Female: return 60f;
                                }
                            }

                            break;

                        // Gender roles don't really matter any more
                        case TechLevel.Spacer:
                        case TechLevel.Ultra:
                        case TechLevel.Transcendent:
                            if (pawn.gender == Gender.Male)
                            {
                                if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 45)
                                {
                                    return 0f;
                                }

                                switch (hair.hairGender)
                                {
                                    case HairGender.Male: return 15f;
                                    case HairGender.MaleUsually: return 30f;
                                    case HairGender.Any: return 35f;
                                    case HairGender.FemaleUsually: return 30f;
                                    case HairGender.Female: return 15f;
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
                                    case HairGender.Male: return 15f;
                                    case HairGender.MaleUsually: return 30f;
                                    case HairGender.Any: return 35f;
                                    case HairGender.FemaleUsually: return 30f;
                                    case HairGender.Female: return 15f;
                                }
                            }

                            break;

                        default: throw new ArgumentOutOfRangeException();
                    }

                    if (pawn.Faction.def.techLevel >= TechLevel.Spacer)
                    {
                        if (pawn.gender == Gender.Male)
                        {
                            if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 45)
                            {
                                return 0f;
                            }

                            switch (hair.hairGender)
                            {
                                case HairGender.Male: return 15f;
                                case HairGender.MaleUsually: return 30f;
                                case HairGender.Any: return 35f;
                                case HairGender.FemaleUsually: return 30f;
                                case HairGender.Female: return 15f;
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
                                case HairGender.Male: return 15f;
                                case HairGender.MaleUsually: return 30f;
                                case HairGender.Any: return 35f;
                                case HairGender.FemaleUsually: return 30f;
                                case HairGender.Female: return 15f;
                            }
                        }
                    }
                }
            }

            // Everyone else
            if (pawn.gender == Gender.Male)
            {
                if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 35)
                {
                    return 0f;
                }

                switch (hair.hairGender)
                {
                    case HairGender.Male: return 70f;
                    case HairGender.MaleUsually: return 30f;
                    case HairGender.Any: return 50f;
                    case HairGender.FemaleUsually: return 0f;
                    case HairGender.Female: return 0f;
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
                    case HairGender.Male: return 0f;
                    case HairGender.MaleUsually: return 0f;
                    case HairGender.Any: return 50f;
                    case HairGender.FemaleUsually: return 30f;
                    case HairGender.Female: return 70f;
                }
            }

            Log.Error(string.Concat("Unknown hair likelihood for ", hair, " with ", pawn));
            return 0f;
        }

        public static void RandomBeardDefFor(
                            [NotNull] Pawn pawn,
            [NotNull] FactionDef factionType,
            [NotNull] out BeardDef mainBeard,
            [NotNull] out MoustacheDef moustache)
        {
            BeardRoulette(pawn, factionType, out mainBeard, out moustache);
        }

        [JetBrains.Annotations.NotNull]
        public static BeardDef RandomBeardDefFor([NotNull] Pawn pawn, BeardType type)
        {
            if (pawn.gender != Gender.Male)
            {
                return BeardDefOf.Beard_Shaved;
            }

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

            if (pawn.ageTracker.AgeBiologicalYearsFloat < 19 || rand < 0.1f)
            {
                chosenBeard = BeardDefOf.Beard_Shaved;
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

        [JetBrains.Annotations.NotNull]
        public static BrowDef RandomBrowDefFor([JetBrains.Annotations.NotNull] Pawn pawn,
                                               [JetBrains.Annotations.NotNull] FactionDef factionType)
        {
            IEnumerable<BrowDef> source = from brow in DefDatabase<BrowDef>.AllDefs
                                          where brow.raceList.Contains(pawn.def)
                                          where brow.hairTags.SharesElementWith(factionType.hairTags)
                                          select brow;

            if (!source.Any())
            {
                source = from brow in DefDatabase<BrowDef>.AllDefs select brow;
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
                        IEnumerable<BrowDef> filtered =
                            source.Where(x => !x.label.Contains("Depressed") && !x.label.Contains("Tired"));
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

        [JetBrains.Annotations.NotNull]
        public static EyeDef RandomEyeDefFor([JetBrains.Annotations.NotNull] Pawn pawn,
                                             [JetBrains.Annotations.NotNull] FactionDef factionType)
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

        #endregion Public Methods

        #region Private Methods

        private static float BeardChoiceLikelihoodFor([NotNull] BeardDef beard, [NotNull] Pawn pawn)
        {
            if (beard.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 37)
            {
                return 0f;
            }

            switch (beard.hairGender)
            {
                case HairGender.Male: return 70f;
                case HairGender.MaleUsually: return 30f;
                case HairGender.Any: return 60f;
                case HairGender.FemaleUsually: return 5f;
                case HairGender.Female: return 1f;
            }

            Log.Error(string.Concat("Unknown beard likelihood for ", beard, " with ", pawn));
            return 0f;
        }

        private static void BeardRoulette(
            [JetBrains.Annotations.NotNull] Pawn pawn,
            [JetBrains.Annotations.NotNull] FactionDef factionType,
                                    [JetBrains.Annotations.NotNull] out BeardDef mainBeard,
                                    [JetBrains.Annotations.NotNull] out MoustacheDef moustache)
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
            else if (rand < 0.1f)
            {
                chosenBeard = BeardDefOf.Beard_Shaved;
            }
            else if (rand < 0.25f)
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

        private static float BrowChoiceLikelihoodFor([NotNull] BrowDef brow, [NotNull] Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }

            if (pawn.gender == Gender.Male)
            {
                switch (brow.hairGender)
                {
                    case HairGender.Male: return 70f;
                    case HairGender.MaleUsually: return 30f;
                    case HairGender.Any: return 60f;
                    case HairGender.FemaleUsually: return 0f;
                    case HairGender.Female: return 0f;
                }
            }

            if (pawn.gender == Gender.Female)
            {
                switch (brow.hairGender)
                {
                    case HairGender.Female: return 70f;
                    case HairGender.FemaleUsually: return 30f;
                    case HairGender.Any: return 60f;
                    case HairGender.MaleUsually: return 0f;
                    case HairGender.Male: return 0f;
                }
            }

            Log.Error(string.Concat("Unknown brow likelihood for ", brow, " with ", pawn));
            return 0f;
        }

        private static float EyeChoiceLikelihoodFor([NotNull] EyeDef eye, [NotNull] Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }

            if (pawn.gender == Gender.Male)
            {
                switch (eye.hairGender)
                {
                    case HairGender.Male: return 70f;
                    case HairGender.MaleUsually: return 30f;
                    case HairGender.Any: return 60f;
                    case HairGender.FemaleUsually: return 5f;
                    case HairGender.Female: return 1f;
                }
            }

            if (pawn.gender == Gender.Female)
            {
                switch (eye.hairGender)
                {
                    case HairGender.Female: return 70f;
                    case HairGender.FemaleUsually: return 30f;
                    case HairGender.Any: return 60f;
                    case HairGender.MaleUsually: return 5f;
                    case HairGender.Male: return 1f;
                }
            }

            Log.Error(string.Concat("Unknown eye likelihood for ", eye, " with ", pawn));
            return 0f;
        }

        [JetBrains.Annotations.NotNull]
        private static MoustacheDef MoustacheRoulette([NotNull] Pawn pawn, [NotNull] FactionDef factionType)
        {
            IEnumerable<MoustacheDef> source = from moustache in DefDatabase<MoustacheDef>.AllDefs
                                               where moustache.raceList.Contains(pawn.def)
                                               where moustache.hairTags.SharesElementWith(factionType.hairTags)
                                               select moustache;

            if (!source.Any())
            {
                source = from moustache in DefDatabase<MoustacheDef>.AllDefs select moustache;
            }
            else
            {
                return MoustacheDefOf.Shaved;
            }

            MoustacheDef moustacheDef;
            {
                moustacheDef = source.RandomElementByWeight(beard => TacheChoiceLikelihoodFor(beard, pawn));
            }

            return moustacheDef;
        }

        private static float TacheChoiceLikelihoodFor([NotNull] MoustacheDef beard, [NotNull] Pawn pawn)
        {
            if (beard.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 37)
            {
                return 0f;
            }


            switch (beard.hairGender)
            {
                case HairGender.Male: return 70f;
                case HairGender.MaleUsually: return 30f;
                case HairGender.Any: return 60f;
                case HairGender.FemaleUsually: return 5f;
                case HairGender.Female: return 1f;
            }

            Log.Error(string.Concat("Unknown tache likelihood for ", beard, " with ", pawn));
            return 0f;
        }

        #endregion Private Methods

    }
}