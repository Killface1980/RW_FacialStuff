using System;
using System.Collections.Generic;
using System.IO;
using PawnPlus.DefOfs;
using PawnPlus.Defs;
using PawnPlus.Harmony;
using UnityEngine;
using Verse;

namespace PawnPlus.Graphics
{
	public class FaceGraphic
	{
		public Graphic BrowGraphic;
		private Graphic_FaceMirror[,] _eyeGraphics;
		private Graphic_FaceMirror[,] _eyeActionGraphics;
		private List<Graphic_FaceMirror> _mouthGraphics;
		public Graphic_Multi JawGraphic;
		public Graphic_Multi MainBeardGraphic;
		public Graphic_Multi MoustacheGraphic;
		public Graphic_Multi RottingWrinkleGraphic;
		public Graphic_Multi WrinkleGraphic;
				
		private readonly CompFace _compFace;
		private readonly Pawn _pawn;
		private readonly FaceData _pawnFace;

		public FaceGraphic(CompFace compFace, int eyeCount)
		{
			_compFace = compFace;
			_pawn = compFace.Pawn;
			_eyeGraphics = new Graphic_FaceMirror[eyeCount, Enum.GetNames(typeof(BodyPartLevel)).Length];
			_eyeActionGraphics = new Graphic_FaceMirror[eyeCount, Enum.GetNames(typeof(EyeAction)).Length];
			_pawnFace = compFace.FaceData;
			if(_pawnFace != null)
			{
				if(compFace.Props.hasBeard)
				{
					InitializeGraphicsBeard(compFace);
				}
				if(compFace.Props.hasWrinkles)
				{
					InitializeGraphicsWrinkles(compFace);
				}
				if(compFace.EyeBehavior.NumEyes > 0)
				{
					InitializeGraphicsEyes();
					InitializeGraphicsBrows();
				}
			}
			if(compFace.Props.hasMouth)
			{
				Color color = Color.white;
				_mouthGraphics = new List<Graphic_FaceMirror>(_pawnFace.MouthSetDef.texNames.Count);
				for(int i = 0; i < _pawnFace.MouthSetDef.texNames.Count; ++i)
				{
					_mouthGraphics.Add(
						GraphicDatabase.Get<Graphic_FaceMirror>(
							Path.Combine(
								_pawnFace.MouthSetDef.texBasePath,
								_pawnFace.MouthSetDef.texNames[i]),
							Shaders.FacePart,
							Vector2.one,
							color) as Graphic_FaceMirror);
				}
			}
		}
						
		private void InitializeGraphicsBeard(CompFace compFace)
		{
			if(_pawnFace == null)
			{
				return;
			}

			string mainBeardDefTexPath = GetBeardPath();
			string moustacheDefTexPath = GetMoustachePath();
			Color beardColor = _pawnFace.BeardColor;
			Color tacheColor = _pawnFace.BeardColor;

			if(_pawnFace.MoustacheDef == MoustacheDefOf.Shaved)
			{
				// no error, only use the beard def shaved as texture
				tacheColor = Color.white;
			}
			if(_pawnFace.BeardDef == BeardDefOf.Beard_Shaved)
			{
				beardColor = Color.white;
			}
			if(Controller.settings.SameBeardColor)
			{
				beardColor = tacheColor = compFace.Pawn.story.hairColor;
			}

			MoustacheGraphic = GraphicDatabase.Get<Graphic_Multi>(
				moustacheDefTexPath,
				ShaderDatabase.Cutout,
				Vector2.one,
				tacheColor) as Graphic_Multi;

			MainBeardGraphic = GraphicDatabase.Get<Graphic_Multi>(
				mainBeardDefTexPath,
				ShaderDatabase.Cutout,
				Vector2.one,
				beardColor) as Graphic_Multi;
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
			BrowGraphic = GraphicDatabase.Get<Graphic_Multi>(
				BrowTexPath(_pawnFace.BrowDef),
				ShaderDatabase.CutoutSkin,
				Vector2.one,
				color) as Graphic_Multi;
		}
				
