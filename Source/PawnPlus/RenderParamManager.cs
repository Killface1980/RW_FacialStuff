namespace PawnPlus
{
    using System.Collections.Generic;

    using PawnPlus.Defs;
    using PawnPlus.Parts;

    using Verse;

    public static class RenderParamManager
	{
		private class RenderNode
		{
			private RootType _rootType;
			private Dictionary<string, RenderParam[]> _rootTextureRenderInfoMapping = new Dictionary<string, RenderParam[]>();

			public RootType RootType => _rootType;

			public RenderNode(RootType rootType)
			{
				_rootType = rootType;
			}

			public void BuildRenderParamFromRenderInfo(string rootTexture, List<RenderNodeMappingDef.RenderInfo> renderInfo)
			{
				RenderParam[] renderParams = new RenderParam[4];
				for(int i = 0; i < 4; ++i)
                {
                    Rot4 rotation = new Rot4(i);
                    RenderParam renderParam = new RenderParam();
                    int partRenderIdx = 0;

                    // Check partRenderIdx >= 0 after calling FindLastIndex()
                    if ((partRenderIdx = renderInfo.FindLastIndex(x => x.rotation == rotation)) >= 0)
                    {
                        RenderNodeMappingDef.RenderInfo rotationParam = renderInfo[partRenderIdx];
                        renderParam.render = true;
                        renderParam.offset = rotationParam.offset;
                        renderParam.mesh = rotationParam.meshDef.Mesh;
                    }
                    else
                    {
                        // If there is no render info for <multiPartIndex> and direction, do not render.
                        renderParam.render = false;
                    }

                    renderParams[i] = renderParam;
                }

                _rootTextureRenderInfoMapping[rootTexture] = renderParams;
			}

			public void GetRenderParams(Pawn pawn, out RootType rootType, out RenderParam[] renderParams)
			{
				string rootTexturePath = null;
				rootType = _rootType;
				switch(_rootType)
				{
					case RootType.Body:
						rootTexturePath = pawn.story.bodyType.bodyNakedGraphicPath;
						break;

					case RootType.Head:
						rootTexturePath = pawn.story.HeadGraphicPath;
						break;
				}
				if(rootTexturePath == null)
				{
					renderParams = null;
					return;
				}

				_rootTextureRenderInfoMapping.TryGetValue(rootTexturePath, out renderParams);
			}
		}

		private static bool _initialized;
		private static Dictionary<string, RenderNode> _renderNodes = new Dictionary<string, RenderNode>();

		public static bool Initialized => _initialized;

		public static void ReadFromRenderDefs()
		{
			List<RenderNodeMappingDef> renderDefList = DefDatabase<RenderNodeMappingDef>.AllDefsListForReading;
			foreach(RenderNodeMappingDef renderDef in renderDefList)
			{
				if(!_renderNodes.ContainsKey(renderDef.renderNodeName))
				{
					_renderNodes.Add(renderDef.renderNodeName, new RenderNode(renderDef.rootType));
				}

				RenderNode renderNode = _renderNodes[renderDef.renderNodeName];
				if(renderNode.RootType != renderDef.rootType)
				{
					Log.Warning(
						"Pawn Plus: all RenderNodeMappingDefs defining the same <renderNode> must have the same <rootType>. RenderDef " + 
						renderDef.defName + 
						" will be ignored.");
					continue;
				}

				foreach(RenderNodeMappingDef.RootTextureRenderInfoMapping rootTextureToRenderInfo in renderDef.rootTexturesToRenderInfoMapping)
				{
					foreach(string rootTexturePath in rootTextureToRenderInfo.rootTexturePaths)
					{
						renderNode.BuildRenderParamFromRenderInfo(rootTexturePath, rootTextureToRenderInfo.renderInfoPerRotation);
					}
				}
			}

			_initialized = true;
		}

		public static void GetRenderParams(Pawn pawn, string renderNodeName, out RootType rootType, out RenderParam[] renderParams)
		{
			if(_renderNodes.TryGetValue(renderNodeName, out RenderNode renderNode))
			{
				renderNode.GetRenderParams(pawn, out rootType, out renderParams);
				return;
			}

			rootType = RootType.Body;
			renderParams = null;
		}
	}
}
