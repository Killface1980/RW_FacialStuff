using PawnPlus.Defs;
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
	class HumanMouthRenderer : IPartRenderer
	{
		public BodyPartLocator mouthPartLocator;

		private Pawn _pawn;
		private MatProps_Multi _normal;
		private MatProps_Multi _happy;
		private MatProps_Multi _minor;
		private MatProps_Multi _major;
		private MatProps_Multi _extreme;
		private MatProps_Multi _crying;
		private MatProps_Multi _dead;
		private MatProps_Multi _curGraphic;
		private int _ticksSinceLastUpdate;

		public void Initialize(
			Pawn pawn,
			BodyDef bodyDef,
			string defaultTexPath,
			Dictionary<string, string> namedTexPaths,
			BodyPartSignals bodyPartSignals,
			ref TickDelegate tickDelegate)
		{
			_pawn = pawn;
			MatProps_Multi defaultGraphic = MatProps_Multi.Create(defaultTexPath);
			Dictionary<string, MatProps_Multi> namedGraphics = new Dictionary<string, MatProps_Multi>()
			{
				{ "Normal", null },
				{ "Happy", null },
				{ "Minor", null },
				{ "Major", null },
				{ "Extreme", null },
				{ "Crying", null },
				{ "Dead", null }
			}; 
			foreach(string key in new List<string>(namedGraphics.Keys))
			{
				if(namedTexPaths.ContainsKey(key))
				{
					namedGraphics[key] = MatProps_Multi.Create(namedTexPaths[key]);
				} else
				{
					namedGraphics[key] = defaultGraphic;
				}
			}
			_normal = namedGraphics["Normal"];
			_happy = namedGraphics["Happy"];
			_minor = namedGraphics["Minor"];
			_major = namedGraphics["Major"];
			_extreme = namedGraphics["Extreme"];
			_crying = namedGraphics["Crying"];
			_dead = namedGraphics["Dead"];
			_curGraphic = _normal;
			tickDelegate.RareUpdate = Update;
		}

		public void Update(
			PawnState pawnState,
			BodyPartStatus bodyPartStatus,
			ref bool updatePortrait)
		{
			if(!pawnState.Alive)
			{
				_curGraphic = _dead;
				return;
			}
			if(pawnState.Fleeing || pawnState.InPainShock)
			{
				_curGraphic = _crying;
				return;
			}
			float moodLevel = _pawn.needs.mood.CurInstantLevel;
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdExtreme)
			{
				_curGraphic = _extreme;
				return;
			}
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdMajor)
			{
				_curGraphic = _major;
				return;
			}
			if(moodLevel <= _pawn.mindState.mentalBreaker.BreakThresholdMinor)
			{
				_curGraphic = _minor;
				return;
			}
			float happyThreshold =
				_pawn.mindState.mentalBreaker.BreakThresholdMinor +
				((1f - _pawn.mindState.mentalBreaker.BreakThresholdMinor) / 2f);
			if(moodLevel < happyThreshold)
			{
				_curGraphic = _normal;
				return;
			}
			_curGraphic = _happy;
		}

		public void Render(
			Vector3 rootPos,
			Quaternion rootQuat,
			Rot4 rootRot4,
			Vector3 renderNodeOffset,
			Mesh renderNodeMesh,
			bool portrait)
		{
			MatProps_Multi graphic = portrait ?
				_normal :
				_curGraphic;
			if(graphic == null)
			{
				return;
			}
			MaterialPropertyBlock matPropBlock = graphic.GetMaterialProperty(rootRot4);
			if(matPropBlock != null)
			{
				Vector3 offset = rootQuat * renderNodeOffset;
				if(!portrait)
				{
					UnityEngine.Graphics.DrawMesh(
						renderNodeMesh,
						Matrix4x4.TRS(rootPos + offset, rootQuat, Vector3.one),
						Shaders.FacePart,
						0,
						null,
						0,
						matPropBlock);
				} else
				{
					Shaders.FacePart.mainTexture = matPropBlock.GetTexture(Shaders.MainTexPropID);
					Shaders.FacePart.SetPass(0);
					UnityEngine.Graphics.DrawMeshNow(renderNodeMesh, rootPos + offset, rootQuat);
				}
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
