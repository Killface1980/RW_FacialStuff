// ReSharper disable StyleCop.SA1401

using System;
using FacialStuff.Animator;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.Harmony;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
	public class FaceGraphic
	{
		const string STR_south = "_south";

		public Graphic BrowGraphic;
		public Graphic DeadEyeGraphic;
		private Graphic_FaceMirror[,] _eyeGraphics = new Graphic_FaceMirror[2, 3];
		private Graphic_FaceMirror[] _eyeClosedGraphics = new Graphic_FaceMirror[2];
		public Graphic_Multi_AddedHeadParts EyeRightPatchGraphic;
		public Graphic_Multi_AddedHeadParts EyeLeftPatchGraphic;
		
		public Graphic_Multi_NaturalHeadParts JawGraphic;
		public Graphic MainBeardGraphic;
		public Graphic MoustacheGraphic;
		private HumanMouthGraphics Mouthgraphic;

		public Graphic RottingWrinkleGraphic;

		public Graphic WrinkleGraphic;

		private readonly CompFace _compFace;
		private readonly Pawn _pawn;

		private readonly FaceData pawnFace;
		public Graphic_FaceMirror EyeLeftMissingGraphic;
		public Graphic_FaceMirror EyeRightMissingGraphic;

		public Graphic_FaceMirror EarLeftMissingGraphic;
		public Graphic_FaceMirror EarRightMissingGraphic;

		public string texPathBrow;
		
		public string texPathEyeLeftPatch;
		public string texPathEyeRightPatch;
		
		public string texPathEarLeftPatch;
		public string texPathEarLeftMissing;
		public string texPathEarRightPatch;
		public string texPathEarRightMissing;

		public string texPathJawAddedPart;

		public FaceGraphic(CompFace compFace, int eyeCount)
		{
			_compFace = compFace;
			_pawn = compFace.Pawn;
			_eyeGraphics = new Graphic_FaceMirror[eyeCount, Enum.GetNames(typeof(BodyPartLevel)).Length];
			_eyeClosedGraphics = new Graphic_FaceMirror[eyeCount];
			pawnFace = compFace.FaceData;
			if(pawnFace != null)
			{
				if(compFace.Props.hasBeard)
				{
					InitializeGraphicsBeard(compFace);
				}
				if(compFace.Props.hasWrinkles)
				{
					InitializeGraphicsWrinkles(compFace);
				}
				if(compFace.Props.perEyeBehaviors.Count > 0)
				{
					MakeEyes(pawnFace);
				}
			}
			if(compFace.Props.hasMouth)
			{
				Mouthgraphic = new HumanMouthGraphics(pawnFace.MouthSetDef);
			}
		}
				
		public void MakeEyes(FaceData pawnFace)
		{
			texPathBrow = BrowTexPath(pawnFace.BrowDef);

			InitializeGraphicsEyes();
			InitializeGraphicsBrows();
		}
				
		private void InitializeGraphicsBeard(CompFace compFace)
		{
			if(pawnFace == null)
			{
				return;
			}

			string mainBeardDefTexPath = GetBeardPath();

			string moustacheDefTexPath = GetMoustachePath();

			Color beardColor = pawnFace.BeardColor;
			Color tacheColor = pawnFace.BeardColor;

			if(pawnFace.MoustacheDef == MoustacheDefOf.Shaved)
			{
				// no error, only use the beard def shaved as texture
				tacheColor = Color.white;
			}

			if(pawnFace.BeardDef == BeardDefOf.Beard_Shaved)
			{
				beardColor = Color.white;
			}

			if(Controller.settings.SameBeardColor)
			{
				beardColor = tacheColor = _pawn.story.hairColor;
			}

			MoustacheGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
				moustacheDefTexPath,
				ShaderDatabase.Cutout,
				Vector2.one,
				tacheColor);

			MainBeardGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
				mainBeardDefTexPath,
				ShaderDatabase.Cutout,
				Vector2.one,
				beardColor);
		}
		
		public string EarTexPath(Side side, EarDef ear)
		{
			string earPath = ear.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Ears/" : ear.texBasePath;
			string path = earPath + "Ear_" + ear.texName + "_" + _pawn.gender + "_" + side;
			return path.Replace(@"\", @"/");
		}

		public string BrowTexPath(BrowDef browDef)
		{
			string browPath = browDef.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Brows/" : browDef.texBasePath;
			string browTexPath = browPath + "Brow_" + _pawn.gender + "_" + browDef.texName;
			return browTexPath;
		}

		public string GetBeardPath(BeardDef def = null)
		{
			if(def == null)
			{
				if(_compFace.FaceData?.BeardDef != null)
				{
					def = _compFace.FaceData?.BeardDef;
				} else
				{
					return string.Empty;
				}
			}

			if(def == BeardDefOf.Beard_Shaved)
			{
				return StringsFS.PathHumanlike + "Beards/Beard_Shaved";
			}

			if(def.IsBeardNotHair())
			{
				return StringsFS.PathHumanlike + "Beards/" + def.texPath;
			}

			return StringsFS.PathHumanlike + "Beards/Beard_" + _compFace.PawnHeadType + "_" + def.texPath + "_" + _compFace.PawnCrownType;
		}

		public string GetMoustachePath(MoustacheDef def = null)
		{
			if(def == null)
			{
				if(_compFace.FaceData?.MoustacheDef != null)
				{
					def = _compFace.FaceData?.MoustacheDef;
				} else
				{
					return string.Empty;
				}
			}

			if(def == MoustacheDefOf.Shaved)
			{
				return this.GetBeardPath(BeardDefOf.Beard_Shaved);
			}

			return def.texPath + "_" + _compFace.PawnCrownType;
		}
		
		private void InitializeGraphicsBrows()
		{
			Color color = _pawn.story.hairColor;
			BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
				texPathBrow,
				ShaderDatabase.CutoutSkin,
				Vector2.one,
				color);
		}
				
		public Material EyeMatAt(int partIdx, Rot4 headFacing, bool portrait, bool eyeOpen, BodyPartLevel partLevel)
		{
			// TODO: get damaged mat
			Graphic eyeGraphic = null;
			if(eyeOpen || portrait)
			{
				eyeGraphic = _eyeGraphics[partIdx, (int)partLevel];
			}
			else
			{
				eyeGraphic = _eyeClosedGraphics[partIdx];
			}
			return eyeGraphic?.MatAt(headFacing);
		}
		
		public string EyeTexturePath(EyeDef eyeDef)
		{
			string texBasePath = pawnFace.EyeDef.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Eyes/" : pawnFace.EyeDef.texBasePath;
			string texFileName = eyeDef.texCollection + "_" + eyeDef.texName;
			return texBasePath + texFileName;
		}

		private void InitializeGraphicsEyes()
		{
			Color eyeColor = Color.white;
			var compProps = _compFace.Props;
			Array.Clear(_eyeGraphics, 0, _eyeGraphics.Length);

			// compProps.perEyeDefs.Count indicates how many eyes the race has.
			// Open eye graphics
			for(int i = 0; i < compProps.perEyeBehaviors.Count; ++i)
			{
				string texPath = EyeTexturePath(pawnFace.EyeDef);
				_eyeGraphics[i, (int)BodyPartLevel.Natural] = GraphicDatabase.Get<Graphic_FaceMirror>(
					texPath,
					ShaderDatabase.CutoutComplex,
					Vector2.one,
					eyeColor) as Graphic_FaceMirror;
			}
			// Closed eye graphics
			for(int i = 0; i < compProps.perEyeBehaviors.Count; ++i)
			{
				if(pawnFace.EyeDef.closedEyeDef != null)
				{
					string texPath = EyeTexturePath(pawnFace.EyeDef.closedEyeDef);
					_eyeClosedGraphics[i] = GraphicDatabase.Get<Graphic_FaceMirror>(
						texPath,
						ShaderDatabase.CutoutComplex,
						Vector2.one,
						eyeColor) as Graphic_FaceMirror;
				}
			}
			// Missing eye part graphics
			for(int i = 0; i < compProps.perEyeBehaviors.Count; ++i)
			{
				if(pawnFace.EyeDef.missingEyeDef != null)
				{
					string texPath = EyeTexturePath(pawnFace.EyeDef.missingEyeDef);
					_eyeGraphics[i, (int)BodyPartLevel.Removed] = GraphicDatabase.Get<Graphic_FaceMirror>(
						texPath,
						ShaderDatabase.CutoutComplex,
						Vector2.one,
						eyeColor) as Graphic_FaceMirror;
				}
			}
		}
		
		public Material MouthMatAt(Rot4 headFacing, int textureIdx)
		{
			return Mouthgraphic.HumanMouthGraphic[textureIdx].MatAt(headFacing);
		}
		
		private void InitializeGraphicsWrinkles(CompFace compFace)
		{
			Color wrinkleColor = _pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);

			{
				wrinkleColor.a = pawnFace.WrinkleIntensity;

				WrinkleDef pawnFaceWrinkleDef = pawnFace.WrinkleDef;

				WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
					pawnFaceWrinkleDef.texPath + "_" + compFace.PawnCrownType + "_" + compFace.PawnHeadType,
					ShaderDatabase.Transparent,
					Vector2.one,
					wrinkleColor);

				RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
					pawnFaceWrinkleDef.texPath + "_" + compFace.PawnCrownType + "_" + compFace.PawnHeadType,
					ShaderDatabase.Transparent,
					Vector2.one,
					wrinkleColor * FaceTextures.SkinRottingMultiplyColor);
			}
		}
	}
}
