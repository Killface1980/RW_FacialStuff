using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology.PrepareCarefully
{
    using FacialStuff;

    public class SaveRecordPsycheV3 : IExposable
    {
        private PawnFace visuals;

        public SaveRecordPsycheV3()
        {
        }

        public SaveRecordPsycheV3(Pawn pawn)
        {
            CompFace face = pawn.TryGetComp<CompFace>();
            if (face != null)
            {
                this.visuals = face.pawnFace;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["visuals"] != null)
            {
                Scribe_Deep.Look(ref this.visuals, "visuals");
            }
        }
    }
}