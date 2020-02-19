using FacialStuff.GraphicsFS;
using UnityEngine;

namespace Verse
{
    public class Graphic_Multi_Four : Graphic_Multi
    {
        private Material[] mats = new Material[4];

        public override Material MatSouth => this.mats[2];

        public override Material MatEast => this.mats[1];

        public override Material MatWest => this.mats[3];

        public override Material MatNorth => this.mats[0];

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return this.MatNorth;

                case 1:
                    return this.MatEast;

                case 2:
                    return this.MatSouth;

                case 3:
                    return this.MatWest;

                default:
                    return BaseContent.BadMat;
            }
        }

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[4];
            array[0] = ContentFinder<Texture2D>.Get(req.path + "_north", false);
            if (array[0] == null)
            {
                Log.Error("Failed to find any texture while constructing " + this);
                return;
            }
            array[1] = ContentFinder<Texture2D>.Get(req.path + "_east", false);
            if (array[1] == null)
            {
                array[1] = array[0];
            }
            array[2] = ContentFinder<Texture2D>.Get(req.path + "_south", false);
            if (array[2] == null)
            {
                array[2] = array[0];
            }
            array[3] = ContentFinder<Texture2D>.Get(req.path + "_west", false);
            if (array[3] == null)
            {
                array[3] = array[1];
            }

            Texture2D[] array2 = new Texture2D[4];
            if (req.shader.SupportsMaskTex())
            {
                array2[0] = ContentFinder<Texture2D>.Get(req.path + "_northm", false);
                if (array2[0] == null)
                {
                    array2[0] = FaceTextures.RedTexture;
                }
                array2[1] = ContentFinder<Texture2D>.Get(req.path + "_eastm", false);
                if (array2[1] == null)
                {
                    array2[1] = FaceTextures.RedTexture;
                }
                array2[2] = ContentFinder<Texture2D>.Get(req.path + "_southm", false);
                if (array2[2] == null)
                {
                    array2[2] = FaceTextures.RedTexture;
                }
                array2[3] = ContentFinder<Texture2D>.Get(req.path + "_westm", false);
                if (array2[3] == null)
                {
                    array2[3] = FaceTextures.RedTexture;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                MaterialRequest req2 = default;
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = this.color;
                req2.colorTwo = this.colorTwo;
                req2.maskTex = array2[i];
                this.mats[i] = MaterialPool.MatFrom(req2);
            }
        }
    }
}