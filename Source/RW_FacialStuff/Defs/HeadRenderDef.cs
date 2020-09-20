using System;
using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
	public class HeadRenderDef : Def
	{
		public string headTexture;
		public EyeRenderDef eyeRenderDef;
		public MouthRenderDef mouthRenderDef;

        public static bool GetCachedHeadRenderParams(
            string headTexturePath, 
            out RenderParam[,] eyeRenderParam,
            out RenderParam[] mouthRenderParam)
		{
            if(headTextureMapping.TryGetValue(headTexturePath, out HeadRenderDef headRenderDef))
			{
                if(!headRenderDef._cacheBuilt)
				{
                    headRenderDef.BuildRenderParamCache();
                    headRenderDef._cacheBuilt = true;
                }
                eyeRenderParam = headRenderDef._cachedEyeRenderParam;
                mouthRenderParam = headRenderDef._cachedMouthRenderParam;
                return true;
            }
            eyeRenderParam = null;
            mouthRenderParam = null;
            return false;
        }

        // The caches can't be built in static constructor because Unity crashes when trying to create mesh while loading. 
        // Therefore, meshes need to be created in game.
        private void BuildRenderParamCache()
		{          
            // It is possible to extract rendering parameters from HeadRenderDef itself, but this requires some linear search (complexity of O(n)) and 
            // reference chasing. This alone is rather trivial but may have impact if there are a lot of pawns. Building caches for the parameters using 
            // multidimensional array offers free performance benefits and makes it easier to scale.
            // RenderInfo caches are built here because Verse.MeshPool's static constructor needs to be called before building the cache.

            // Build RenderInfo cache for eyes.
            var headRenderDefList = DefDatabase<HeadRenderDef>.AllDefsListForReading;
            foreach(var headRenderDef in headRenderDefList)
            {
                // Build RenderInfo cache for eyes
                int maxEyeIndex = 0;
                foreach(var partRender in headRenderDef.eyeRenderDef.parts)
                {
                    maxEyeIndex = Math.Max(partRender.multiPartIndex, maxEyeIndex);
                }
                headRenderDef._cachedEyeRenderParam = new RenderParam[maxEyeIndex + 1, 4];
                for(int i = 0; i <= maxEyeIndex; ++i)
                {
                    RenderParam[] renderParams = 
                        GetRenderParamForPart(headRenderDef.eyeRenderDef?.parts.FindLast(x => x.multiPartIndex == i));
                    for(int j = 0; j < 4; ++j)
					{
                        headRenderDef._cachedEyeRenderParam[i, j] = renderParams[j];
                    }
                }
                // Build RenderInfo cache for mouth
                headRenderDef._cachedMouthRenderParam = GetRenderParamForPart(headRenderDef.mouthRenderDef?.part);
            }
        }

        private RenderParam[] GetRenderParamForPart(PartRender partRender)
		{
            if(partRender == null)
			{
                return null;
			}
            RenderParam[] renderParams = new RenderParam[4];
            for(int i = 0; i < 4; ++i)
            {
                Rot4 rotation = new Rot4(i);
                RenderParam renderInfo = new RenderParam();
                int partRenderIdx = 0;
                if(partRender != null &&
                    // If partRender isn't null, check partRenderIdx >= 0 after calling FindLastIndex()
                    (partRenderIdx = partRender.perRotation.FindLastIndex(x => x.rotation == rotation)) >= 0)
                {
                    var perRotation = partRender.perRotation[partRenderIdx];
                    renderInfo.render = true;
                    renderInfo.offset = perRotation.offset;
                    renderInfo.mesh = perRotation.meshDef.mirror ?
                        MeshPool.GridPlaneFlip(perRotation.meshDef.dimension) :
                        MeshPool.GridPlane(perRotation.meshDef.dimension);
                } else
                {
                    // If there is no render info for <multiPartIndex> and direction, do not render.
                    renderInfo.render = false;
                }
                renderParams[i] = renderInfo;
            }
            return renderParams;
        }

        [Unsaved(false)]
        private bool _cacheBuilt = false;

        // First dimension corresponds to <multiPartIndex> field in EyeRenderDef.
        // Second dimension corresponds to rotation, with 0 for north, 1 for east, 2 for south, and 3 for west (same as the internal representation for Rot4)
        [Unsaved(false)]
        private RenderParam[,] _cachedEyeRenderParam;
		
		// Index corresponds to the rotation in the same manner as _cachedEyeRenderParam.
		[Unsaved(false)]
        private RenderParam[] _cachedMouthRenderParam;

        // This dictionary is populated in FacialStuffModBase.DefsLoaded()
        [Unsaved(false)]
        public static Dictionary<string, HeadRenderDef> headTextureMapping = new Dictionary<string, HeadRenderDef>();
	}
}
