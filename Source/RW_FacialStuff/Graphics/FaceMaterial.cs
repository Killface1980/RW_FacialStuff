namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Graphics;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public class FaceMaterial
    {
        #region Public Fields

        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull]
        public DamageFlasher flasher;

        #endregion Public Fields

        #region Private Fields

        [NotNull]
        private readonly CompFace compFace;

        [NotNull]
        private readonly PawnFaceGraphic pawnFaceGraphic;

        private Pawn pawn;

        #endregion Private Fields

        #region Public Constructors

        public FaceMaterial([NotNull] CompFace compFace, [NotNull] PawnFaceGraphic pawnFaceGraphic)
        {
            this.compFace = compFace;
            this.pawnFaceGraphic = pawnFaceGraphic;
            this.pawn = this.compFace.Pawn;
            this.flasher = this.pawn.Drawer.renderer.graphics.flasher;

        }

        #endregion Public Constructors

        #region Public Methods

        [CanBeNull]
        public Material BeardMatAt(Rot4 facing)
        {
            if (this.pawn.gender != Gender.Male || this.compFace.PawnFace?.BeardDef == BeardDefOf.Beard_Shaved
                || this.compFace.bodyStat.jaw != PartStatus.Natural)
            {
                return null;
            }

            Material material = this.pawnFaceGraphic.MainBeardGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material BrowMatAt(Rot4 facing)
        {
            Material material = this.pawnFaceGraphic.BrowGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
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
                material = this.pawnFaceGraphic.DeadEyeGraphic?.MatAt(facing);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.pawnFaceGraphic.DeadEyeGraphic?.MatAt(facing);
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeLeftMatAt(Rot4 facing, bool portrait)
        {
            if (facing == Rot4.East)
            {
                return null;
            }

            Material material = this.pawnFaceGraphic.EyeLeftGraphic?.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.compFace.bodyStat.eyeLeft == PartStatus.Natural)
                {
                    if (this.compFace.IsAsleep || this.compFace.EyeWiggler.EyeLeftBlinkNow)
                    {
                        material = this.pawnFaceGraphic.EyeLeftClosedGraphic.MatAt(facing);
                    }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeLeftPatchMatAt(Rot4 facing)
        {
            Material material = this.pawnFaceGraphic.EyeLeftPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeRightMatAt(Rot4 facing, bool portrait)
        {
            if (facing == Rot4.West)
            {
                return null;
            }

            Material material = this.pawnFaceGraphic.EyeRightGraphic?.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.compFace.bodyStat.eyeRight == PartStatus.Natural)
                {
                    if (this.compFace.IsAsleep || this.compFace.EyeWiggler.EyeRightBlinkNow)
                    {
                        material = this.pawnFaceGraphic.EyeRightClosedGraphic?.MatAt(facing);
                    }
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeRightPatchMatAt(Rot4 facing)
        {
            Material material = this.pawnFaceGraphic.EyeRightPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material MoustacheMatAt(Rot4 facing)
        {
            if (this.pawn.gender != Gender.Male || this.compFace.PawnFace.MoustacheDef == MoustacheDefOf.Shaved
                || this.compFace.bodyStat.jaw != PartStatus.Natural)
            {
                return null;
            }

            Material material = this.pawnFaceGraphic.MoustacheGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material MouthMatAt(Rot4 facing, bool portrait)
        {
            Material material = null;

            if (this.compFace.bodyStat.jaw != PartStatus.Natural && Controller.settings.ShowExtraParts)
            {
                material = this.pawnFaceGraphic.JawGraphic?.MatAt(facing);
            }
            else
            {
                if (!Controller.settings.UseMouth || !this.compFace.PawnFace.DrawMouth)
                {
                    return null;
                }
                if (!this.compFace.Props.hasMouth)
                {
                    return null;
                }

                if (this.pawn.gender == Gender.Male)
                {
                    if (!this.compFace.PawnFace.BeardDef.drawMouth || this.compFace.PawnFace.MoustacheDef != MoustacheDefOf.Shaved)
                    {
                        return null;
                    }
                }

                if (portrait)
                {
                    material = this.pawnFaceGraphic.mouthgraphic.HumanMouthGraphic[3].Graphic.MatAt(facing);
                }
                else
                {
                    material = this.pawnFaceGraphic.MouthGraphic?.MatAt(facing);

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
                material = this.flasher.GetDamagedMat(material);
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
                    material = this.pawnFaceGraphic.WrinkleGraphic?.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.pawnFaceGraphic.RottingWrinkleGraphic?.MatAt(facing);
                }
            }

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        #endregion Public Methods
    }
}