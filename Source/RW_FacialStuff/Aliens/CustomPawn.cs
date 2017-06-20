using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Aliens
{
    using Verse;

    public class CustomPawn 
    {
        public Pawn Pawn;

        private AlienRace alienRace = null;

        public CustomPawn()
        {

        }

        public CustomPawn(Pawn pawn)
        {
            InitializeWithPawn(pawn);
        }

        public void InitializeWithPawn(Pawn pawn)
        {
            this.Pawn = pawn;
            alienRace = AlienStuff.Instance.Providers.AlienRaces.GetAlienRace(pawn.def);

        }

        public AlienRace AlienRace
        {
            get
            {
                return this.alienRace;
            }
        }
    }
}
