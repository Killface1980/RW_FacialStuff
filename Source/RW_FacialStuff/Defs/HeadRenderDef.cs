using System;
using System.Collections.Generic;
using Verse;

namespace FacialStuff.Defs
{
	public class HeadRenderDef : Def
	{
		public string headTexture;
		public EyeRenderDef eyeRenderDef;

        public static bool GetCachedHeadRenderParams(
            string headTexturePath, 
            out RenderParam[,] eyeRenderParam)
		{
            if(headTextureMapping.TryGetValue(headTexturePath, out HeadRenderDef headRenderDef))
			{
                if(!headRenderDef._cacheBuilt)
				{
                    headRenderDef.BuildRenderParamCache();
                    headRenderDef._cacheBuilt = true;
                }
                eyeRenderParam = headRenderDef._cachedEyeRenderParam;
                return true;
            }
            eyeRenderParam = null;
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
                int maxIndex = 0;
                foreach(var partRender in headRenderDef.eyeRenderDef.parts)
                {
                    maxIndex = Math.Max(partRender.multiPartIndex, maxIndex);
                }
                headRenderDef._cachedEyeRenderParam = new RenderParam[maxIndex + 1, 4];
                for(int i = 0; i <= maxIndex; ++i)
                {
                    PartRender partRender = headRenderDef.eyeRenderDef?.parts.FindLast(x => x.multiPartIndex == i);
                    for(int j = 0; j < 4; ++j)
                    {
                        Rot4 rotation = new Rot4(j);
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
                        headRenderDef._cachedEyeRenderParam[i, j] = renderInfo;
                    }
                }
            }
        }

        [Unsaved(false)]
        private bool _cacheBuilt = false;

        // First dimension corresponds to <multiPartIndex> field in EyeRenderDef.
        // Second dimension corresponds to rotation, with 0 for north, 1 for east, 2 for south, and 3 for west (same as the internal representation for Rot4)
        [Unsaved(false)]
        private RenderParam[,] _cachedEyeRenderParam;
		
        // This dictionary is populated in FacialStuffModBase.DefsLoaded()
        [Unsaved(false)]
        public static Dictionary<string, HeadRenderDef> headTextureMapping = new Dictionary<string, HeadRenderDef>();
	}
}
