using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FacialStuff.GraphicsFS;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.HairCut
{
    // ReSharper disable once InconsistentNaming
    [StaticConstructorOnStartup]
    public static class CutHairDB
    {
        const string STR_MergedHair = "/Textures/MergedHair/";
        private static readonly Dictionary<GraphicRequest, Graphic> AllGraphics =
            new Dictionary<GraphicRequest, Graphic>();

        private static readonly List<HairCutPawn> PawnHairCache = new List<HairCutPawn>();

        private static Texture2D _maskTexFrontBack;

        private static Texture2D _maskTexSide;

        private static string _mergedHairPath;


        private static string MergedHairPath
        {
            get
            {
                if (!_mergedHairPath.NullOrEmpty())
                {
                    return _mergedHairPath;
                }

                ModMetaData mod = ModLister.AllInstalledMods.FirstOrDefault(
                    x => { return x?.Name != null && x.Active && x.Name.StartsWith("Facial Stuff"); });
                if (mod != null)
                {
                    _mergedHairPath = mod.RootDir + STR_MergedHair;
                }

                return _mergedHairPath;
            }
        }

        // ReSharper disable once MissingXmlDoc
        public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, HeadCoverage coverage)
            where T : Graphic, new()
        {
                        // Added second 'color' to get a separate graphic
            GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, color, null, 0, new List<ShaderParameter>());
            return GetInner<T>(req, coverage);
        }

        public static HairCutPawn GetHairCache(Pawn pawn)
        {
            foreach (HairCutPawn c in PawnHairCache)
            {
                if (c.Pawn == pawn)
                {
                    return c;
                }
            }

            HairCutPawn n = new HairCutPawn { Pawn = pawn };
            PawnHairCache.Add(n);
            return n;
        }

        private static void CutOutHair([NotNull] ref Texture2D hairTex, Texture2D maskTex)
        {
            for (int x = 0; x < hairTex.width; x++)
            {
                for (int y = 0; y < hairTex.height; y++)
                {
                    Color maskColor = maskTex.GetPixel(x, y);
                    Color hairColor = hairTex.GetPixel(x, y);

                    Color finalColor1 = hairColor * maskColor;

                    hairTex.SetPixel(x, y, finalColor1);
                }
            }

            hairTex.Apply();
        }

        private static T GetInner<T>(GraphicRequest req, HeadCoverage coverage)
            where T : Graphic, new()
        {
            string oldPath = req.path;
            string name = Path.GetFileNameWithoutExtension(oldPath);

            string newPath = MergedHairPath + name + "_" + coverage;
            req.path = newPath;

            if (AllGraphics.TryGetValue(req, out Graphic graphic))
            {
                return (T)graphic;
            }

            // no graphics found, create =>
            graphic = Activator.CreateInstance<T>();

            // // Check if textures already present and readable, else create
            if (ContentFinder<Texture2D>.Get(req.path + "_north", false) != null)
            {
                graphic.Init(req);

                // graphic.MatSouth.mainTexture = ContentFinder<Texture2D>.Get(newPath + "_south");
                // graphic.MatEast.mainTexture = ContentFinder<Texture2D>.Get(newPath + "_east");
                // graphic.MatNorth.mainTexture = ContentFinder<Texture2D>.Get(newPath + "_north");
            }
            else
            {
                req.path = oldPath;
                graphic.Init(req);

                Texture2D temptexturefront = graphic.MatSouth.mainTexture as Texture2D;
                Texture2D temptextureside = graphic.MatEast.mainTexture as Texture2D;
                Texture2D temptextureback = graphic.MatNorth.mainTexture as Texture2D;

                Texture2D temptextureside2 = (graphic as Graphic_Multi)?.MatWest.mainTexture as Texture2D;

                temptexturefront = FaceTextures.MakeReadable(temptexturefront);
                temptextureside = FaceTextures.MakeReadable(temptextureside);
                temptextureback = FaceTextures.MakeReadable(temptextureback);

                temptextureside2 = FaceTextures.MakeReadable(temptextureside2);

                    // new mask textures 
                if (coverage == HeadCoverage.UpperHead)
                {
                    _maskTexFrontBack = FaceTextures.MaskTexUppherheadFrontBack;
                    _maskTexSide = FaceTextures.MaskTexUpperheadSide;
                }
                else
                {
                    _maskTexFrontBack = FaceTextures.MaskTexFullheadFrontBack;
                    _maskTexSide = FaceTextures.MaskTexFullheadSide;
                }


                CutOutHair(ref temptexturefront, _maskTexFrontBack);

                CutOutHair(ref temptextureside, _maskTexSide);

                CutOutHair(ref temptextureback, _maskTexFrontBack);

                CutOutHair(ref temptextureside2, _maskTexSide);

                req.path = newPath;

                // if (!name.NullOrEmpty() && !File.Exists(req.path + "_south.png"))
                // {
                // byte[] bytes = temptexturefront.EncodeToPNG();
                // File.WriteAllBytes(req.path + "_south.png", bytes);
                // byte[] bytes2 = temptextureside.EncodeToPNG();
                // File.WriteAllBytes(req.path + "_east.png", bytes2);
                // byte[] bytes3 = temptextureback.EncodeToPNG();
                // File.WriteAllBytes(req.path + "_north.png", bytes3);
                // }
                temptexturefront.Compress(true);
                temptextureside.Compress(true);
                temptextureback.Compress(true);
                temptextureside2.Compress(true);

                temptexturefront.mipMapBias = 0.5f;
                temptextureside.mipMapBias = 0.5f;
                temptextureback.mipMapBias = 0.5f;
                temptextureside2.mipMapBias = 0.5f;

                temptexturefront.Apply(false, true);
                temptextureside.Apply(false, true);
                temptextureback.Apply(false, true);
                temptextureside2.Apply(false, true);

                graphic.MatSouth.mainTexture = temptexturefront;
                graphic.MatEast.mainTexture = temptextureside;
                graphic.MatNorth.mainTexture = temptextureback;
                ((Graphic_Multi) graphic).MatWest.mainTexture = temptextureside2;

                // Object.Destroy(temptexturefront);
                // Object.Destroy(temptextureside);
                // Object.Destroy(temptextureback);
            }

            AllGraphics.Add(req, graphic);

            // }
            return (T)graphic;
        }

        public static void ExportHairCut(
            HairDef hairDef,
            string name)
        {
            string path = MergedHairPath + name;
            if (!name.NullOrEmpty() && !File.Exists(path + "_Upperhead_south.png"))
            {
                LongEventHandler.ExecuteWhenFinished(
                    delegate
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

                        SetTempTextures(graphic, out Texture2D temptexturefront, out Texture2D temptextureside, out Texture2D temptextureback, out Texture2D temptextureside2);

                        string upperPath = path + "_Upperhead";

                        _maskTexFrontBack = FaceTextures.MaskTexUppherheadFrontBack;
                        _maskTexSide = FaceTextures.MaskTexUpperheadSide;

                        CutOutHair(upperPath, ref temptexturefront, ref temptextureside, ref temptextureback, ref temptextureside2);

                        SetTempTextures(graphic, out temptexturefront, out temptextureside, out temptextureback, out temptextureside2);

                        string fullPath = path + "_Fullhead";

                        _maskTexFrontBack = FaceTextures.MaskTexFullheadFrontBack;
                        _maskTexSide = FaceTextures.MaskTexFullheadSide;

                        CutOutHair(fullPath, ref temptexturefront, ref temptextureside, ref temptextureback, ref temptextureside2);
                    });
            }
        }

        private static void SetTempTextures(Graphic graphic, out Texture2D temptexturefront, out Texture2D temptextureside, out Texture2D temptextureback, out Texture2D temptextureside2)
        {
            temptexturefront = graphic.MatSouth.mainTexture as Texture2D;
            temptextureside = graphic.MatEast.mainTexture as Texture2D;
            temptextureback = graphic.MatNorth.mainTexture as Texture2D;
            temptextureside2 = (graphic as Graphic_Multi)?.MatWest.mainTexture as Texture2D;

            temptexturefront = FaceTextures.MakeReadable(temptexturefront);
            temptextureside = FaceTextures.MakeReadable(temptextureside);
            temptextureback = FaceTextures.MakeReadable(temptextureback);
            temptextureside2 = FaceTextures.MakeReadable(temptextureside2);
        }

        private static void CutOutHair(string exportPath, ref Texture2D temptexturefront, ref Texture2D temptextureside, ref Texture2D temptextureback, ref Texture2D temptextureside2)
        {
            CutOutHair(ref temptexturefront, _maskTexFrontBack);

            CutOutHair(ref temptextureside, _maskTexSide);

            CutOutHair(ref temptextureback, _maskTexFrontBack);
            CutOutHair(ref temptextureside2, _maskTexSide);



            byte[] bytes = temptexturefront.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_south.png", bytes);
            byte[] bytes2 = temptextureside.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_east.png", bytes2);
            byte[] bytes3 = temptextureback.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_north.png", bytes3);
            byte[] bytes4 = temptextureside2.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_west.png", bytes2);
        }
    }
}