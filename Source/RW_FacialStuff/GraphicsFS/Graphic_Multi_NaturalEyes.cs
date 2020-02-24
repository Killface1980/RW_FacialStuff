using System;
using System.IO;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class Graphic_Multi_NaturalEyes : Graphic_Multi
    {
        private readonly Material[] _mats = new Material[4];

        public override Material MatNorth => this._mats[0];
        public override Material MatEast => this._mats[1];
        public override Material MatSouth => this._mats[2];
        public override Material MatWest => this._mats[3];


        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = "Things/Pawn/Humanlike/Eyes/";
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[4];

            string eyeType = null;
            string side = null;
            string gender = null;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(req.path);

            // ReSharper disable once PossibleNullReferenceException
            string[] arrayString = fileNameWithoutExtension.Split('_');
            try
            {
                eyeType = arrayString[1];
                gender = arrayString[2];
                side = arrayString[3];
            }
            catch (Exception ex)
            {
                Log.Error("Parse error with head graphic at " + req.path + ": " + ex.Message);
            }

            string eyeStringBase = this.path + "Eye_" + eyeType + "_" + gender + "_" + side;
            string eyeSouth = eyeStringBase + "_south";
            Log.Warning(eyeSouth);

            if (ContentFinder<Texture2D>.Get(eyeSouth))
            {
                array[2] = ContentFinder<Texture2D>.Get(eyeSouth);
            }
            else
            {
                Log.Message(
                            "Facial Stuff: Failed to get front texture at " + req.path + "_south"
                          + " - Graphic_Multi_NaturalEyes");
                return;

                // array[2] = MaskTextures.BlankTexture();
            }

            string eyeEast = eyeStringBase + "_east";
            Log.Warning(eyeEast);

            // 1 texture= 1 eye, blank for the opposite side
            if (ContentFinder<Texture2D>.Get(eyeEast))
            {


                // ReSharper disable once PossibleNullReferenceException
                if (side.Equals("Right"))
                {
                    array[3] = FaceTextures.BlankTexture;
                }
                else
                {
                    array[3] = ContentFinder<Texture2D>.Get(eyeEast);
                }

                if (side.Equals("Left"))
                {
                    array[1] = FaceTextures.BlankTexture;
                }
                else
                {
                    array[1] = ContentFinder<Texture2D>.Get(eyeEast);
                }
            }
            else
            {
                Log.Message("Facial Stuff: No texture found at " + eyeEast + " - Graphic_Multi_NaturalEyes");
                array[3] = FaceTextures.BlankTexture;
            }

            string eyeNorth = eyeStringBase + "_north";
            if (ContentFinder<Texture2D>.Get(eyeNorth, false))
            {
                array[0] = ContentFinder<Texture2D>.Get(eyeNorth);
            }
            else
            {
                array[0] = FaceTextures.BlankTexture;
            }

            string eyeSouthm = eyeStringBase + "_southm";
            Texture2D[] array2 = new Texture2D[4];
            if (req.shader.SupportsMaskTex())
            {
                array2[0] = FaceTextures.RedTexture;
                array2[2] = ContentFinder<Texture2D>.Get(eyeSouthm, false);
                if (array2[2] == null)
                {
                    array2[2] = FaceTextures.RedTexture;
                }

                string eyeEastm = eyeStringBase + "_eastm";

                // 1 texture= 1 eye, blank for the opposite side

                if (side != null && side.Equals("Right"))
                {
                    array2[3] = FaceTextures.RedTexture;
                }
                else
                {
                    array2[3] = ContentFinder<Texture2D>.Get(eyeEastm, false);
                }

                if (side != null && side.Equals("Left"))
                {
                    array2[1] = FaceTextures.RedTexture;
                }
                else
                {
                    array2[1] = ContentFinder<Texture2D>.Get(eyeEastm, false);
                }
                if (array2[1] == null) { array2[1] = FaceTextures.RedTexture; }
                if (array2[3] == null) { array2[3] = FaceTextures.RedTexture; }


            }

            for (int i = 0; i < 4; i++)
            {
                MaterialRequest req2 = default;
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = this.color;
                req2.colorTwo = this.colorTwo;

                req2.maskTex = array2[i];

                // req2.mainTex.filterMode = FilterMode.Trilinear;
                this._mats[i] = MaterialPool.MatFrom(req2);
            }
        }


        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0: return this.MatNorth;
                case 1: return this.MatEast;
                case 2: return this.MatSouth;
                case 3: return this.MatWest;
                default: return BaseContent.BadMat;
            }
        }
    }
}