﻿// ReSharper disable StyleCop.SA1401

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
	public class PawnFaceGraphic
	{
		const string STR_south = "_south";
		const string STR_ROMV_Fangs = "ROMV_Fangs";

		public Graphic BrowGraphic;
		public Graphic DeadEyeGraphic;
		public Graphic_Multi_NaturalEyes EyeRightClosedGraphic;
		public Graphic_Multi_NaturalEyes EyeLeftClosedGraphic;
		public Graphic_Multi_NaturalEyes EyeRightGraphic;
		public Graphic_Multi_NaturalEyes EyeLeftGraphic;
		public Graphic_Multi_AddedHeadParts EyeRightPatchGraphic;
		public Graphic_Multi_AddedHeadParts EyeLeftPatchGraphic;
		
		public Graphic_Multi_NaturalEars EarRightGraphic;
		public Graphic_Multi_NaturalEars EarLeftGraphic;
		public Graphic_Multi_AddedHeadParts EarRightPatchGraphic;
		public Graphic_Multi_NaturalEars EarLeftPatchGraphic;

		public Graphic_Multi_NaturalHeadParts JawGraphic;
		public Graphic MainBeardGraphic;
		public Graphic MoustacheGraphic;
		public HumanMouthGraphics Mouthgraphic;
		public Graphic_Multi_NaturalHeadParts MouthGraphic;

		public Graphic RottingWrinkleGraphic;

		public Graphic WrinkleGraphic;

		private readonly CompFace _compFace;
		private readonly Pawn _pawn;

		private float _mood = 0.5f;
		private readonly PawnFace pawnFace;
		public Graphic_Multi_NaturalEyes EyeLeftMissingGraphic;
		public Graphic_Multi_NaturalEyes EyeRightMissingGraphic;

		public Graphic_Multi_NaturalEars EarLeftMissingGraphic;
		public Graphic_Multi_NaturalEars EarRightMissingGraphic;

		public string texPathBrow;
		
		public string texPathEyeLeft;
		public string texPathEyeLeftPatch;
		public string texPathEyeLeftMissing;
		public string texPathEyeLeftClosed;
		public string texPathEyeRight;
		public string texPathEyeRightPatch;
		public string texPathEyeRightMissing;
		public string texPathEyeRightClosed;
		
		public string texPathEarLeft;
		public string texPathEarLeftPatch;
		public string texPathEarLeftMissing;
		public string texPathEarRight;
		public string texPathEarRightPatch;
		public string texPathEarRightMissing;

		public string texPathJawAddedPart;

		public PawnFaceGraphic(CompFace compFace)
		{
			_compFace = compFace;
			_pawn = compFace.Pawn;

			pawnFace = compFace.PawnFace;
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
				if(compFace.Props.hasEyes)
				{
					MakeEyes(pawnFace);
				}
				if(!compFace.Props.hasEars)
				{
					MakeEars(pawnFace);
				}
			}

			if(compFace.Props.hasMouth)
			{
				Mouthgraphic = new HumanMouthGraphics(_pawn);
				InitializeGraphicsMouth(compFace);
			}
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
		
		public void MakeEyes(PawnFace pawnFace)
		{
			texPathEyeRight = EyeTexPath(Side.Right, pawnFace.EyeDef);
			texPathEyeLeft = EyeTexPath(Side.Left, pawnFace.EyeDef);
			texPathEyeRightMissing = EyeTexPath(Side.Right, EyeDefOf.Missing);
			texPathEyeLeftMissing = EyeTexPath(Side.Left, EyeDefOf.Missing);
			texPathEyeLeftClosed = EyeTexPath(Side.Left, EyeDefOf.Closed);
			texPathEyeRightClosed = EyeTexPath(Side.Right, EyeDefOf.Closed);
			texPathBrow = BrowTexPath(pawnFace.BrowDef);

			InitializeGraphicsEyes();
			InitializeGraphicsBrows();
		}

		public void MakeEars(PawnFace pawnFace)
		{
			texPathEarRight = EarTexPath(Side.Right, pawnFace.EarDef);
			texPathEarLeft = EarTexPath(Side.Left, pawnFace.EarDef);
			texPathEarRightMissing = EarTexPath(Side.Right, EarDefOf.Missing);
			texPathEarLeftMissing = EarTexPath(Side.Left, EarDefOf.Missing);

			this.InitializeGraphicsEars();
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

		public string EyeTexPath(Side side, EyeDef eyeDef)
		{
			string eyePath = eyeDef.texBasePath.NullOrEmpty() ? StringsFS.PathHumanlike + "Eyes/" : eyeDef.texBasePath;
			string path = eyePath + "Eye_" + eyeDef.texName + "_" + _pawn.gender + "_" + side;
			return path.Replace(@"\", @"/");
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
				if(_compFace.PawnFace?.BeardDef != null)
				{
					def = _compFace.PawnFace?.BeardDef;
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
				if(_compFace.PawnFace?.MoustacheDef != null)
				{
					def = _compFace.PawnFace?.MoustacheDef;
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

		private void InitializeGraphicsEyePatches()
		{
			if(!texPathEyeLeftPatch.NullOrEmpty())
			{
				bool leftTexExists = EyePatchLeftTexExists();
				if(leftTexExists)
				{
					EyeLeftPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
						texPathEyeLeftPatch,
						ShaderDatabase.Transparent,
						Vector2.one,
						Color.white) as Graphic_Multi_AddedHeadParts;
					_compFace.BodyStat.EyeLeft = PartStatus.Artificial;
				} else
				{
					Log.Message(
						"Facial Stuff: No texture for added part: " +
						texPathEyeLeftPatch +
						" - Graphic_Multi_AddedHeadParts");
				}
			}

			if(!texPathEyeRightPatch.NullOrEmpty())
			{
				bool rightTexExists = EyePatchRightTexExists();
				if(rightTexExists)
				{
					EyeRightPatchGraphic = GraphicDatabase.Get<Graphic_Multi_AddedHeadParts>(
						texPathEyeRightPatch,
						ShaderDatabase.Transparent,
						Vector2.one,
						Color.white) as Graphic_Multi_AddedHeadParts;
					_compFace.BodyStat.EyeRight = PartStatus.Artificial;
				} else
				{
					Log.Message(
						"Facial Stuff: No texture for added part: " +
						texPathEyeRightPatch +
						" - Graphic_Multi_AddedHeadParts");
				}
			}
		}

		public bool EyePatchRightTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				texPathEyeRightPatch + STR_south, false).NullOrBad();
		}

		public bool EyePatchLeftTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				texPathEyeLeftPatch + STR_south, false).NullOrBad();
		}
		
		public bool EarPatchRightTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				texPathEarRightPatch + STR_south, false).NullOrBad();
		}

		public bool EarPatchLeftTexExists()
		{
			return !ContentFinder<Texture2D>.Get(
				texPathEarLeftPatch + STR_south, false).NullOrBad();
		}

		private void InitializeGraphicsEyes()
		{
			InitializeGraphicsEyePatches();

			Color eyeColor = Color.white;

			EyeLeftGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				texPathEyeLeft,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				texPathEyeRight,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeLeftMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				texPathEyeLeftMissing,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeRightMissingGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				texPathEyeRightMissing,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeLeftClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				texPathEyeLeftClosed,
				ShaderDatabase.Cutout,
				Vector2.one,
				eyeColor) as Graphic_Multi_NaturalEyes;

			EyeRightClosedGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
				texPathEyeRightClosed,
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
				texPathEarLeft,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				earColor) as Graphic_Multi_NaturalEars;

			EarRightGraphic = GraphicDatabase.Get<Graphic_Multi_NaturalEars>(
				texPathEarRight,
				ShaderDatabase.CutoutComplex,
				Vector2.one,
				earColor) as Graphic_Multi_NaturalEars;
		}

		private void InitializeGraphicsMouth(CompFace compFace)
		{
			if(!texPathJawAddedPart.NullOrEmpty())
			{
				bool flag = ContentFinder<Texture2D>.Get(texPathJawAddedPart + STR_south, false) != null;
				if(flag)
				{
					JawGraphic =
					GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
						texPathJawAddedPart,
						ShaderDatabase.CutoutSkin,
						Vector2.one,
						Color.white) as Graphic_Multi_NaturalHeadParts;
					compFace.BodyStat.Jaw = PartStatus.Artificial;
					string addedPart = texPathJawAddedPart;
					if(addedPart != null && addedPart.Contains(STR_ROMV_Fangs))
					{
						compFace.BodyStat.Jaw = PartStatus.DisplayOverBeard;
					}

					// all done, return
					return;
				}

				// texture for added/extra part not found, log and default
				Log.Message(
					"Facial Stuff: No texture for added part: " +
					texPathJawAddedPart +
					" - Graphic_Multi_NaturalHeadParts. This is not an error, just an info.");

			}

			MouthGraphic = Mouthgraphic.HumanMouthGraphic[_pawn.Dead || _pawn.Downed ? 2 : 3].Graphic;
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
