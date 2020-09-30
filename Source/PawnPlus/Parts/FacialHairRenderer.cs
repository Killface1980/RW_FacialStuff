using PawnPlus.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Parts
{
	class FacialHairRenderer : SimplePartRenderer
	{
		public override void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			BodyPartRecord bodyPartRecord,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals)
		{
			_graphic = GraphicDatabase.Get<Graphic_Multi>(
				defaultTexPath,
				Shaders.FacePart,
				Vector3.one,
				pawn.story.hairColor);
		}
	}
}
