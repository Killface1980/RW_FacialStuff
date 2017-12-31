namespace FacialStuff
{
    using System.Diagnostics.CodeAnalysis;

    using JetBrains.Annotations;

    using UnityEngine;

    using Verse;

    public class Controller_Animator : Mod
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1307
        [SuppressMessage(
            "StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "Reviewed. Suppression is OK here.")]
        public static Settings_Animator settings;

        public Controller_Animator(ModContentPack content)
            : base(content)
        {
            settings = this.GetSettings<Settings_Animator>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }

        [NotNull]
        public override string SettingsCategory()
        {
            return "Facial Stuff Animator";
        }

        // ReSharper disable once MissingXmlDoc
        public override void WriteSettings()
        {
            settings?.Write();

        }
    }
}