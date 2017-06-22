namespace FacialStuff
{
    using UnityEngine;

    using Verse;

   public class HaircutPawn
    {
        public Pawn Pawn;

        public Graphic HairCutGraphic;

        public  Material HairCutMatAt(Rot4 facing)
        {
            Material material = this.HairCutGraphic.MatAt(facing);

            if (material != null)
            {
                material = this.Pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }
    }
}
