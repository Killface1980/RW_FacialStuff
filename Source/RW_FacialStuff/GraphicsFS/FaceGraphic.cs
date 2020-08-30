using System;
using System.Collections.Generic;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.Harmony;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
	public class FaceGraphic
	{
		public Graphic BrowGraphic;
		private Graphic_FaceMirror[,] _eyeGraphics;
		private Graphic_FaceMirror[] _eyeClosedGraphics;
		private List<Graphic_Multi> _mouthGraphics;
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
			_eyeClosedGraphics = new Graphic_FaceMirror[eyeCount];
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
				_mouthGraphics = new List<Graphic_Multi>(_pawnFace.MouthSetDef.texNames.Count);
				for(int i = 0; i < _pawnFace.MouthSetDef.texNames.Count; ++i)
				{
					_mouthGraphics.Add(
						GraphicDatabase.Get<Graphic_Multi>(
							_pawnFace.MouthSetDef.texBasePath +
								_pawnFace.MouthSetDef.texCollection + "_" +
								_pawnFace.MouthSetDef.texSetName + "_" +
								_pawnFace.MouthSetDef.texNames[i],
							ShaderDatabase.CutoutSkin,
							Vector2.one,
							color) as Graphic_Multi);
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
			BrowGraphic = GraphicDatabase.Get<Graphic_Multi>(
				BrowTexPath(_pawnFace.BrowDef),
				ShaderDatabase.CutoutSkin,
				Vector2.one,
				color) as Graphic_Multi;
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
			string texBasePath = _pawnFace.EyeDef.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Eyes/" : _pawnFace.EyeDef.texBasePath;
			string texFileName = eyeDef.texCollection + "_" + eyeDef.texName;
			return texBasePath + texFileName;
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
					ShaderDatabase.CutoutComplex,
					Vector2.one,
					eyeColor) as Graphic_FaceMirror;
			}
			// Closed eye graphics
			for(int i = 0; i < _compFace.EyeBehavior.NumEyes; ++i)
			{
				if(_pawnFace.EyeDef.closedEyeDef != null)
				{
					string texPath = EyeTexturePath(_pawnFace.EyeDef.closedEyeDef);
					_eyeClosedGraphics[i] = GraphicDatabase.Get<Graphic_FaceMirror>(
						texPath,
						ShaderDatabase.CutoutComplex,
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
						ShaderDatabase.CutoutComplex,
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
