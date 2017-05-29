using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    using System.Linq;

    // to do: patch
    //[Detour(typeof(GraphicDatabaseHeadRecords), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
    [HarmonyPatch(typeof(GraphicDatabaseHeadRecords), "Reset")]

    public static class GraphicDatabaseHeadRecordsModded
    {
        static class Reset_Prefix
        {
            [HarmonyPrefix]
            public static void Reset()
            {
                headsVanillaCustom.Clear();
                skull = null;
                stump = null;
            }

        }

        public static List<HeadGraphicRecordVanillaCustom> headsVanillaCustom = new List<HeadGraphicRecordVanillaCustom>();
        public static List<HeadGraphicRecordModded> headsModded = new List<HeadGraphicRecordModded>();

        private static HeadGraphicRecordVanillaCustom skull;
        private static HeadGraphicRecordVanillaCustom stump;

        private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
        private static readonly string StumpPath = "Things/Pawn/Humanlike/Heads/None_Average_Stump";


        public static int headIndex = 0;

        public class HeadGraphicRecordVanillaCustom
        {
            public Gender gender;

            public CrownType crownType;

            public string graphicPathVanillaCustom;

            public static List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public HeadGraphicRecordVanillaCustom(string graphicPath)
            {
                graphicPathVanillaCustom = graphicPath;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);

                string[] array = fileNameWithoutExtension?.Split('_');

                try
                {
                    crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
                    gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    crownType = CrownType.Undefined;
                    gender = Gender.None;
                }


            }

            public Graphic_Multi GetGraphic(Color color)
            {
                foreach (KeyValuePair<Color, Graphic_Multi> graphic_multi in graphics)
                {
                    if (color.IndistinguishableFrom(graphic_multi.Key))
                    {
                        return graphic_multi.Value;
                    }
                }
                Graphic_Multi graphicMultiHead = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPathVanillaCustom, ShaderDatabase.Cutout, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphicMultiHead));
                return graphicMultiHead;
            }
        }


        public class HeadGraphicRecordModded
        {
            public Pawn pawn;

            public List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

            public bool unique = false;

            public Gender gender;

            public CrownType crownType;

            public string graphicPath;

            public string graphicPathModded;

            public HeadGraphicRecordModded(Pawn pawn)
            {
                CompFace faceComp = pawn.TryGetComp<CompFace>();

                this.pawn = pawn;
                graphicPath = pawn.story.HeadGraphicPath;
                graphicPathModded = faceComp.headGraphicIndex;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
                string[] array = fileNameWithoutExtension?.Split('_');
                try
                {
                    crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
                    gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
                }
                catch (Exception ex)
                {
                    Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
                    crownType = CrownType.Undefined;
                    gender = Gender.None;
                }


            }

            public Graphic_Multi GetGraphicBlank(Color color)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    if (color.IndistinguishableFrom(graphics[i].Key))
                    {
                        return graphics[i].Value;
                    }
                }

                Graphic_Multi graphic_Multi_Head = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPathModded, ShaderDatabase.Cutout, Vector2.one, color);
                graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi_Head));

                return graphic_Multi_Head;
            }

        }


        public static Graphic_Multi GetModdedHeadNamed(Pawn pawn, bool useVanilla, Color color)
        {
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (useVanilla)
            {
                foreach (HeadGraphicRecordVanillaCustom headGraphicRecordVanillaCustom in headsVanillaCustom)
                {
                    if (headGraphicRecordVanillaCustom.graphicPathVanillaCustom == pawn.story.HeadGraphicPath.Remove(0, 22))
                    {
                        return headGraphicRecordVanillaCustom.GetGraphic(color);
                    }
                }
            }

            foreach (HeadGraphicRecordModded headGraphicRecordModded in headsModded)
            {
                if (headGraphicRecordModded.graphicPathModded == faceComp.headGraphicIndex)
                {
                    return headGraphicRecordModded.GetGraphicBlank(color);
                }
            }

            Log.Message("Tried to get pawn head at path " + faceComp.headGraphicIndex + " that was not found. Defaulting...");

            return headsVanillaCustom.First().GetGraphic(color);
        }

        public static void BuildDatabaseIfNecessary()
        {
            //  headsVanillaCustom.Clear();
            if (headsVanillaCustom.Count > 0 && skull != null && stump != null)
            {
                return;
            }
            headsVanillaCustom.Clear();

            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Average_Wide"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Female/Female_Narrow_Wide"));

            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Average_Wide"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Normal"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Pointy"));
            headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom("Heads/Male/Male_Narrow_Wide"));

            skull = new HeadGraphicRecordVanillaCustom(SkullPath);
            stump = new HeadGraphicRecordVanillaCustom(StumpPath);

            //   string[] headsFolderPaths = HeadsFolderPaths;
            //   for (int i = 0; i < headsFolderPaths.Length; i++)
            //   {
            //       string text = headsFolderPaths[i];
            //       foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
            //       {
            //           headsVanillaCustom.Add(new HeadGraphicRecordVanillaCustom(text + "/" + current));
            //       }
            //   }
        }

        public static Graphic_Multi GetStump(Color skinColor)
        {
            BuildDatabaseIfNecessary();
            return stump.GetGraphic(skinColor);
        }

        public static List<KeyValuePair<string, Graphic_Multi>> moddedHeadGraphics = new List<KeyValuePair<string, Graphic_Multi>>();

        //   private static List<Texture2D> _textures;


    }
}


