// ReSharper disable NonReadonlyMemberInGetHashCode

using System;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class Graphic_Multi_AddedHeadParts : Graphic
    {
        private readonly Material[] _mats = new Material[4];

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo) => GraphicDatabase.Get<Graphic_Multi>(path,
                newShader, drawSize,
                newColor,
                newColorTwo, data);

        public override int GetHashCode()
        {
            int seed = 0;
            seed = Gen.HashCombine(seed, path);
            seed = Gen.HashCombineStruct(seed, color);
            return Gen.HashCombineStruct(seed, colorTwo);
        }

        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[4];

            string addedpartName = null;
            string side = null;
            string crowntype = null;

            string fileNameWithoutExtension = req.path;
            string[] array2 = fileNameWithoutExtension.Split('_');
            try
            {
                addedpartName = array2[0];
                side = array2[1];
                crowntype = array2[2];
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
                Log.Message(
                    "Facial Stuff: Failed to get front texture at " + req.path + "_front"
                    + " - Graphic_Multi_AddedHeadParts");
                return;

                // array[2] = MaskTextures.BlankTexture();
            }

            Texture2D sideTex = ContentFinder<Texture2D>.Get(addedpartName + "_" + crowntype + "_side", false);
            Texture2D side2Tex = ContentFinder<Texture2D>.Get(addedpartName + "_" + crowntype + "_side2", false);
            Texture2D backTex = ContentFinder<Texture2D>.Get(req.path + "_back", false);

            if (sideTex.NullOrBad())
            {
                Log.Message(
                    "Facial Stuff: No texture found at " + addedpartName + "_" + crowntype + "_side"
                    + " - Graphic_Multi_AddedHeadParts. This message is just a note, no error.");
                array[3] = FaceTextures.BlankTexture;
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                if (side.Equals("Right"))
                {
                    if (!side2Tex.NullOrBad())
                    {
                        array[3] = side2Tex;
                    }
                    else
                    {
                        array[3] = FaceTextures.BlankTexture;
                    }
                }
                else
                {
                    array[3] = sideTex;
                }

                if (side.Equals("Left"))
                {
                    if (side2Tex.NullOrBad())
                    {
                        array[1] = FaceTextures.BlankTexture;
                    }
                    else
                    {
                        array[1] = side2Tex;
                    }
                }
                else
                {
                    array[1] = sideTex;
                }
            }

            if (backTex)
            {
                array[0] = backTex;
            }
            else
            {
                array[0] = FaceTextures.BlankTexture;
            }

            for (int i = 0; i < 4; i++)
            {
                if (array[i] == null)
                {
                    Log.Message("Array = null at: " + i);
                }

                MaterialRequest req2 = default;
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = color;
                req2.colorTwo = colorTwo;

                // ReSharper disable once PossibleNullReferenceException
                req2.mainTex.filterMode = FilterMode.Trilinear;

                // req2.maskTex = array2[i];
                _mats[i] = MaterialPool.MatFrom(req2);
            }
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0: return MatBack;
                case 1: return MatRight;
                case 2: return MatFront;
                case 3: return MatLeft;
                default: return BaseContent.BadMat;
            }
        }

        public override string ToString() => string.Concat(
                "Multi(initPath=", path,
                ", color=", color,
                ", colorTwo=", colorTwo,
                ")");

        public string GraphicPath => path;

        public override Material MatBack => _mats[0];

        public override Material MatFront => _mats[2];

        public Material MatLeft => _mats[3];

        public Material MatRight => _mats[1];

        public override bool ShouldDrawRotated => MatRight == MatBack;
    }
}