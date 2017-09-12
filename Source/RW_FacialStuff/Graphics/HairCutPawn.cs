namespace FacialStuff.Graphics
{
    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public class HairCutPawn
    {
        #region Public Fields

        public Graphic HairCutGraphic;

        public Pawn Pawn;

        #endregion Public Fields

        #region Public Methods

        [CanBeNull]
        public Material HairCutMatAt(Rot4 facing)
        {
            Material material = this.HairCutGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.Pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }

        #endregion Public Methods
    }
}