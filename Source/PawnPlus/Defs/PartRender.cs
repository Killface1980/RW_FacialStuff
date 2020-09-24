using PawnPlus.Defs;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PawnPlus
{	
	public class PartRender
	{
		public enum Attachment
		{
            Body = 0,
            Head = 1
        }

		public class PerRotation
		{
			public Rot4 rotation;
			public MeshDef meshDef;
			public Vector3 offset;
		}
		
        public Attachment attachment;
		public Dictionary<BodyDef, BodyPartLocator> linkedRacesBodyPart = new Dictionary<BodyDef, BodyPartLocator>();
		public List<PerRotation> perRotation = new List<PerRotation>();

		[Unsaved(false)]
		public RenderParam[] _cachedRenderParam;

		public void BuildRenderParamCache()
		{
            _cachedRenderParam = new RenderParam[4];
            for(int i = 0; i < 4; ++i)
            {
                Rot4 rotation = new Rot4(i);
                RenderParam renderInfo = new RenderParam();
                int partRenderIdx = 0;
                // Check partRenderIdx >= 0 after calling FindLastIndex()
                if((partRenderIdx = perRotation.FindLastIndex(x => x.rotation == rotation)) >= 0)
                {
                    var rotationParam = perRotation[partRenderIdx];
                    renderInfo.render = true;
                    renderInfo.offset = rotationParam.offset;
                    renderInfo.mesh = rotationParam.meshDef.mirror ?
                        MeshPool.GridPlaneFlip(rotationParam.meshDef.dimension) :
                        MeshPool.GridPlane(rotationParam.meshDef.dimension);
                } else
                {
                    // If there is no render info for <multiPartIndex> and direction, do not render.
                    renderInfo.render = false;
                }
                _cachedRenderParam[i] = renderInfo;
            }
        }
	}
}
