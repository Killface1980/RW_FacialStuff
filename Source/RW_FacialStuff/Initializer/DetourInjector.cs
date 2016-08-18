using CommunityCoreLibrary;
using RimWorld;
using RW_FacialStuff.Sexuality;
using Verse;

namespace RW_FacialStuff
{
    class DetourInjector : SpecialInjector
    {
        public override bool Inject()
        {
            // Detour FloatMenuMaker
            if (!Detours.TryDetourFromTo(typeof(Pawn_RelationsTracker).GetMethod("AttractionTo"),
                typeof(Pawn_RelationsTrackerModded).GetMethod("AttractionToModded")))
                return false;
         // if (!Detours.TryDetourFromTo(typeof(PawnRenderer).GetMethod("RenderPawnAt"),
         //     typeof(PawnRendererModded).GetMethod("RenderPawnAt")))
         //     return false;
            return true;
        }

    }
}
