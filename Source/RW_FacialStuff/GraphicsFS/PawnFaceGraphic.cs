// ReSharper disable StyleCop.SA1401

using System;
using FacialStuff.Animator;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
	public class PawnFaceGraphic
	{
		const string STR_south = "_south";
		const string STR_ROMV_Fangs = "ROMV_Fangs";

		public Graphic BrowGraphic;

		public Graphic DeadEyeGraphic;

		public Graphic_Multi_NaturalEyes EyeLeftClosedGraphic;

		public Graphic_Multi_NaturalEyes EyeLeftGraphic;
		public Graphic_Multi_NaturalEars EarLeftGraphic;

		public Graphic_Multi_AddedHeadParts EyeLeftPatchGraphic;

		public Graphic_Multi_NaturalEyes EyeRightClosedGraphic;

		public Graphic_Multi_NaturalEars EarLeftPatchGraphic;


		public Graphic_Multi_NaturalEyes EyeRightGraphic;
		public Graphic_Multi_NaturalEars EarRightGraphic;

		public Graphic_Multi_AddedHeadParts EyeRightPatchGraphic;
		public Graphic_Multi_AddedHeadParts EarRightPatchGraphic;

		public Graphic_Multi_NaturalHeadParts JawGraphic;

		public Graphic MainBeardGraphic;

		public Graphic MoustacheGraphic;

		public HumanMouthGraphics Mouthgraphic;

		public Graphic_Multi_NaturalHeadParts MouthGraphic;

		public Graphic RottingWrinkleGraphic;

		public string TexPathBrow;

		[NotNull] public string TexPathEyeLeftClosed;

		public string TexPathEyeRightClosed;

		public Graphic WrinkleGraphic;

		[NotNull] private readonly CompFace _compFace;

		[NotNull] private readonly Pawn _pawn;

		private float _mood = 0.5f;
		private readonly PawnFace pawnFace;
		public Graphic_Multi_NaturalEyes EyeLeftMissingGraphic;
		public Graphic_Multi_NaturalEyes EyeRightMissingGraphic;

		public Graphic_Multi_NaturalEars EarLeftMissingGraphic;
		public Graphic_Multi_NaturalEars EarRightMissingGraphic;

		public PawnFaceGraphic(CompFace compFace)
		{
			_compFace = compFace;
			_pawn = compFace.Pawn;

			pawnFace = _compFace.PawnFace;
			if(pawnFace != null)
			{
				if(_compFace.Props.hasBeard)
				{
					InitializeGraphicsBeard();
				}

				if(_compFace.Props.hasWrinkles)
				{
					InitializeGraphicsWrinkles();
				}

				MakeEyes();
				MakeEars();
			}

			if(_compFace.Props.hasMouth)
			{
				Mouthgraphic = new HumanMouthGraphics(_pawn);
				InitializeGraphicsMouth();
			}
		}

		public void MakeEyes()
		{
			if(!_compFace.Props.hasEyes)
			{
				return;
			}

			_compFace.TexPathEyeRight = _compFace.EyeTexPath(Side.Right);
			_compFace.TexPathEyeLeft = _compFace.EyeTexPath(Side.Left);
			_compFace.TexPathEyeRightMissing = _compFace.EyeTexPath(Side.Right, EyeDefOf.Missing);
			_compFace.TexPathEyeLeftMissing = _compFace.EyeTexPath(Side.Left, EyeDefOf.Missing);
			TexPathEyeLeftClosed = _compFace.EyeTexPath(Side.Left, EyeDefOf.Closed);
			TexPathEyeRightClosed = _compFace.EyeTexPath(Side.Right, EyeDefOf.Closed);
			TexPathBrow = _compFace.BrowTexPath(pawnFace.BrowDef);


			InitializeGraphicsEyes();
			InitializeGraphicsBrows();
		}
		public void MakeEars()
		{
			if(!_compFace.Props.hasEars)
			{
				return;
			}

			_compFace.TexPathEarRight = _compFace.EarTexPath(Side.Right);
			_compFace.TexPathEarLeft = _compFace.EarTexPath(Side.Left);
			_compFace.TexPathEarRightMissing = _compFace.EarTexPath(Side.Right, EarDefOf.Missing);
			_compFace.TexPathEarLeftMissing = _compFace.EarTexPath(Side.Left, EarDefOf.Missing);


			this.InitializeGraphicsEars();
		}

		public void SetMouthAccordingToMoodLevel()
		{
			if(_pawn == null)
			{
				return;
			}

			if(!Controller.settings.UseMouth || _compFace.BodyStat.Jaw != PartStatus.Natural)
			{
				return;
			}

			if(_pawn.Fleeing() || _pawn.IsBurning())
			{
				MouthGraphic = Mouthgraphic.MouthGraphicCrying;
				return;
			}

			if(_pawn.health.InPainShock && !_compFace.IsAsleep)
			{
				PawnEyeWiggler eyeWiggler = _compFace.EyeWiggler;
				if(eyeWiggler == null || eyeWiggler.EyeRightBlinkNow && eyeWiggler.EyeLeftBlinkNow)
				{
					MouthGraphic = Mouthgraphic.MouthGraphicCrying;
					return;
				}
			}

			if(_pawn.needs?.mood?.thoughts != null)
			{
				_mood = _pawn.needs.mood.CurInstantLevel;
			}

			int indexOfMood = Mouthgraphic.GetMouthTextureIndexOfMood(this._mood);

			MouthGraphic = Mouthgraphic.HumanMouthGraphic[indexOfMood].Graphic;
		}

		private void InitializeGraphicsBeard()
		{
			if(pawnFace == null)
			{
				return;
			}

			string mainBeardDefTexPath = _compFace.GetBeardPath();

			string moustacheDefTexPath = _compFace.GetMoustachePath();

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

		private void InitializeGraphicsBrows()
		{
			Color color = _pawn.story.hairColor;
			BrowGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
				TexPathBrow,
				ShaderDatabase.CutoutSkin,
				Vector2.one,
				color);
		}

		private void InitializeGraphicsEyePatches()
		{
			if(!this._compFace.TexPathEyeLeftPatch.NullOrEmpty())
			{
				bool leftTexExists = EyePatchLeftTexExists();
				if(leftTexExists)
				{
					EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
						_compFace.TexPathEyeLeftPatch,
						ShaderDatabase.Transparent,
						Vector2.one,
						Color.white) as Graphic_Multi_AddedHeadParts;
					_compFace.BodyStat.EyeLeft = PartStatus.Artificial;
				} else
				{
					Log.Message(
						"Facial Stuff: No texture for added part: " +
						_compFace.TexPathEyeLeftPatch +
						" - Graphic_Multi_AddedHeadParts");
				}
			}

			if(!this._compFace.TexPathEyeRightPatch.NullOrEmpty())
			{
				bool rightTexExists = EyePatchRightTexExists();
				if(rightTexExists)
				{
					EyeRightPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
						_compFace.TexPathEyeRightPatch,
						ShaderDatabase.Transparent,
						Vector2.one,
						Color.white) as Graphic_Multi_AddedHeadParts;
					_compFace.BodyStat.EyeRight = PartStatus.Artificial;
				} else
				{
					Log.Message(
						"Facial Stuff: No texture for added part: " +
						_compFace.TexPathEyeRightPatch +
						" - Graphic_Multi_AddedHeadParts");
				}
			}
		}

		public bool EyePatchRightTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				_compFace.TexPathEyeRightPatch + STR_south, false).NullOrBad();
		}

		public bool EyePatchLeftTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				_compFace.TexPathEyeLeftPatch + STR_south, false).NullOrBad();
		}
		public bool EarPatchRightTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				_compFace.TexPathEarRightPatch + STR_south, false).NullOrBad();
		}

		public bool EarPatchLeftTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				_compFace.TexPathEarLeftPatch + STR_south, false).NullOrBad();
		}

		private void InitializeGraphicsEyes()
		{
			InitializeGraphicsEyePatches();

			Color eyeColor = Color.white;

			EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				_compFace.TexPathEyeLeft,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				_compFace.TexPathEyeRight,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeLeftMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				_compFace.TexPathEyeLeftMissing,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeRightMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				_compFace.TexPathEyeRightMissing,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				TexPathEyeLeftClosed,
				ShaderDatabase.Cutout,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				TexPathEyeRightClosed,
				ShaderDatabase.Cutout,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			DeadEyeGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
				StringsFS.PathHumanlike + "Eyes/Eyes_Dead",
				ShaderDatabase.Cutout,
				Vector2.one,
				Color.black);
		}
		private void InitializeGraphicsEars()
		{
			// this.InitializeGraphicsEyePatches();

			Color earColor = _pawn.story.SkinColor;

			EarLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(
				_compFace.TexPathEarLeft,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				earColor) as Graphic_Multi_NaturalEars;

			EarRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(
				_compFace.TexPathEarRight,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				earColor) as Graphic_Multi_NaturalEars;
		}

		private void InitializeGraphicsMouth()
		{
			if(!_compFace.TexPathJawAddedPart.NullOrEmpty())
			{
				bool flag = ContentFinder<Texture2D>.Get(_compFace.TexPathJawAddedPart + STR_south, false) != null;
				if(flag)
				{
					JawGraphic =
					GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
						_compFace.TexPathJawAddedPart,
						ShaderDatabase.CutoutSkin,
						Vector2.one,
						Color.white) as Graphic_Multi_NaturalHeadParts;
					_compFace.BodyStat.Jaw = PartStatus.Artificial;
					string addedPart = _compFace.TexPathJawAddedPart;
					if(addedPart != null && addedPart.Contains(STR_ROMV_Fangs))
					{
						_compFace.BodyStat.Jaw = PartStatus.DisplayOverBeard;
					}

					// all done, return
					return;
				}

				// texture for added/extra part not found, log and default
				Log.Message(
					"Facial Stuff: No texture for added part: " +
					_compFace.TexPathJawAddedPart +
					" - Graphic_Multi_NaturalHeadParts. This is not an error, just an info.");

			}

			MouthGraphic = Mouthgraphic.HumanMouthGraphic[_pawn.Dead || _pawn.Downed ? 2 : 3].Graphic;
		}

		private void InitializeGraphicsWrinkles()
		{
			Color wrinkleColor = _pawn.story.SkinColor * new Color(0.1f, 0.1f, 0.1f);

			{
				wrinkleColor.a = pawnFace.WrinkleIntensity;

				WrinkleDef pawnFaceWrinkleDef = pawnFace.WrinkleDef;

				WrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
					pawnFaceWrinkleDef.texPath + "_" + _compFace.PawnCrownType + "_" + _compFace.PawnHeadType,
					ShaderDatabase.Transparent,
					Vector2.one,
					wrinkleColor);

				RottingWrinkleGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
					pawnFaceWrinkleDef.texPath + "_" + _compFace.PawnCrownType + "_" + _compFace.PawnHeadType,
					ShaderDatabase.Transparent,
					Vector2.one,
					wrinkleColor * FaceTextures.SkinRottingMultiplyColor);
			}
		}
	}
}
