using UnityEngine;
using Verse;

namespace FacialStuff
{
    // public class GraphicModded: Graphic
    // {
    // public virtual Material MatFrontNarrow { get; }
    // public virtual Material MatSideNarrow { get; }
    // }

    // class taken from vanilla, base is Graphic_Multi; needed for adding stuff AFTER game has loaded
    public class Graphic_Multi_NaturalHeadParts : Graphic
    {
        private Material[] mats = new Material[3];

        public string GraphicPath
        {
            get
            {
                return this.path;
            }
        }

        public override Material MatSingle
        {
            get
            {
                return this.mats[2];
            }
        }

        public override Material MatFront
        {
            get
            {
                return this.mats[2];
            }
        }

        public override Material MatSide
        {
            get
            {
                return this.mats[1];
            }
        }

        public override Material MatBack
        {
            get
            {
                return this.mats[0];
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                return this.MatSide == this.MatBack;
            }
        }

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[3];

            if (ContentFinder<Texture2D>.Get(req.path + "_front", false))
            {
                array[2] = ContentFinder<Texture2D>.Get(req.path + "_front");
            }
            else
            {
                Log.Message(
                    "Facial Stuff: Failed to get front texture at " + req.path + "_front"
                    + " - Graphic_Multi_NaturalHeadParts");
                return;
            }

            // array[2] = LoadTexture(req.path + "_front");
            // if (array[2] == null)
            // {
            // Log.Error("FacialStuff: Failed to find any texture while constructing " + ToString());
            // return;
            // }
            if (ContentFinder<Texture2D>.Get(req.path + "_side", false))
            {
                array[1] = ContentFinder<Texture2D>.Get(req.path + "_side");
            }

            // if (array[1] == null)
            // {

            // array[1] = LoadTexture(req.path + "_side");

            // }

            // if (File.Exists(GraphicDatabaseHeadRecordsModded.modpath + req.path + "_back.png"))
            // array[0] = LoadTexture(req.path + "_back");
            // else
            // array[0] = BlankTexture();
            if (ContentFinder<Texture2D>.Get(req.path + "_back", false))
            {
                array[0] = ContentFinder<Texture2D>.Get(req.path + "_back");
            }
            else
            {
                array[0] = MaskTextures.BlankTexture();
            }

            // Texture2D[] array2 = new Texture2D[3];
            // if (req.shader.SupportsMaskTex())
            // {
            // array2[0] = LoadTexture(req.path + "_backm");
            // if (array2[0] != null)
            // {
            // array2[1] = LoadTexture(req.path + "_sidem");
            // if (array2[1] == null)
            // {
            // array2[1] = array2[0];
            // }
            // array2[2] = LoadTexture(req.path + "_frontm");
            // if (array2[2] == null)
            // {
            // array2[2] = array2[0];
            // }
            // }
            // }

            /*
            Texture2D[] array2 = new Texture2D[3];
            if (req.shader.SupportsMaskTex())
            {
                array2[0] = ContentFinder<Texture2D>.Get(req.path + "_backm", false);
                if (array2[0] != null)
                {
                    array2[1] = ContentFinder<Texture2D>.Get(req.path + "_sidem", false);
                    if (array2[1] == null)
                    {
                        array2[1] = array2[0];
                    }
                    array2[2] = ContentFinder<Texture2D>.Get(req.path + "_frontm", false);
                    if (array2[2] == null)
                    {
                        array2[2] = array2[0];
                    }
                }
            }
            */
            for (int i = 0; i < 3; i++)
            {
                MaterialRequest req2 = default(MaterialRequest);
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = this.color;
                req2.colorTwo = this.colorTwo;

                // req2.maskTex = array2[i];
                this.mats[i] = MaterialPool.MatFrom(req2);
            }
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_Multi>(
                this.path,
                newShader,
                this.drawSize,
                newColor,
                newColorTwo,
                this.data);
        }

        public override string ToString()
        {
            return string.Concat(
                "Multi(initPath=",
                this.path,
                ", color=",
                this.color,
                ", colorTwo=",
                this.colorTwo,
                ")");
        }

        public override int GetHashCode()
        {
            int seed = 0;
            seed = Gen.HashCombine(seed, this.path);
            seed = Gen.HashCombineStruct(seed, this.color);
            return Gen.HashCombineStruct(seed, this.colorTwo);
        }
    }
}
