namespace AlienFace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    using global::AlienRace;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class ProviderAlienRaces
    {
        protected static Dictionary<ThingDef_AlienRace, AlienRace> lookup = new Dictionary<ThingDef_AlienRace, AlienRace>();
        public ProviderAlienRaces()
        {

        }

        [CanBeNull]
        public static AlienRace GetAlienRace(ThingDef_AlienRace def, Pawn p)
        {
            AlienRace result;
            if (lookup.TryGetValue(def, out result))
            {
                return result;
            }
            else
            {
                result = InitializeAlienRace(def, p);
                if (result != null)
                {
                    lookup.Add(def, result);
                }

                return result;
            }
        }

        [CanBeNull]
        public static AlienRace InitializeAlienRace(ThingDef_AlienRace raceDef, Pawn p)
        {
            ThingDef_AlienRace.AlienSettings race = raceDef.alienRace;
            GeneralSettings generalSettings = race?.generalSettings;
            AlienPartGenerator alienPartGenerator = generalSettings?.alienPartGenerator;
            if (alienPartGenerator == null)
            {
                return null;
            }

            List<GraphicPaths> graphicsPaths = race.graphicPaths;
            if (graphicsPaths == null)
            {
                return null;
            }

            // We have enough to start putting together the result object, so we instantiate it now.
            AlienRace result = new AlienRace();

            // Get the list of body types.
            List<BodyType> alienbodytypes = alienPartGenerator.alienbodytypes;
            if (alienbodytypes == null)
            {
                return null;
            }

            List<BodyType> bodyTypes = new List<BodyType>();
            if (alienbodytypes.Count > 0)
            {
                foreach (BodyType o in alienbodytypes)
                {
                    bodyTypes.Add((BodyType)o);
                }
            }

            result.BodyTypes = bodyTypes;

            // Determine if the alien races uses gender-specific heads.
            result.GenderSpecificHeads = alienPartGenerator.useGenderedHeads;

            result.CrownTypes = alienPartGenerator.aliencrowntypes;

            // Go through the graphics paths and find the heads path.
            string graphicsPathForHeads = null;
            foreach (GraphicPaths graphicsPath in graphicsPaths)
            {
                ICollection lifeStageCollection = GetFieldValueAsCollection(raceDef, graphicsPath, "lifeStageDefs");
                if (lifeStageCollection == null || lifeStageCollection.Count == 0)
                {
                    string path = GetFieldValueAsString(raceDef, graphicsPath, "head");
                    if (path != null)
                    {
                        graphicsPathForHeads = path;
                        break;
                    }
                }
            }

            result.GraphicsPathForHeads = graphicsPathForHeads;

            // Figure out colors.
            ColorGenerator primaryGenerator = alienPartGenerator.alienskincolorgen;
            result.UseMelaninLevels = true;
            if (primaryGenerator != null)
            {
                result.UseMelaninLevels = false;
                result.PrimaryColors = primaryGenerator.GetColorList();
            }
            else
            {
                result.PrimaryColors = new List<Color>();
            }

            ColorGenerator secondaryGenerator = alienPartGenerator.alienskinsecondcolorgen;
            result.HasSecondaryColor = false;
            if (secondaryGenerator != null)
            {
                result.HasSecondaryColor = true;
                result.SecondaryColors = secondaryGenerator.GetColorList();
            }
            else
            {
                result.SecondaryColors = new List<Color>();
            }

            // Hair properties.
            HairSettings hairSettings = race.hairSettings;
            result.HasHair = false;
            if (hairSettings != null)
            {
                result.HasHair = hairSettings.hasHair;

                if (hairSettings.hairTags.NullOrEmpty())
                {
                    result.HairTags = p.kindDef.defaultFactionType.hairTags;
                }
                else
                {
                    result.HairTags = hairSettings.hairTags;
                }


            }

            object hairColorGeneratorValue = GetFieldValue(raceDef, alienPartGenerator, "alienhaircolorgen", true);
            ColorGenerator hairColorGenerator = hairColorGeneratorValue as ColorGenerator;
            if (hairColorGenerator != null)
            {
                result.HairColors = primaryGenerator.GetColorList();
            }
            else
            {
                result.HairColors = null;
            }

            // Apparel properties.
            object restrictionSettingsValue = GetFieldValue(raceDef, race, "raceRestriction", true);
            result.RestrictedApparelOnly = false;
            if (restrictionSettingsValue != null)
            {
                bool? restrictedApparelOnly = GetFieldValueAsBool(raceDef, restrictionSettingsValue, "onlyUseRaceRestrictedApparel");
                if (restrictedApparelOnly != null)
                {
                    result.RestrictedApparelOnly = restrictedApparelOnly.Value;
                }

                ICollection restrictedApparelCollection = GetFieldValueAsCollection(raceDef, restrictionSettingsValue, "apparelList");
                if (restrictedApparelCollection != null)
                {
                    HashSet<string> apparel = new HashSet<string>();
                    foreach (object o in restrictedApparelCollection)
                    {
                        string defName = o as string;
                        if (defName != null)
                        {
                            apparel.Add(defName);
                        }
                    }

                    if (apparel.Count > 0)
                    {
                        result.RestrictedApparel = apparel;
                    }
                }
            }

            return result;
        }

        protected static object GetFieldValue(ThingDef raceDef, object source, string name, bool allowNull = false)
        {
            try
            {
                FieldInfo field = source.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                {
                    Log.Warning("Prepare carefully could not find " + name + " field for " + raceDef.defName);
                    return null;
                }

                object result = field.GetValue(source);
                if (result == null)
                {
                    if (!allowNull)
                    {
                        Log.Warning("Prepare carefully could not find " + name + " field value for " + raceDef.defName);
                    }

                    return null;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception)
            {
                Log.Warning("Prepare carefully could resolve value of the " + name + " field for " + raceDef.defName);
                return null;
            }
        }

        protected static ICollection GetFieldValueAsCollection(ThingDef raceDef, object source, string name)
        {
            object result = GetFieldValue(raceDef, source, name, true);
            if (result == null)
            {
                return null;
            }

            ICollection collection = result as System.Collections.ICollection;
            if (collection == null)
            {
                Log.Warning("Prepare carefully could not convert " + name + " field value into a collection for " + raceDef.defName + ".");
                return null;
            }
            else
            {
                return collection;
            }
        }

        protected static bool? GetFieldValueAsBool(ThingDef raceDef, object source, string name)
        {
            object result = GetFieldValue(raceDef, source, name, true);
            if (result == null)
            {
                return null;
            }

            if (result.GetType() == typeof(bool))
            {
                return (bool)result;
            }
            else
            {
                Log.Warning("Prepare carefully could not convert " + name + " field value into a bool for " + raceDef.defName + ".");
                return null;
            }
        }

        protected static string GetFieldValueAsString(ThingDef raceDef, object source, string name)
        {
            object value = GetFieldValue(raceDef, source, name, true);
            if (value == null)
            {
                return null;
            }

            string result = value as string;
            if (result != null)
            {
                return result;
            }
            else
            {
                Log.Warning("Prepare carefully could not convert " + name + " field value into a string for " + raceDef.defName + ".");
                return null;
            }
        }
    }
}