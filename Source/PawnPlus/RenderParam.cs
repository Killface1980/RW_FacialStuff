using UnityEngine;

namespace PawnPlus
{
	// RenderParams are designed to be immutable after initialization, so make it struct instead.
	public struct RenderParam
	{
		public bool render;
		public Mesh mesh;
		public Vector3 offset;
	}
}
