using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    using System;

    public class Graphic_Multi_EyeWear : Graphic
    {
        private Material[] mats = new Material[4];

        public string GraphicPath
        {
            get
            {
                return path;
            }
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return this.MatBack;
                case 1:
                    return this.MatSide;
                case 2:
                    return this.MatFront;
                case 3:
                    return this.MatSingle;
                default:
                    return BaseContent.BadMat;
            }
        }

        public override Material MatSingle
        {
            get
            {
                return mats[3];
            }
        }

        public override Material MatFront
        {
            get
            {
                return mats[2];
            }
        }

        public override Material MatSide
        {
            get
            {
                return mats[1];
            }
        }

        public override Material MatBack
        {
            get
            {
                return mats[0];
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                return this.MatSide == MatBack;
            }
        }



        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            this.path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[4];

            string basepath = null;
            string usageType = null;

            string fileNameWithoutExtension = req.path;
            string[] array2 = fileNameWithoutExtension.Split('_');
            try
            {
                basepath = array2[0];
                usageType = array2[1];
            }
            catch (Exception ex)
            {
                Log.Error("Parse error with head graphic at " + req.path + ": " + ex.Message);
            }

            string reqPath = basepath + "_" + usageType;

            if (ContentFinder<Texture2D>.Get(reqPath + "_side", false))
            {
                if (usageType.Equals("Right"))
                {
                    array[3] = MaskTextures.BlankTexture();
                }
                else
                {
                    array[3] = ContentFinder<Texture2D>.Get(reqPath + "_side");
                }
            }
            else
            {
                Log.Message("Facial Stuff: Failed to get front texture at " + reqPath + "_side" + " - Graphic_Multi_EyeWear");
                array[3] = MaskTextures.BlankTexture();
            }

            if (ContentFinder<Texture2D>.Get(req.path + "_front", false))
            {
                array[2] = ContentFinder<Texture2D>.Get(req.path + "_front");
            }
            else
            {
                Log.Message("Facial Stuff: Failed to get front texture at " + req.path + "_front" + " - Graphic_Multi_EyeWear");
                array[2] = MaskTextures.BlankTexture();
            }

            if (ContentFinder<Texture2D>.Get(reqPath + "_side", false))
            {
                if (usageType.Equals("Left"))
                {
                    array[1] = MaskTextures.BlankTexture();
                }
                else
                {
                    array[1] = ContentFinder<Texture2D>.Get(reqPath + "_side");
                }
            }
            else
            {
                Log.Message("Facial Stuff: Failed to get front texture at " + reqPath + "_side" + " - Graphic_Multi_EyeWear");
                array[1] = MaskTextures.BlankTexture();
            }

            if (ContentFinder<Texture2D>.Get(req.path + "_back", false))
            {
                array[0] = ContentFinder<Texture2D>.Get(req.path + "_back");
            }
            else
            {
                array[0] = MaskTextures.BlankTexture();
            }

            for (int i = 0; i < 4; i++)
            {
                MaterialRequest req2 = default(MaterialRequest);
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = color;
                req2.colorTwo = colorTwo;
                //        req2.maskTex = array2[i];
                mats[i] = MaterialPool.MatFrom(req2);
            }
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_Multi>(path, newShader, drawSize, newColor, newColorTwo, data);
        }

        public override string ToString()
        {
            return string.Concat("Multi(initPath=", path, ", color=", color, ", colorTwo=", colorTwo, ")");
        }

        public override int GetHashCode()
        {
            int seed = 0;
            seed = Gen.HashCombine(seed, path);
            seed = Gen.HashCombineStruct(seed, color);
            return Gen.HashCombineStruct(seed, colorTwo);
        }
    }

}
