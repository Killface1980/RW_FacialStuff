using FacialStuff.Harmony;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff.HairCut
{
    public class HairCutPawn
    {
        [CanBeNull] public Graphic HairCutGraphic;

        public Pawn Pawn;

        [CanBeNull]
        public Material HairCutMatAt(Rot4 facing)
        {
            if (this.HairCutGraphic == null)
            {
                HarmonyPatchesFS.ResolveApparelGraphics_Postfix(this.Pawn.Drawer.renderer.graphics);

            }

            Material material = this.HairCutGraphic?.MatAt(facing);

            if (!material.NullOrBad())
            {
                material = this.Pawn.Drawer.renderer.graphics.flasher.GetDamagedMat(material);
            }

            return material;
        }
    }
}