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
        public List<PersonalFace> visuals = new List<PersonalFace>();

        public SaveRecordPsycheV3()
        {
        }

        public SaveRecordPsycheV3(Pawn pawn)
        {
            CompFace face = pawn.TryGetComp<CompFace>();
            if (face != null)
            {
                this.visuals = face.PersonalFaceSettings;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["personality"] != null)
            {
                Scribe_Collections.Look(ref this.visuals, "visuals", LookMode.Deep, null);
            }
        }
    }
}