namespace FacialStuff.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public static class GraphicDatabaseHeadRecordsModded
    {
        #region Public Fields

        public static readonly List<HeadGraphicRecordVanillaCustom> HeadsVanillaCustom =
            new List<HeadGraphicRecordVanillaCustom>();

        #endregion Public Fields

        #region Private Fields

        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

        private static readonly string StumpPath = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

        private static HeadGraphicRecordVanillaCustom skull;

        private static HeadGraphicRecordVanillaCustom stump;

        #endregion Private Fields

        #region Public Methods

        public static void BuildDatabaseIfNecessary()
        {
            if (HeadsVanillaCustom.Count > 0 && skull != null && stump != null)
            {
                return;
            }

            HeadsVanillaCustom.Clear();

            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Wide"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Wide"));

            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Wide"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Normal"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Pointy"));
            HeadsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Wide"));

            skull = new HeadGraphicRecordVanillaCustom(SkullPath);
            stump = new HeadGraphicRecordVanillaCustom(StumpPath);
        }

        public static Graphic_Multi GetModdedHeadNamed([NotNull] Pawn pawn, Color color)
        {
            BuildDatabaseIfNecessary();
            foreach (HeadGraphicRecordVanillaCustom headGraphicRecordVanillaCustom in HeadsVanillaCustom)
            {
                if (headGraphicRecordVanillaCustom.graphicPathVanillaCustom == pawn.story.HeadGraphicPath.Remove(0, 22))
                {
                    // Log.Message("Getting vanilla " + pawn.story.HeadGraphicPath.Remove(0, 22) + ".");
                    return headGraphicRecordVanillaCustom.GetGraphic(color);
                }
            }

            Log.Message(
                "Tried to get pawn head at path " + pawn.story.HeadGraphicPath.Remove(0, 22)
                + " that was not found. Defaulting...");

            return HeadsVanillaCustom.First().GetGraphic(color);
        }

        public static Graphic_Multi GetStump(Color skinColor)
        {
            BuildDatabaseIfNecessary();
            return stump.GetGraphic(skinColor);
        }

        public static void Reset()
        {
            HeadsVanillaCustom.Clear();
            skull = null;
            stump = null;
        }

        #endregion Public Methods

        #region Public Classes

        public class HeadGraphicRecordVanillaCustom
        {
            #region Public Fields

            public CrownType crownType;

            public Gender gender;

            public string graphicPathVanillaCustom;

            #endregion Public Fields

            #region Private Fields

            private readonly List<KeyValuePair<Color, Graphic_Multi>> graphics =
                new List<KeyValuePair<Color, Graphic_Multi>>();

            #endregion Private Fields

            #region Public Constructors

            public HeadGraphicRecordVanillaCustom(string graphicPath)
            {
                this.graphicPathVanillaCustom = graphicPath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);

                string[] array = fileNameWithoutExtension?.Split('_');

                try
                {
                    this.crownType =
                        (CrownType)(byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType));
                    this.gender = (Gender)(byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    this.crownType = CrownType.Undefined;
                    this.gender = Gender.None;
                }
            }

            #endregion Public Constructors

            #region Public Methods

            public Graphic_Multi GetGraphic(Color color)
            {
                for (int i = 0; i < this.graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(this.graphics[i].Key))
                    {
                        return this.graphics[i].Value;
                    }
                }

                Graphic_Multi graphicMultiHead = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(
                    this.graphicPathVanillaCustom,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color);
                this.graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphicMultiHead));
                return graphicMultiHead;
            }

            #endregion Public Methods
        }

        #endregion Public Classes
    }
}