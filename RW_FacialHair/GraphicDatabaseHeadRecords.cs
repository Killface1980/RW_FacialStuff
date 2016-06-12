using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace RW_FacialHair
{

    public class GraphicDatabaseHeadRecords
    {
        private class HeadGraphicRecord
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public HeadGraphicRecord(string graphicPath)
            {
                this.graphicPath = graphicPath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
                string[] array = fileNameWithoutExtension.Split(new char[]
                {
                    '_'
                });
                try
                {
                    this.crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
                    this.gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    this.crownType = CrownType.Undefined;
                    this.gender = Gender.None;
                }
            }

            public Graphic_Multi GetGraphic(Color skinColor)
            {
                for (int i = 0; i < this.graphics.Count; i++)
                {
                    if (skinColor.IndistinguishableFrom(this.graphics[i].Key))
                    {
                        return this.graphics[i].Value;
                    }
                }
                Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(this.graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, skinColor);
                this.graphics.Add(new KeyValuePair<Color, Graphic_Multi>(skinColor, graphic_Multi));
                return graphic_Multi;
            }
        }

        private static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();

        private static HeadGraphicRecord skull = null;

        private static readonly string[] HeadsFolderPaths;
    //  private static readonly string[] HeadsFolderPaths = new string[]
    //  {
    //      "Things/Pawn/Humanlike/Heads/Male",
    //      "Things/Pawn/Humanlike/Heads/Female"
    //  };

        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

        public static Graphic_Multi GetSkull()
        {
            GraphicDatabaseHeadRecords.BuildDatabaseIfNecessary();
            return GraphicDatabaseHeadRecords.skull.GetGraphic(Color.white);
        }

        public static void Reinit()
        {
            heads.Clear();
            skull = null;
        }

        private static void BuildDatabaseIfNecessary()
        {
            if (heads.Count > 0 && skull != null)
            {
                return;
            }
            string[] headsFolderPaths = HeadsFolderPaths;
            for (int i = 0; i < headsFolderPaths.Length; i++)
            {
                string text = headsFolderPaths[i];
                foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
                {
                    heads.Add(new HeadGraphicRecord(text + "/" + current));
                }
            }
            skull = new HeadGraphicRecord(SkullPath);
        }

        public static Graphic_Multi GetHeadNamed(string graphicPath, Color skinColor)
        {
            BuildDatabaseIfNecessary();
            for (int i = 0; i < heads.Count; i++)
            {
                HeadGraphicRecord headGraphicRecord = heads[i];
                if (headGraphicRecord.graphicPath == graphicPath)
                {
                    return headGraphicRecord.GetGraphic(skinColor);
                }
            }
            Log.Message("Tried to get pawn head at path " + graphicPath + " that was not found. Defaulting...");
            return heads.First().GetGraphic(skinColor);
        }


        public static Graphic_Multi GetHeadRandom(Gender gender, Color skinColor, CrownType crownType)
        {
            BuildDatabaseIfNecessary();
            Predicate<HeadGraphicRecord> predicate = (HeadGraphicRecord head) => head.crownType == crownType && head.gender == gender;
            int num = 0;
            HeadGraphicRecord headGraphicRecord;
            while (true)
            {
                headGraphicRecord = heads.RandomElement();
                if (predicate(headGraphicRecord))
                {
                    break;
                }
                num++;
                if (num > 40)
                {
                    goto Block_2;
                }
            }
            return headGraphicRecord.GetGraphic(skinColor);
            Block_2:
            foreach (HeadGraphicRecord current in heads.InRandomOrder(null))
            {
                if (predicate(current))
                {
                    return current.GetGraphic(skinColor);
                }
            }
            Log.Error("Failed to find head for gender=" + gender + ". Defaulting...");
            return heads.First().GetGraphic(skinColor);
        }

        public static Graphic_Multi GetHeadRandom(Gender gender, Color skinColor, Color hairColor, CrownType crownType)
        {
            BuildDatabaseIfNecessary();
            Predicate<HeadGraphicRecord> predicate = (HeadGraphicRecord head) => head.crownType == crownType && head.gender == gender;
            int num = 0;
            HeadGraphicRecord headGraphicRecord;
            while (true)
            {
                headGraphicRecord = heads.RandomElement();
                if (predicate(headGraphicRecord))
                {
                    break;
                }
                num++;
                if (num > 40)
                {
                    goto Block_2;
                }
            }
            return headGraphicRecord.GetGraphic(skinColor);
            Block_2:
            foreach (HeadGraphicRecord current in heads.InRandomOrder(null))
            {
                if (predicate(current))
                {
                    return current.GetGraphic(skinColor);
                }
            }
            Log.Error("Failed to find head for gender=" + gender + ". Defaulting...");
            return heads.First().GetGraphic(skinColor);
        }

    }
}
