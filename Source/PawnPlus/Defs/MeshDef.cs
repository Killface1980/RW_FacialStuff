using UnityEngine;
using Verse;

namespace PawnPlus.Defs
{
	public class MeshDef : Def
	{
		public Vector2 dimension;
		public bool mirror;

		[Unsaved]
		private Mesh _cachedMesh;

		// Unity crashes when trying to load the mesh before the map is loaded. To avoid crash load meshes as needed instead.
		public Mesh Mesh
		{ 
			get
			{
				if(_cachedMesh == null)
				{
					_cachedMesh = mirror ? MeshPool.GridPlaneFlip(dimension) : MeshPool.GridPlane(dimension);
				}
				return _cachedMesh;
			}
		}
	}
}
