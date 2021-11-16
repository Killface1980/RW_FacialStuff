namespace PawnPlus.Defs
{
    using System.Collections.Generic;

    using PawnPlus.Parts;

    using UnityEngine;

    using Verse;

    public class RenderNodeMappingDef : Def
	{
		public class RenderInfo
		{
			public Rot4 rotation;
			public MeshDef meshDef;
			public Vector3 offset;
		}

		public class RootTextureRenderInfoMapping
		{
			public List<string> rootTexturePaths;
			public List<RenderInfo> renderInfoPerRotation;
		}

		public string renderNodeName;
		
		public RootType rootType;

		public List<RootTextureRenderInfoMapping> rootTexturesToRenderInfoMapping;
	}
}
