using System.Collections.Generic;
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
        public static readonly List<HeadGraphicRecordVanillaCustom> HeadsVanillaCustom =
            new List<HeadGraphicRecordVanillaCustom>();

        #endregion Public Fields

        #region Private Fields

        [NotNull]
        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

        [NotNull]
        private static readonly string StumpPath = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

        private static HeadGraphicRecordVanillaCustom _skull;

        private static HeadGraphicRecordVanillaCustom _stump;

        #endregion Private Fields

        #region Public Methods

        public static void BuildDatabaseIfNecessary()
        {
            if (HeadsVanillaCustom.Count > 0 && _skull != null && _stump != null)
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

            _skull = new HeadGraphicRecordVanillaCustom(SkullPath);
            _stump = new HeadGraphicRecordVanillaCustom(StumpPath);
        }

        public static Graphic_Multi GetModdedHeadNamed([NotNull] Pawn pawn, Color color)
        {
            BuildDatabaseIfNecessary();
            foreach (HeadGraphicRecordVanillaCustom headGraphicRecordVanillaCustom in HeadsVanillaCustom)
            {
                if (headGraphicRecordVanillaCustom.GraphicPathVanillaCustom == pawn.story?.HeadGraphicPath?.Remove(0, 22))
                {
                    // Log.Message("Getting vanilla " + pawn.story.HeadGraphicPath.Remove(0, 22) + ".");
                    return headGraphicRecordVanillaCustom.GetGraphic(color);
                }
            }

            Log.Message(
                "Tried to get pawn head at path " + pawn.story?.HeadGraphicPath?.Remove(0, 22)
                + " that was not found. Defaulting...");

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

        public class HeadGraphicRecordVanillaCustom
        {
            #region Public Fields

            public string GraphicPathVanillaCustom;

            #endregion Public Fields

            #region Private Fields

            private readonly List<KeyValuePair<Color, Graphic_Multi>> _graphics =
                new List<KeyValuePair<Color, Graphic_Multi>>();

            #endregion Private Fields

            #region Public Constructors

            public HeadGraphicRecordVanillaCustom(string graphicPath)
            {
                this.GraphicPathVanillaCustom = graphicPath;
            }

            #endregion Public Constructors

            #region Public Methods

            public Graphic_Multi GetGraphic(Color color)
            {
                for (int i = 0; i < this._graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(this._graphics[i].Key))
                    {
                        return this._graphics[i].Value;
                    }
                }

                Graphic_Multi graphicMultiHead = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(this.GraphicPathVanillaCustom,
                    ShaderDatabase.CutoutSkin,
                    Vector2.one,
                    color);
                this._graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphicMultiHead));
                return graphicMultiHead;
            }

            #endregion Public Methods
        }

        #endregion Public Classes
    }
}