using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnPlus.Defs
{
	public class HeadRenderDef : Def
	{
		public string headTexture;
		public EyeRenderDef eyeRenderDef;
		public MouthRenderDef mouthRenderDef;

        public static bool GetCachedHeadRenderParams(
            BodyDef bodyDef,
            string headTexturePath, 
            out Dictionary<int, RenderParam[]> eyeRenderParam,
            out RenderParam[] mouthRenderParam)
		{
            if(headTextureMapping.TryGetValue(headTexturePath, out HeadRenderDef headRenderDef))
			{
                if(!headRenderDef._cacheBuilt)
				{
                    headRenderDef.BuildRenderParamCache(bodyDef);
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
        private void BuildRenderParamCache(BodyDef bodyDef)
		{          
            // It is possible to extract rendering parameters from HeadRenderDef itself, but this requires some linear search (complexity of O(n)) and 
            // reference chasing. This alone is rather trivial but may have impact if there are a lot of pawns. Building caches for the parameters using 
            // multidimensional array offers free performance benefits and makes it easier to scale.
            // RenderInfo caches are built here because Verse.MeshPool's static constructor needs to be called before building the cache.

            // Build RenderInfo cache for eyes.
            var headRenderDefList = DefDatabase<HeadRenderDef>.AllDefsListForReading;
            foreach(var headRenderDef in headRenderDefList)
            {
                if(headRenderDef.eyeRenderDef != null)
                {
                    headRenderDef._cachedEyeRenderParam = new Dictionary<int, RenderParam[]>();
                    for(int i = 0; i < headRenderDef.eyeRenderDef.parts.Count; ++i)
                    {
                        PartRender partRender = headRenderDef.eyeRenderDef.parts[i];
                        if(!partRender.linkedRacesBodyPart.TryGetValue(bodyDef, out BodyPartLocator bodyPartLocator))
						{
                            // TODO log
                            continue;
						}
                        bodyPartLocator.LocateBodyPart(bodyDef);
                        if(bodyPartLocator._resolvedPartIndex < 0)
						{
                            // TODO log
                            continue;
						}
                        partRender.BuildRenderParamCache();
                        RenderParam[] renderParams = partRender._cachedRenderParam;
                        if(!headRenderDef._cachedEyeRenderParam.ContainsKey(bodyPartLocator._resolvedPartIndex))
						{
                            headRenderDef._cachedEyeRenderParam.Add(bodyPartLocator._resolvedPartIndex, null);
                        }
                        headRenderDef._cachedEyeRenderParam[bodyPartLocator._resolvedPartIndex] = renderParams;
                    }
                }
                else
				{
                    headRenderDef._cachedEyeRenderParam = null;
                }
                
                // Build RenderInfo cache for mouth
                RenderParam[] mouthRenderParam = null;
                if(headRenderDef.mouthRenderDef != null)
                {
                    headRenderDef.mouthRenderDef.part.BuildRenderParamCache();
                    mouthRenderParam = headRenderDef.mouthRenderDef.part._cachedRenderParam;
                }
                headRenderDef._cachedMouthRenderParam = mouthRenderParam;
            }
        }

        [Unsaved(false)]
        private bool _cacheBuilt = false;

        // First dimension corresponds to <multiPartIndex> field in EyeRenderDef.
        // Second dimension corresponds to rotation, with 0 for north, 1 for east, 2 for south, and 3 for west (same as the internal representation for Rot4)
        [Unsaved(false)]
        private Dictionary<int, RenderParam[]> _cachedEyeRenderParam;
		
		// Index corresponds to the rotation in the same manner as _cachedEyeRenderParam.
		[Unsaved(false)]
        private RenderParam[] _cachedMouthRenderParam;

        // This dictionary is populated in FacialStuffModBase.DefsLoaded()
        [Unsaved(false)]
        public static Dictionary<string, HeadRenderDef> headTextureMapping = new Dictionary<string, HeadRenderDef>();
	}
}
