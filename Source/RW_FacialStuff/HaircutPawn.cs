using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RW_FacialStuff
{
    using UnityEngine;

    using Verse;

   public class HaircutPawn
    {
        public Pawn Pawn;

        public Graphic HairCutGraphic;

        public  Material HairCutMatAt(Rot4 facing)
        {
            Material material = HairCutGraphic.MatAt(facing, null);

            if (material != null)
            {
                material = Pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }
    }
}
