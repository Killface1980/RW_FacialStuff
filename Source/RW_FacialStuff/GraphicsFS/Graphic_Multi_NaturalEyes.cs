using System;
using System.IO;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class Graphic_Multi_NaturalEyes : Graphic
    {
        private readonly Material[] _mats = new Material[4];

        public string GraphicPath => this.path;

        public override Material MatBack => this._mats[0];
        public override Material MatFront => this._mats[2];
        public Material MatLeft => this._mats[3];
        public Material MatRight => this._mats[1];

        public override bool ShouldDrawRotated => this.MatRight == this.MatBack;

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_Multi>(this.path,
                                                      newShader, this.drawSize,
                                                      newColor,
                                                      newColorTwo, this.data);
        }

        public override int GetHashCode()
        {
            int seed = 0;
            seed = Gen.HashCombine(seed, this.path);
            seed = Gen.HashCombineStruct(seed, this.color);

            return Gen.HashCombineStruct(seed, this.colorTwo);
        }

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[4];

            string eyeType = null;
            string side = null;
            string gender = null;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(req.path);

            // ReSharper disable once PossibleNullReferenceException
            string[] array2 = fileNameWithoutExtension.Split('_');
            try
            {
                eyeType = array2[1];
                gender = array2[2];
                side = array2[3];
            }
            catch (Exception ex)
            {
                Log.Error("Parse error with head graphic at " + req.path + ": " + ex.Message);
            }

            if (ContentFinder<Texture2D>.Get(req.path + "_front"))
            {
                array[2] = ContentFinder<Texture2D>.Get(req.path + "_front");
            }
            else
            {
                Log.Message(
                            "Facial Stuff: Failed to get front texture at " + req.path + "_front"
                          + " - Graphic_Multi_NaturalEyes");
                return;

                // array[2] = MaskTextures.BlankTexture();
            }

            string sidePath = Path.GetDirectoryName(req.path) +"/Eye_" + eyeType+"_" + gender + "_side";

            // 1 texture= 1 eye, blank for the opposite side
            if (ContentFinder<Texture2D>.Get(sidePath))
            {
                // ReSharper disable once PossibleNullReferenceException
                if (side.Equals("Right"))
                {
                    array[3] = FaceTextures.BlankTexture;
                }
                else
                {
                    array[3] = ContentFinder<Texture2D>.Get(sidePath);
                }

                if (side.Equals("Left"))
                {
                    array[1] = FaceTextures.BlankTexture;
                }
                else
                {
                    array[1] = ContentFinder<Texture2D>.Get(sidePath);
                }
            }
            else
            {
                Log.Message("Facial Stuff: No texture found at " + sidePath + " - Graphic_Multi_NaturalEyes");
                array[3] = FaceTextures.BlankTexture;
            }

            if (ContentFinder<Texture2D>.Get(req.path + "_back", false))
            {
                array[0] = ContentFinder<Texture2D>.Get(req.path + "_back");
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

                MaterialRequest req2 = default(MaterialRequest);
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = this.color;
                req2.colorTwo = this.colorTwo;

                // ReSharper disable once PossibleNullReferenceException
                req2.mainTex.filterMode = FilterMode.Trilinear;

                // req2.maskTex = array2[i];
                this._mats[i] = MaterialPool.MatFrom(req2);
            }
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0: return this.MatBack;
                case 1: return this.MatRight;
                case 2: return this.MatFront;
                case 3: return this.MatLeft;
                default: return BaseContent.BadMat;
            }
        }

        public override string ToString()
        {
            return string.Concat(
                                 "Multi(initPath=", this.path,
                                 ", color=", this.color,
                                 ", colorTwo=", this.colorTwo,
                                 ")");
        }
    }
}