		public Material EyeMatAt(int partIdx, Rot4 headFacing, bool portrait, EyeAction eyeAction, BodyPartLevel partLevel)
		{
			// TODO: get damaged mat
			Graphic eyeGraphic = null;
			if(eyeAction == EyeAction.None || portrait)
			{
				eyeGraphic = _eyeGraphics[partIdx, (int)partLevel];
			}
			else
			{
				eyeGraphic = _eyeActionGraphics[partIdx, (int)eyeAction - 1];
			}
			return eyeGraphic?.MatAt(headFacing);
		}
		
		public string EyeTexturePath(EyeDef eyeDef)
		{
			return Path.Combine(_pawnFace.EyeDef.texBasePath, eyeDef.texName);
		}

		private void InitializeGraphicsEyes()
		{
			Color eyeColor = Color.white;
			Array.Clear(_eyeGraphics, 0, _eyeGraphics.Length);

			// compProps.eyeBehavior.NumEyes indicates how many eyes the race has.
			// Open eye graphics
			for(int i = 0; i < _compFace.EyeBehavior.NumEyes; ++i)
			{
				string texPath = EyeTexturePath(_pawnFace.EyeDef);
				_eyeGraphics[i, (int)BodyPartLevel.Natural] = GraphicDatabase.Get<Graphic_FaceMirror>(
					texPath,
					Shaders.FacePart,
					Vector2.one,
					eyeColor) as Graphic_FaceMirror;
			}
			// Closed eye graphics
			for(int i = 0; i < _compFace.EyeBehavior.NumEyes; ++i)
			{
				if(_pawnFace.EyeDef.closedEyeDef != null)
				{
					string texPath = EyeTexturePath(_pawnFace.EyeDef.closedEyeDef);
					_eyeActionGraphics[i, (int)EyeAction.Closed - 1] = GraphicDatabase.Get<Graphic_FaceMirror>(
						texPath,
						Shaders.FacePart,
						Vector2.one,
						eyeColor) as Graphic_FaceMirror;
				}
			}
			// In pain eye graphics
			for(int i = 0; i < _compFace.EyeBehavior.NumEyes; ++i)
			{
				if(_pawnFace.EyeDef.closedEyeDef != null)
				{
					string texPath = EyeTexturePath(_pawnFace.EyeDef.inPainEyeDef);
					_eyeActionGraphics[i, (int)EyeAction.Pain - 1] = GraphicDatabase.Get<Graphic_FaceMirror>(
						texPath,
						Shaders.FacePart,
						Vector2.one,
						eyeColor) as Graphic_FaceMirror;
				}
			}
			// Missing eye part graphics
			for(int i = 0; i < _compFace.EyeBehavior.NumEyes; ++i)
			{
				if(_pawnFace.EyeDef.missingEyeDef != null)
				{
					string texPath = EyeTexturePath(_pawnFace.EyeDef.missingEyeDef);
					_eyeGraphics[i, (int)BodyPartLevel.Removed] = GraphicDatabase.Get<Graphic_FaceMirror>(
						texPath,
						Shaders.FacePart,
						Vector2.one,
						eyeColor) as Graphic_FaceMirror;
				}
			}
		}
		
		public Material MouthMatAt(Rot4 headFacing, int textureIdx)
		{
			return _mouthGraphics[textureIdx].MatAt(headFacing);
		}
		
		private void InitializeGraphicsWrinkles(CompFace compFace)
		{
			Color wrinkleColor = _pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);
			wrinkleColor.a = _pawnFace.WrinkleIntensity;

			WrinkleDef pawnFaceWrinkleDef = _pawnFace.WrinkleDef;

			WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi>(
				pawnFaceWrinkleDef.texPath + "_" + compFace.PawnCrownType + "_" + compFace.PawnHeadType,
				ShaderDatabase.Transparent,
				Vector2.one,
				wrinkleColor) as Graphic_Multi;

			RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi>(
				pawnFaceWrinkleDef.texPath + "_" + compFace.PawnCrownType + "_" + compFace.PawnHeadType,
				ShaderDatabase.Transparent,
				Vector2.one,
				wrinkleColor * FaceTextures.SkinRottingMultiplyColor) as Graphic_Multi;
		}
	}
}
