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
