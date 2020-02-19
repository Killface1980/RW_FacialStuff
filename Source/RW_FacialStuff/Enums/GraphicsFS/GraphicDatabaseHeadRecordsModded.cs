using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public static class GraphicDatabaseHeadRecordsModded
    {
        #region Public Fields

        [NotNull]
        public static List<HeadGraphicRecord> HeadsVanillaCustom =
        new List<HeadGraphicRecord>();

        #endregion Public Fields

        #region Private Fields
    
        // Vanilla
        [NotNull] private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        [NotNull] private static readonly string StumpPath = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

        #endregion Private Fields

        #region Public Methods

        private static readonly string[] HeadsFolderPaths = {
                                                            "Things/Pawn/Humanlike/Heads_blank/Male",
                                                            "Things/Pawn/Humanlike/Heads_blank/Female"
                                                            };

        private static HeadGraphicRecord _skull;
        private static HeadGraphicRecord _stump;

        public static void BuildDatabaseIfNecessary()
        {
            if (HeadsVanillaCustom.Count > 0 && _skull != null && _stump != null)
            {
                return;
            }

            HeadsVanillaCustom.Clear();

            string thingsPawnHumanlike = StringsFS.PathHumanlike;
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Female/Female_Average_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Female/Female_Average_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Female/Female_Average_Wide"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Female/Female_Narrow_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Female/Female_Narrow_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Female/Female_Narrow_Wide"));

            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Male/Male_Average_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Male/Male_Average_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Male/Male_Average_Wide"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Male/Male_Narrow_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike+"Heads_blank/Male/Male_Narrow_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecord(thingsPawnHumanlike + "Heads_blank/Male/Male_Narrow_Wide"));

            _skull = new HeadGraphicRecord(SkullPath);
            _stump = new HeadGraphicRecord(StumpPath);
        }

        public static Graphic_Multi GetModdedHeadNamed([NotNull] Pawn pawn, Color color)
        {
            BuildDatabaseIfNecessary();
            foreach (HeadGraphicRecord headGraphicRecordVanillaCustom in HeadsVanillaCustom)
            {
                if (Path.GetFileNameWithoutExtension(headGraphicRecordVanillaCustom.graphicPath) ==
                    Path.GetFileNameWithoutExtension(pawn.story?.HeadGraphicPath))
                {
                    return headGraphicRecordVanillaCustom.GetGraphic(color);
                }
            }


            Log.Message(
                        "Tried to get blank pawn head " + Path.GetFileNameWithoutExtension(pawn.story?.HeadGraphicPath)
                                                          + " that was not found. Facial Stuff Defaulting...");

            return HeadsVanillaCustom.First()?.GetGraphic(color);
        }

        public static Graphic_Multi GetStump(Color skinColor)
        {
            BuildDatabaseIfNecessary();
            return _stump.GetGraphic(skinColor);
        }

        public static void Reset()
        {
            HeadsVanillaCustom.Clear();
            _skull = null;
            _stump = null;
        }

        #endregion Public Methods

        #region Public Classes

        public class HeadGraphicRecord
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public HeadGraphicRecord(string graphicPath)
            {
                this.graphicPath                  = graphicPath;
                string   fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
                string[] array                    = fileNameWithoutExtension?.Split('_');
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    this.crownType = (CrownType)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType));
                    this.gender    = (Gender)ParseHelper.FromString(array[array.Length    - 3], typeof(Gender));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    this.crownType = CrownType.Undefined;
                    this.gender    = Gender.None;
                }
            }

            public Graphic_Multi GetGraphic(Color color)
            {
                for (int i = 0; i < this.graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(this.graphics[i].Key))
                    {
                        return this.graphics[i].Value;
                    }
                }
                Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(this.graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
                this.graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
                return graphic_Multi;
            }
        }

    #endregion Public Classes
}
}