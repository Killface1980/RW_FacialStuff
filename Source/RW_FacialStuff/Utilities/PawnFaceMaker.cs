using System.Collections.Generic;
using System.Linq;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace FacialStuff.Utilities
{
    public static class PawnFaceMaker
    {
        public static WrinkleDef AssignWrinkleDefFor(Pawn pawn)
        {
            IEnumerable<WrinkleDef> source = from wrinkle in DefDatabase<WrinkleDef>.AllDefs

                                             // where !wrinkle.forbiddenOnRace.Contains(pawn.def)
                                             where wrinkle.hairGender.ToString()
                                                   == pawn.gender.ToString() // .SharesElementWith(factionType.hairTags)
                                             select wrinkle;

            WrinkleDef chosenWrinkles = source.FirstOrDefault();

            return chosenWrinkles;
        }

        // RimWorld.PawnHairChooser
        public static float HairChoiceLikelihoodFor(HairDef hair, [NotNull] Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }

            if (pawn.gender != Gender.Male && hair.hairTags.Contains("MaleOnly"))
            {
                return 0f;
            }

            // No CompFace here yet, and PrepC kills the faction!
            Faction faction = pawn.Faction ?? Faction.OfPlayer;

            if (Controller.settings.UseWeirdHairChoices)
            {
                // The more advanced, the weirder they get
                if (faction.def.techLevel >= TechLevel.Neolithic)
                {
                    switch (faction.def.techLevel)
                    {
                        case TechLevel.Undefined: break;
                        case TechLevel.Animal: break;

                        // More traditional gender roles
                        case TechLevel.Neolithic:
                            switch (pawn.gender)
                            {
                                case Gender.Male:
                                    if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 32)
                                    {
                                        return 0f;
                                    }

                                    switch (hair.hairGender)
                                    {
                                        case HairGender.Male:          return 70f;
                                        case HairGender.MaleUsually:   return 40f;
                                        case HairGender.Any:           return 30f;
                                        case HairGender.FemaleUsually: return 3f;
                                        case HairGender.Female:        return 1f;
                                    }

                                    break;
                                case Gender.Female:
                                    switch (hair.hairGender)
                                    {
                                        case HairGender.Male:          return 1f;
                                        case HairGender.MaleUsually:   return 3f;
                                        case HairGender.Any:           return 30f;
                                        case HairGender.FemaleUsually: return 40f;
                                        case HairGender.Female:        return 70f;
                                    }

                                    break;
                            }

                            break;

                        // Fashion victims
                        case TechLevel.Medieval:
                            {
                                switch (pawn.gender)
                                {
                                    case Gender.Male:
                                        if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 32)
                                        {
                                            return 0f;
                                        }

                                        switch (hair.hairGender)
                                        {
                                            case HairGender.Male:          return 70f;
                                            case HairGender.MaleUsually:   return 30f;
                                            case HairGender.Any:           return 40f;
                                            case HairGender.FemaleUsually: return 5f;
                                            case HairGender.Female:        return 1f;
                                        }

                                        break;
                                    case Gender.Female:
                                        switch (hair.hairGender)
                                        {
                                            case HairGender.Male:          return 1f;
                                            case HairGender.MaleUsually:   return 5f;
                                            case HairGender.Any:           return 40f;
                                            case HairGender.FemaleUsually: return 30f;
                                            case HairGender.Female:        return 70f;
                                        }

                                        break;
                                }

                                break;
                            }

                        // High affection to unisex
                        case TechLevel.Industrial:
                            switch (pawn.gender)
                            {
                                case Gender.Male:
                                    if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 32)
                                    {
                                        return 0f;
                                    }

                                    switch (hair.hairGender)
                                    {
                                        case HairGender.Male:          return 60f;
                                        case HairGender.MaleUsually:   return 40f;
                                        case HairGender.Any:           return 70f;
                                        case HairGender.FemaleUsually: return 10f;
                                        case HairGender.Female:        return 1f;
                                    }

                                    break;
                                case Gender.Female:
                                    switch (hair.hairGender)
                                    {
                                        case HairGender.Male:          return 1f;
                                        case HairGender.MaleUsually:   return 10f;
                                        case HairGender.Any:           return 70f;
                                        case HairGender.FemaleUsually: return 40f;
                                        case HairGender.Female:        return 60f;
                                    }

                                    break;
                            }

                            break;

                        // Gender roles don't really matter any more
                        case TechLevel.Spacer:
                        case TechLevel.Ultra:
                        case TechLevel.Archotech:
                            switch (pawn.gender)
                            {
                                case Gender.Male:
                                    if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 45)
                                    {
                                        return 0f;
                                    }

                                    switch (hair.hairGender)
                                    {
                                        case HairGender.Male:          return 15f;
                                        case HairGender.MaleUsually:   return 30f;
                                        case HairGender.Any:           return 35f;
                                        case HairGender.FemaleUsually: return 30f;
                                        case HairGender.Female:        return 15f;
                                    }

                                    break;
                                case Gender.Female:
                                    switch (hair.hairGender)
                                    {
                                        case HairGender.Male:          return 15f;
                                        case HairGender.MaleUsually:   return 30f;
                                        case HairGender.Any:           return 35f;
                                        case HairGender.FemaleUsually: return 30f;
                                        case HairGender.Female:        return 15f;
                                    }

                                    break;
                            }

                            break;
                    }
                }
            }

            // Everyone else
            switch (pawn.gender)
            {
                case Gender.Male:
                    if (hair.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 35)
                    {
                        return 0f;
                    }

                    switch (hair.hairGender)
                    {
                        case HairGender.Male:          return 70f;
                        case HairGender.MaleUsually:   return 30f;
                        case HairGender.Any:           return 50f;
                        case HairGender.FemaleUsually: return 0f;
                        case HairGender.Female:        return 0f;
                    }

                    break;
                case Gender.Female:
                    switch (hair.hairGender)
                    {
                        case HairGender.Male:          return 0f;
                        case HairGender.MaleUsually:   return 0f;
                        case HairGender.Any:           return 50f;
                        case HairGender.FemaleUsually: return 30f;
                        case HairGender.Female:        return 70f;
                    }

                    break;
            }

            Log.Error(string.Concat("Unknown hair likelihood for ", hair, " with ", pawn));
            return 0f;
        }

        public static void RandomBeardDefFor(
            [NotNull] CompFace face,
            [NotNull] FactionDef factionType,
            [NotNull] out BeardDef mainBeard,
            [NotNull] out MoustacheDef moustache)
        {
            if (!face.Props.hasBeard)
            {
                mainBeard = BeardDefOf.Beard_Shaved;
                moustache = MoustacheDefOf.Shaved;
                return;
            }

            BeardRoulette(face.Pawn, factionType, out mainBeard, out moustache);
        }

        public static BeardDef RandomBeardDefFor([NotNull] Pawn pawn, BeardType type)
        {
            if (pawn.gender != Gender.Male)
            {
                return BeardDefOf.Beard_Shaved;
            }

            IEnumerable<BeardDef> source = from beard in DefDatabase<BeardDef>.AllDefs

                                           // where !beard.forbiddenOnRace.Contains(pawn.def)
                                           where beard.beardType == type
                                           select beard;

            if (!source.Any())
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs select beard;
            }

            BeardDef chosenBeard;
            float rand = Rand.Value;

            if (pawn.ageTracker.AgeBiologicalYearsFloat < 19 || rand < 0.075f)
            {
                chosenBeard = BeardDefOf.Beard_Shaved;
            }
            else if (rand < 0.125f)
            {
                chosenBeard = BeardDefOf.Beard_Stubble;
            }
            else
            {
                chosenBeard = source.RandomElementByWeight(beard => BeardChoiceLikelihoodFor(beard, pawn));
            }

            return chosenBeard;
        }

        public static BrowDef RandomBrowDefFor([NotNull] Pawn pawn, FactionDef factionType)
        {
            IEnumerable<BrowDef> source = from brow in DefDatabase<BrowDef>.AllDefs

                                              // where brow.forbiddenOnRace.Contains(pawn.def)
                                          where brow.hairTags.SharesElementWith(factionType.hairTags)
                                          select brow;

            if (!source.Any())
            {
                source = from brow in DefDatabase<BrowDef>.AllDefs select brow;
            }

            /*
                        switch (pawn.story.traits.DegreeOfTrait(TraitDef.Named("NaturalMood")))
                        {
                            case 2:
                                {
                                    IEnumerable<BrowDef> filtered = browDefs.Where(x => x.label.Contains("Nice"));
                                    chosenBrows = filtered.RandomElementByWeight(brow => BrowChoiceLikelihoodFor(brow, pawn));
                                    return chosenBrows;
                                }

                            case 1:
                                {
                                    IEnumerable<BrowDef> filtered = browDefs.Where(x => x.label.Contains("Aware"));
                                    chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                                    return chosenBrows;
                                }

                            case 0:
                                {
                                    IEnumerable<BrowDef> filtered =
                                        browDefs.Where(x => !x.label.Contains("Depressed") && !x.label.Contains("Tired"));
                                    chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                                    return chosenBrows;
                                }

                            case -1:
                                {
                                    IEnumerable<BrowDef> filtered = browDefs.Where(x => x.label.Contains("Tired"));
                                    chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                                    return chosenBrows;
                                }

                            case -2:
                                {
                                    IEnumerable<BrowDef> filtered = browDefs.Where(x => x.label.Contains("Depressed"));
                                    chosenBrows = filtered.RandomElementByWeight(eye => BrowChoiceLikelihoodFor(eye, pawn));
                                    return chosenBrows;
                                }
                        }
                        */
            return source.RandomElementByWeight(brow => BrowChoiceLikelihoodFor(brow, pawn));
        }

        public static EyeDef RandomEyeDefFor(Pawn pawn, FactionDef factionType)
        {
            // Log.Message("Selecting eyes.");
            IEnumerable<EyeDef> source = from eye in DefDatabase<EyeDef>.AllDefs

                                             // where eye.forbiddenOnRace.Contains(pawn.def)
                                         where eye.hairTags.SharesElementWith(factionType.hairTags)
                                         select eye;

            if (!source.Any())
            {
                // Log.Message("No eyes found, defaulting.");
                source = from eye in DefDatabase<EyeDef>.AllDefs select eye;
            }

            return source.RandomElementByWeight(eye => EyeChoiceLikelihoodFor(eye, pawn));

            // Log.Message("Chosen eyes: " + chosenEyes);
        }
        public static EarDef RandomEarDefFor(Pawn pawn, FactionDef factionType)
        {
            // Log.Message("Selecting eyes.");
            IEnumerable<EarDef> source = from ear in DefDatabase<EarDef>.AllDefs

                                             // where eye.forbiddenOnRace.Contains(pawn.def)
                                         where ear.hairTags.SharesElementWith(factionType.hairTags)
                                         select ear;

            if (!source.Any())
            {
                // Log.Message("No eyes found, defaulting.");
                source = from eye in DefDatabase<EarDef>.AllDefs select eye;
            }

            return source.RandomElementByWeight(eye => EarChoiceLikelihoodFor(eye, pawn));

            // Log.Message("Chosen eyes: " + chosenEyes);
        }

        private static float BeardChoiceLikelihoodFor([NotNull] BeardDef beard, Pawn pawn)
        {
            if (beard.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 32)
            {
                return 20f;
            }

            switch (beard.hairGender)
            {
                case HairGender.Male: return 70f;
                case HairGender.MaleUsually: return 50f;
                default: return 30f;
            }

            Log.Error(string.Concat("Unknown beard likelihood for ", beard, " with ", pawn));
            return 0f;
        }

        private static void BeardRoulette(
            [NotNull] Pawn pawn,
            FactionDef factionType,
            out BeardDef mainBeard,
            out MoustacheDef moustache)
        {
            moustache = MoustacheDefOf.Shaved;
            IEnumerable<BeardDef> source;
            {
                source = from beard in DefDatabase<BeardDef>.AllDefs

                         // where !beard.forbiddenOnRace.Contains(pawn.def)
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
            else if (rand < 0.2f)
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

        private static float BrowChoiceLikelihoodFor(BrowDef brow, [NotNull] Pawn pawn)
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
                    case HairGender.FemaleUsually: return 5f;
                    case HairGender.Female: return 1f;
                }
            }

            if (pawn.gender == Gender.Female)
            {
                switch (brow.hairGender)
                {
                    case HairGender.Female: return 70f;
                    case HairGender.FemaleUsually: return 30f;
                    case HairGender.Any: return 60f;
                    case HairGender.MaleUsually: return 5f;
                    case HairGender.Male: return 1f;
                }
            }

            Log.Error(string.Concat("Unknown brow likelihood for ", brow, " with ", pawn));
            return 0f;
        }

        private static float EyeChoiceLikelihoodFor(EyeDef eye, [NotNull] Pawn pawn)
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

        private static float EarChoiceLikelihoodFor(EarDef ear, [NotNull] Pawn pawn)
        {
            if (pawn.gender == Gender.None)
            {
                return 100f;
            }

            if (pawn.gender == Gender.Male)
            {
                switch (ear.hairGender)
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
                switch (ear.hairGender)
                {
                    case HairGender.Female: return 70f;
                    case HairGender.FemaleUsually: return 30f;
                    case HairGender.Any: return 60f;
                    case HairGender.MaleUsually: return 5f;
                    case HairGender.Male: return 1f;
                }
            }

            Log.Error(string.Concat("Unknown ear likelihood for ", ear, " with ", pawn));
            return 0f;
        }

        private static MoustacheDef MoustacheRoulette(Pawn pawn, FactionDef factionType)
        {
            IEnumerable<MoustacheDef> source = from moustache in DefDatabase<MoustacheDef>.AllDefs

                                               // where !moustache.forbiddenOnRace.Contains(pawn.def)
                                               where moustache.hairTags.SharesElementWith(factionType.hairTags)
                                               select moustache;

            if (!source.Any())
            {
                source = from moustache in DefDatabase<MoustacheDef>.AllDefs select moustache;
            }

            MoustacheDef moustacheDef;
            {
                moustacheDef = source.RandomElementByWeight(tache => TacheChoiceLikelihoodFor(tache, pawn));
            }

            return moustacheDef;
        }

        private static float TacheChoiceLikelihoodFor([NotNull] MoustacheDef tache, Pawn pawn)
        {
            if (tache.hairTags.Contains("MaleOld") && pawn.ageTracker.AgeBiologicalYears < 37)
            {
                return 0f;
            }

            switch (tache.hairGender)
            {
                case HairGender.Male: return 70f;
                case HairGender.MaleUsually: return 30f;
            }

            Log.Error(string.Concat("Unknown tache likelihood for ", tache, " with ", pawn));
            return 0f;
        }

    }
}