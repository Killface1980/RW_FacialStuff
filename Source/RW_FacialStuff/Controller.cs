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

        public const string modName = "Facial Stuff";

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

    // Backup:
    // <MaleAverageOffsetX>0.04716981</MaleAverageOffsetX>
    // <MaleAverageOffsetY>0.01320755</MaleAverageOffsetY>
    // <MaleNarrowOffsetX>-0.0245283</MaleNarrowOffsetX>
    // <MaleNarrowOffsetY>0.03773585</MaleNarrowOffsetY>
    // <FemaleAverageOffsetX>0.03584905</FemaleAverageOffsetX>
    // <FemaleAverageOffsetY>0.04150943</FemaleAverageOffsetY>
    // <FemaleNarrowOffsetX>0.009433965</FemaleNarrowOffsetX>
    // <FemaleNarrowOffsetY>0.06981131</FemaleNarrowOffsetY>
    // <MouthMaleAverageNormalOffset>0.02641509</MouthMaleAverageNormalOffset>
    // <MouthMaleAveragePointyOffset>0.02264151</MouthMaleAveragePointyOffset>
    // <MouthMaleAverageWideOffset>0.00754717</MouthMaleAverageWideOffset>
    // <MouthFemaleAverageNormalOffset>0.02452831</MouthFemaleAverageNormalOffset>
    // <MouthFemaleAveragePointyOffset>0.02452831</MouthFemaleAveragePointyOffset>
    // <MouthFemaleAverageWideOffset>0.04150943</MouthFemaleAverageWideOffset>
}