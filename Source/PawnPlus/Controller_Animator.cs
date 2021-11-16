namespace PawnPlus
{
    using UnityEngine;

    using Verse;

    public class Controller_Animator : Mod
    {
        #region Public Fields

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