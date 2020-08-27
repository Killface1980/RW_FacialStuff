using FacialStuff.DefOfs;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class FaceMaterial
    {
        #region Public Fields

        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull]
        public DamageFlasher Flasher;

        #endregion Public Fields

        #region Private Fields

        [NotNull]
        private readonly CompFace _compFace;

        [NotNull]
        private readonly FaceGraphic _pawnFaceGraphic;

        private readonly Pawn _pawn;

        #endregion Private Fields

        #region Public Constructors

        public FaceMaterial([NotNull] CompFace compFace, [NotNull] FaceGraphic pawnFaceGraphic)
        {
            this._compFace = compFace;
            this._pawnFaceGraphic = pawnFaceGraphic;
            this._pawn = this._compFace.Pawn;
            this.Flasher = this._pawn.Drawer.renderer.graphics.flasher;

        }

        #endregion Public Constructors

        #region Public Methods

        [CanBeNull]
        public Material BeardMatAt(Rot4 facing)
        {
            if (this.CannotShowFaceHair() || this._compFace.FaceData?.BeardDef == BeardDefOf.Beard_Shaved)
            {
                
                return null;
            }

            Material material = this._pawnFaceGraphic.MainBeardGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }

        private bool CannotShowFaceHair()
        {
            if (this._pawn.gender           != Gender.Male 
             || this._compFace.BodyStat.Jaw == PartStatus.Missing || this._compFace.BodyStat.Jaw == PartStatus.Artificial)
            {
                return true;
            }

            return false;
        }

        [CanBeNull]
        public Material BrowMatAt(Rot4 facing)
        {
            Material material = this._pawnFaceGraphic.BrowGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }

        // Deactivated for now
        // ReSharper disable once FlagArgument
        [CanBeNull]
        public Material DeadEyeMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                material = this._pawnFaceGraphic.DeadEyeGraphic?.MatAt(facing);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this._pawnFaceGraphic.DeadEyeGraphic?.MatAt(facing);
            }

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }
        
        [CanBeNull]
        public Material EyeLeftPatchMatAt(Rot4 facing)
        {
            Material material = this._pawnFaceGraphic.EyeLeftPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }

        /*[CanBeNull]
        public Material EyeMatAt(Rot4 facing, int partIdx, PartStatus eyeStatus, EyeState eyeState, bool portrait)
        {
            if(portrait && eyeState == EyeState.Closed)
			{
                eyeState = EyeState.Open;
			}
            Material material = _pawnFaceGraphic.GetEyeGraphic(partIdx, eyeState).MatAt(facing);            
            if(material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }
            return material;
        }*/

        [CanBeNull]
        public Material EyeMissingMatAt(Rot4 facing, bool portrait)
        {
            if (facing == Rot4.West)
            {
                return null;
            }

            Material material = this._pawnFaceGraphic.EyeRightMissingGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }     

        [CanBeNull]
        public Material EyeRightPatchMatAt(Rot4 facing)
        {
            Material material = this._pawnFaceGraphic.EyeRightPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }
        
        [CanBeNull]
        public Material MoustacheMatAt(Rot4 facing)
        {
            if (this.CannotShowFaceHair() || this._compFace.FaceData?.MoustacheDef == MoustacheDefOf.Shaved)
            {
                return null;
            }

            Material material = this._pawnFaceGraphic.MoustacheGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material MouthMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;

            if (this._compFace.BodyStat.Jaw != PartStatus.Natural && Controller.settings.ShowExtraParts)
            {
                material = this._pawnFaceGraphic.JawGraphic?.MatAt(facing);
            }
            else
            {
                if (!Controller.settings.UseMouth)
                {
                    return null;
                }

                if (this._compFace.FaceData == null || !this._compFace.FaceData.DrawMouth)
                {
                    return null;
                }

                if (!this._compFace.Props.hasMouth)
                {
                    return null;
                }

                if (this._pawn.gender == Gender.Male)
                {
                    if (!this._compFace.FaceData.BeardDef.drawMouth || this._compFace.FaceData.MoustacheDef != MoustacheDefOf.Shaved)
                    {
                        return null;
                    }
                }

                if (portrait)
                {
                    material = this._pawnFaceGraphic.Mouthgraphic.HumanMouthGraphic[3].Graphic.MatAt(facing);
                }
                else
                {
                    material = this._pawnFaceGraphic.MouthGraphic?.MatAt(facing);

                    // if (bodyCondition == RotDrawMode.Fresh)
                    // {
                    // material = this.PawnGraphic.JawGraphic?.MatAt(facing);
                    // }
                    // else if (bodyCondition == RotDrawMode.Rotting)
                    // {
                    // }
                }
            }

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material WrinkleMatAt(Rot4 facing, RotDrawMode bodyCondition)
        {
            Material material = null;
            if (Controller.settings.UseWrinkles)
            {
                if (bodyCondition == RotDrawMode.Fresh)
                {
                    material = this._pawnFaceGraphic.WrinkleGraphic?.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this._pawnFaceGraphic.RottingWrinkleGraphic?.MatAt(facing);
                }
            }

            if (material != null)
            {
                material = this.Flasher.GetDamagedMat(material);
            }

            return material;
        }

        #endregion Public Methods
    }
}