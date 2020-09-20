using FacialStuff.Defs;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FacialStuff
{
	public class PartRender
	{
		public class PerRotation
		{
			public Rot4 rotation;
			public MeshDef meshDef;
			public Vector2 offset;
		}

		public int multiPartIndex;
		public List<PerRotation> perRotation;
	}
}
