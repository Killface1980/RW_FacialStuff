using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class Controller_Animator : Mod
    {
        #region Public Fields

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1307
        [SuppressMessage(
            "StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "Reviewed. Suppression is OK here.")]
        public static Settings_Animator settings;

        #endregion Public Fields

        #region Public Constructors

        public Controller_Animator(ModContentPack content)
            : base(content)
        {
            settings = this.GetSettings<Settings_Animator>();
        }

        #endregion Public Constructors

        #region Public Methods

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings?.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Facial Stuff Animator";
        }

        // ReSharper disable once MissingXmlDoc
        public override void WriteSettings()
        {
            settings?.Write();
        }

        #endregion Public Methods
    }
}