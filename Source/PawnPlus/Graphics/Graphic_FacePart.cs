using RimWorld;
using System;
using System.IO;
using UnityEngine;
using Verse;

namespace PawnPlus.Graphics
{
    public class Graphic_FacePart : Graphic
    {
        private readonly Material[] _mats = new Material[2];

        public override Material MatNorth => null;
        public override Material MatEast => this._mats[1];
        public override Material MatSouth => this._mats[0];
        public override Material MatWest => this._mats[1];
        
        public bool Mirrored { get; set; }

        // The following are not needed because mod uses a separate mesh pool
        public override bool WestFlipped => false;
		public override bool EastFlipped => false;
        public override bool ShouldDrawRotated => false;
        
        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            Texture2D[] array = new Texture2D[2];
                        
            array[0] = ContentFinder<Texture2D>.Get(req.path + "_south");
            array[1] = ContentFinder<Texture2D>.Get(req.path + "_east");

            // No support for mask texture

            for (int i = 0; i < array.Length; i++)
            {
                if(array[i] == null)
				{
                    continue;
				}
                MaterialRequest req2 = default;
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = this.color;
                req2.colorTwo = this.colorTwo;
                req2.maskTex = null;
                _mats[i] = MaterialPool.MatFrom(req2);
            }
        }
        
		public override Mesh MeshAt(Rot4 rot)
		{
            // The default MeshAt() can't be called on this class because actual mirroring is 
            // done by the mesh is obtained from MeshPoolFS
            throw new InvalidOperationException();
		}
	}
}