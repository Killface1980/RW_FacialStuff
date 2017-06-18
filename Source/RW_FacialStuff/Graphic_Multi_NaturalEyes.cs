using UnityEngine;
using Verse;

namespace RW_FacialStuff
{
    using System;
    using System.IO;

    public class Graphic_Multi_NaturalEyes : Graphic
    {
        private Material[] mats = new Material[4];

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return this.MatBack;
                case 1:
                    return this.MatRight;
                case 2:
                    return this.MatFront;
                case 3:
                    return this.MatLeft;
                default:
                    return BaseContent.BadMat;
            }
        }
        public string GraphicPath
        {
            get
            {
                return path;
            }
        }

        public  Material MatLeft
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

        public  Material MatRight
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
                return this.MatRight == MatBack;
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

            string eyeType = null;
            string side = null;
            string crowntype = null;
            string gender = null;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(req.path);

            string[] array2 = fileNameWithoutExtension.Split('_');
            try
            {
                eyeType = array2[3];
                side = array2[4];
                crowntype = array2[2];
                gender = array2[1];
            }
            catch (Exception ex)
            {
                Log.Error("Parse error with head graphic at " + req.path + ": " + ex.Message);
            }

            if (ContentFinder<Texture2D>.Get(req.path + "_front", false))
            {
                array[2] = ContentFinder<Texture2D>.Get(req.path + "_front");
            }
            else
            {
                Log.Message("Facial Stuff: Failed to get front texture at " + req.path + "_front" + " - Graphic_Multi_NaturalEyes");
                return;
             //   array[2] = MaskTextures.BlankTexture();
            }

            string sidePath = "Eyes/Eye_" + gender + "_" + crowntype + "_" + eyeType + "_side";

            if (ContentFinder<Texture2D>.Get(sidePath, true))
            {
                if (side.Equals("Right"))
                {
                    array[3] = MaskTextures.BlankTexture();
                }
                else
                {
                    array[3] = ContentFinder<Texture2D>.Get(sidePath);
                }

                if (side.Equals("Left"))
                {

                    array[1] = MaskTextures.BlankTexture();
                }
                else
                {
                    array[1] = ContentFinder<Texture2D>.Get(sidePath);
                }
            }
            else
            {
                Log.Message("Facial Stuff: No texture found at " + sidePath + " - Graphic_Multi_AddedHeadParts");
                array[3] = MaskTextures.BlankTexture();
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
                if (array[i] == null)
                {
                    Log.Message("Array = null at: " + i);
                }
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
