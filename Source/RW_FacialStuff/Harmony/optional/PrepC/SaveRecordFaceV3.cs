namespace FacialStuff.Harmony.optional.PrepC
{
    using FacialStuff;

    using Verse;

    public class SaveRecordFaceV3 : IExposable
    {
        private PawnFace face = new PawnFace();

        public SaveRecordFaceV3()
        {
        }

        public SaveRecordFaceV3(Pawn pawn)
        {
            PawnFace pawnFace = pawn.TryGetComp<CompFace>().PawnFace;
            if (pawnFace != null)
            {
                this.Face = pawnFace;
            }
        }

        public PawnFace Face { get => this.face; set => this.face = value; }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["PawnFace"] != null)
            {
                Scribe_Deep.Look(ref this.face, "PawnFace");
            }
        }
    }
}