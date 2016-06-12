using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RW_FacialHair
{
    public class Class1
    {
        Pawn pawn;
        Apparel ap;
        private readonly Apparel Moustache_Deluxe;

        public void PawnHair()
        {
            if (pawn.gender.Equals(1))
            {
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.defName.Contains("Moustache"))
                    {
                        ap = Moustache_Deluxe;
                        pawn.apparel.WornApparel.Add(Moustache_Deluxe);
                        pawn.story.BeardDef.
                          

                    }
                }
            }
        }




    }
}
