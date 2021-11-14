﻿using PawnPlus.Parts;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using Verse;

namespace PawnPlus.Defs
{
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
