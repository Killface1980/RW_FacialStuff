using Verse;

namespace FacialStuff.Harmony.Optional.PrepC
{
    public class SaveRecordFaceV3 : IExposable
    {
        // private PawnFace face = new PawnFace();
        private PawnFace _face;

        public SaveRecordFaceV3()
        {
        }

        public SaveRecordFaceV3(Pawn pawn)
        {
            if (pawn.GetPawnFace(out PawnFace pawnFace))
            {
                this.Face = pawnFace;
            }
        }

        public PawnFace Face
        {
            get => this._face;
            private set => this._face = value;
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["PawnFace"] != null)
            {
                Scribe_Deep.Look(ref this._face, "PawnFace");
            }
        }

        public void SetFace(PawnFace pawnFace)
        {
            this._face = pawnFace;
        }
    }
}