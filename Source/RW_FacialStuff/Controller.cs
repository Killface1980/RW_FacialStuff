namespace FacialStuff
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class Controller : Mod
    {
        #region Public Fields

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1307
        [SuppressMessage(
      "StyleCop.CSharp.MaintainabilityRules",
      "SA1401:FieldsMustBePrivate",
      Justification = "Reviewed. Suppression is OK here.")]
        public static Settings settings;

        #endregion Public Fields

        #region Public Constructors

        public Controller(ModContentPack content)
            : base(content)
        {
            settings = this.GetSettings<Settings>();
        }

        #endregion Public Constructors

        #region Public Methods

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Facial Stuff";
        }

        // ReSharper disable once MissingXmlDoc
        public override void WriteSettings()
        {
            settings?.Write();

            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            foreach (Pawn pawn in from pawn in PawnsFinder.AllMapsAndWorld_Alive
                                  where pawn.RaceProps.Humanlike
                                  let faceComp = pawn.TryGetComp<CompFace>()
                                  where faceComp != null
                                  select pawn)
            {
                // This will force the renderer to make "AllResolved" return false, if pawn is drawn
                pawn.Drawer.renderer.graphics.nakedGraphic = null;
            }

            if (Find.ColonistBar != null)
            {
                Find.ColonistBar.MarkColonistsDirty();
            }

        }

        #endregion Public Methods
    }
}