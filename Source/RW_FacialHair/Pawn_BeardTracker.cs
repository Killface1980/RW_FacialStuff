using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_FacialHair
{
    public class Pawn_BeardTracker : IExposable
    {
        public HairDef hairDef;

        private Pawn pawn;

        public Pawn_BeardTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void ExposeData()
        {
            Scribe_Defs.LookDef(ref beardDef, "beardDef");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && beardDef == null)
            {
                beardDef = DefDatabase<BeardDef>.AllDefs.RandomElement();
            }
        }
    }
}