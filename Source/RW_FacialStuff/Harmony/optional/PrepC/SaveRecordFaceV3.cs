namespace FacialStuff.Harmony.Optional.PrepC
{
    using Verse;

    public class SaveRecordFaceV3 : IExposable
    {
        // private PawnFace face = new PawnFace();
        private PawnFace face;

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

        public PawnFace Face
        {
            get => this.face;
            private set => this.face = value;
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["PawnFace"] != null)
            {
                Scribe_Deep.Look(ref this.face, "PawnFace");
            }
        }

        public void SetFace(PawnFace pawnFace)
        {
            this.face = pawnFace;
        }
    }
}