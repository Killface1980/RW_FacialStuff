using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    using RW_FacialStuff.Aliens;

    public class ProviderAlienRaces
    {
        protected Dictionary<ThingDef, AlienRace> lookup = new Dictionary<ThingDef, AlienRace>();

        public AlienRace GetAlienRace(ThingDef def)
        {
            AlienRace alienRace;
            if (this.lookup.TryGetValue(def, out alienRace))
            {
                return alienRace;
            }
            if (ProviderAlienRaces.IsAlienRace(def))
            {
                alienRace = this.InitializeAlienRace(def);
                if (alienRace != null)
                {
                    this.lookup.Add(def, alienRace);
                }
                return alienRace;
            }
            return null;
        }

        public static bool IsAlienRace(ThingDef raceDef)
        {
            return raceDef.GetType().GetField("alienRace", BindingFlags.Instance | BindingFlags.Public) != null;
        }

        protected AlienRace InitializeAlienRace(ThingDef raceDef)
        {
            object fieldValue = this.GetFieldValue(raceDef, raceDef, "alienRace", false);
            if (fieldValue == null)
            {
                return null;
            }
            object fieldValue2 = this.GetFieldValue(raceDef, fieldValue, "generalSettings", false);
            if (fieldValue2 == null)
            {
                return null;
            }
            object fieldValue3 = this.GetFieldValue(raceDef, fieldValue2, "alienPartGenerator", false);
            if (fieldValue3 == null)
            {
                return null;
            }
            ICollection fieldValueAsCollection = this.GetFieldValueAsCollection(raceDef, fieldValue, "graphicPaths");
            if (fieldValueAsCollection == null)
            {
                return null;
            }
            AlienRace alienRace = new AlienRace();
            ICollection fieldValueAsCollection2 = this.GetFieldValueAsCollection(raceDef, fieldValue3, "alienbodytypes");
            if (fieldValueAsCollection2 == null)
            {
                return null;
            }
            List<BodyType> list = new List<BodyType>();
            if (fieldValueAsCollection2.Count > 0)
            {
                foreach (object current in fieldValueAsCollection2)
                {
                    if (current.GetType() == typeof(BodyType))
                    {
                        list.Add((BodyType)current);
                    }
                }
            }
            alienRace.BodyTypes = list;
            bool? fieldValueAsBool = this.GetFieldValueAsBool(raceDef, fieldValue3, "UseGenderedHeads");
            if (!fieldValueAsBool.HasValue)
            {
                return null;
            }
            alienRace.GenderSpecificHeads = fieldValueAsBool.Value;
            ICollection fieldValueAsCollection3 = this.GetFieldValueAsCollection(raceDef, fieldValue3, "aliencrowntypes");
            if (fieldValueAsCollection3 == null)
            {
                return null;
            }
            List<string> list2 = new List<string>();
            if (fieldValueAsCollection3.Count > 0)
            {
                IEnumerator enumerator = fieldValueAsCollection3.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string text = enumerator.Current as string;
                    if (text != null)
                    {
                        list2.Add(text);
                    }
                }
            }
            alienRace.CrownTypes = list2;
            string graphicsPathForHeads = null;
            foreach (object current2 in fieldValueAsCollection)
            {
                ICollection fieldValueAsCollection4 = this.GetFieldValueAsCollection(raceDef, current2, "lifeStageDefs");
                if (fieldValueAsCollection4 == null || fieldValueAsCollection4.Count == 0)
                {
                    string fieldValueAsString = this.GetFieldValueAsString(raceDef, current2, "head");
                    if (fieldValueAsString != null)
                    {
                        graphicsPathForHeads = fieldValueAsString;
                        break;
                    }
                }
            }
            alienRace.GraphicsPathForHeads = graphicsPathForHeads;
            object arg_203_0 = this.GetFieldValue(raceDef, fieldValue3, "alienskincolorgen", true);
            alienRace.UseMelaninLevels = true;
            ColorGenerator colorGenerator = arg_203_0 as ColorGenerator;
            if (colorGenerator != null)
            {
                alienRace.UseMelaninLevels = false;
                alienRace.PrimaryColors = colorGenerator.GetColorList();
            }
            else
            {
                alienRace.PrimaryColors = new List<Color>();
            }
            object arg_248_0 = this.GetFieldValue(raceDef, fieldValue3, "alienskinsecondcolorgen", true);
            alienRace.HasSecondaryColor = false;
            ColorGenerator colorGenerator2 = arg_248_0 as ColorGenerator;
            if (colorGenerator2 != null)
            {
                alienRace.HasSecondaryColor = true;
                alienRace.SecondaryColors = colorGenerator2.GetColorList();
            }
            else
            {
                alienRace.SecondaryColors = new List<Color>();
            }
            object fieldValue4 = this.GetFieldValue(raceDef, fieldValue, "hairSettings", true);
            alienRace.HasHair = true;
            if (fieldValue4 != null)
            {
                bool? fieldValueAsBool2 = this.GetFieldValueAsBool(raceDef, fieldValue4, "HasHair");
                if (fieldValueAsBool2.HasValue)
                {
                    alienRace.HasHair = fieldValueAsBool2.Value;
                }
                ICollection fieldValueAsCollection5 = this.GetFieldValueAsCollection(raceDef, fieldValue4, "hairTags");
                if (fieldValueAsCollection5 != null)
                {
                    HashSet<string> hashSet = new HashSet<string>();
                    IEnumerator enumerator = fieldValueAsCollection5.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string text2 = enumerator.Current as string;
                        if (text2 != null)
                        {
                            hashSet.Add(text2);
                        }
                    }
                    if (hashSet.Count > 0)
                    {
                        alienRace.HairTags = hashSet;
                    }
                }
            }
            if (this.GetFieldValue(raceDef, fieldValue3, "alienhaircolorgen", true) is ColorGenerator)
            {
                alienRace.HairColors = colorGenerator.GetColorList();
            }
            else
            {
                alienRace.HairColors = null;
            }
            object fieldValue5 = this.GetFieldValue(raceDef, fieldValue, "raceRestriction", true);
            alienRace.RestrictedApparelOnly = false;
            if (fieldValue5 != null)
            {
                bool? fieldValueAsBool3 = this.GetFieldValueAsBool(raceDef, fieldValue5, "onlyUseRaceRestrictedApparel");
                if (fieldValueAsBool3.HasValue)
                {
                    alienRace.RestrictedApparelOnly = fieldValueAsBool3.Value;
                }
                ICollection fieldValueAsCollection6 = this.GetFieldValueAsCollection(raceDef, fieldValue5, "apparelList");
                if (fieldValueAsCollection6 != null)
                {
                    HashSet<string> hashSet2 = new HashSet<string>();
                    IEnumerator enumerator = fieldValueAsCollection6.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string text3 = enumerator.Current as string;
                        if (text3 != null)
                        {
                            hashSet2.Add(text3);
                        }
                    }
                    if (hashSet2.Count > 0)
                    {
                        alienRace.RestrictedApparel = hashSet2;
                    }
                }
            }
            return alienRace;
        }

        protected object GetFieldValue(ThingDef raceDef, object source, string name, bool allowNull = false)
        {
            object result;
            try
            {
                FieldInfo field = source.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                if (field == null)
                {
                    Log.Warning("Prepare carefully could not find " + name + " field for " + raceDef.defName);
                    result = null;
                }
                else
                {
                    object value = field.GetValue(source);
                    if (value == null)
                    {
                        if (!allowNull)
                        {
                            Log.Warning("Prepare carefully could not find " + name + " field value for " + raceDef.defName);
                        }
                        result = null;
                    }
                    else
                    {
                        result = value;
                    }
                }
            }
            catch (Exception)
            {
                Log.Warning("Prepare carefully could resolve value of the " + name + " field for " + raceDef.defName);
                result = null;
            }
            return result;
        }

        protected ICollection GetFieldValueAsCollection(ThingDef raceDef, object source, string name)
        {
            object fieldValue = this.GetFieldValue(raceDef, source, name, true);
            if (fieldValue == null)
            {
                return null;
            }
            ICollection collection = fieldValue as ICollection;
            if (collection == null)
            {
                Log.Warning(string.Concat(new string[]
                                              {
                                                  "Prepare carefully could not convert ",
                                                  name,
                                                  " field value into a collection for ",
                                                  raceDef.defName,
                                                  "."
                                              }));
                return null;
            }
            return collection;
        }

        protected bool? GetFieldValueAsBool(ThingDef raceDef, object source, string name)
        {
            object fieldValue = this.GetFieldValue(raceDef, source, name, true);
            if (fieldValue == null)
            {
                return null;
            }
            if (fieldValue.GetType() == typeof(bool))
            {
                return new bool?((bool)fieldValue);
            }
            Log.Warning(string.Concat(new string[]
                                          {
                                              "Prepare carefully could not convert ",
                                              name,
                                              " field value into a bool for ",
                                              raceDef.defName,
                                              "."
                                          }));
            return null;
        }

        protected string GetFieldValueAsString(ThingDef raceDef, object source, string name)
        {
            object fieldValue = this.GetFieldValue(raceDef, source, name, true);
            if (fieldValue == null)
            {
                return null;
            }
            string text = fieldValue as string;
            if (text != null)
            {
                return text;
            }
            Log.Warning(string.Concat(new string[]
                                          {
                                              "Prepare carefully could not convert ",
                                              name,
                                              " field value into a string for ",
                                              raceDef.defName,
                                              "."
                                          }));
            return null;
        }
    }
}
