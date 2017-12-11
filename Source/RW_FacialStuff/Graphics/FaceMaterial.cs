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
        private readonly PawnGraphic pawnGraphic;

        private Pawn pawn;

        #endregion Private Fields

        #region Public Constructors

        public FaceMaterial([NotNull] CompFace compFace, [NotNull] PawnGraphic pawnGraphic)
        {
            this.compFace = compFace;
            this.pawnGraphic = pawnGraphic;
            this.pawn = this.compFace.Pawn;
            this.flasher = this.pawn.Drawer.renderer.graphics.flasher;

        }

        #endregion Public Constructors

        #region Public Methods

        [CanBeNull]
        public Material BeardMatAt(Rot4 facing)
        {
            if (this.pawn.gender != Gender.Male || this.compFace.PawnFace?.BeardDef == BeardDefOf.Beard_Shaved
                || !this.compFace.hasNaturalJaw)
            {
                return null;
            }

            Material material = this.pawnGraphic.MainBeardGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material BrowMatAt(Rot4 facing)
        {
            Material material = this.pawnGraphic.BrowGraphic?.MatAt(facing);

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
                material = this.pawnGraphic.DeadEyeGraphic?.MatAt(facing);
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                material = this.pawnGraphic.DeadEyeGraphic?.MatAt(facing);
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
            if (this.compFace.HasEyePatchLeft)
            {
                return null;
            }

            if (facing == Rot4.East)
            {
                return null;
            }

            Material material = this.pawnGraphic.EyeLeftGraphic?.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.compFace.EyeWiggler.EyeLeftCanBlink)
                {
                    if (this.compFace.EyeWiggler.IsAsleep || this.compFace.EyeWiggler.EyeLeftBlinkNow)
                    {
                        material = this.pawnGraphic.EyeLeftClosedGraphic.MatAt(facing);
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
            Material material = this.pawnGraphic.EyeLeftPatchGraphic?.MatAt(facing);

            if (material != null)
            {
                material = this.flasher.GetDamagedMat(material);
            }

            return material;
        }

        [CanBeNull]
        public Material EyeRightMatAt(Rot4 facing, bool portrait)
        {
            if (this.compFace.HasEyePatchRight)
            {
                return null;
            }

            if (facing == Rot4.West)
            {
                return null;
            }

            Material material = this.pawnGraphic.EyeRightGraphic?.MatAt(facing);

            if (!portrait)
            {
                if (Controller.settings.MakeThemBlink && this.compFace.EyeWiggler.EyeRightCanBlink)
                {
                    if (this.compFace.EyeWiggler.IsAsleep || this.compFace.EyeWiggler.EyeRightBlinkNow)
                    {
                        material = this.pawnGraphic.EyeRightClosedGraphic?.MatAt(facing);
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
            Material material = this.pawnGraphic.EyeRightPatchGraphic?.MatAt(facing);

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
                || !this.compFace.hasNaturalJaw)
            {
                return null;
            }

            Material material = this.pawnGraphic.MoustacheGraphic?.MatAt(facing);

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

            if (!this.compFace.hasNaturalJaw && Controller.settings.ShowExtraParts)
            {
                material = this.pawnGraphic.JawGraphic?.MatAt(facing);
            }
            else
            {
                if (!Controller.settings.UseMouth || !this.compFace.PawnFace.DrawMouth)
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
                    material = this.pawnGraphic.mouthgraphic.HumanMouthGraphic[3].Graphic.MatAt(facing);
                }
                else
                {
                    material = this.pawnGraphic.MouthGraphic?.MatAt(facing);

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
                    material = this.pawnGraphic.WrinkleGraphic?.MatAt(facing);
                }
                else if (bodyCondition == RotDrawMode.Rotting)
                {
                    material = this.pawnGraphic.RottingWrinkleGraphic?.MatAt(facing);
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