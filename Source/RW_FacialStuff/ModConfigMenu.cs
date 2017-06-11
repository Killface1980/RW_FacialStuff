using HugsLib;
using RimWorld;
using UnityEngine;
using Verse;
using static UnityEngine.GUILayout;

namespace RW_FacialStuff
{
    public class FacialStuff_ModBase : ModBase
    {
        public override string ModIdentifier { get { return "FacialStuff"; } }

        public override void SettingsChanged()
        {

            base.SettingsChanged();
        }
    }

    public class FS_Mod : Mod
    {
        #region Fields
        public override string SettingsCategory() => "Facial Stuff";

        private ModSettings modSettings;

        #endregion

        public FS_Mod(ModContentPack content) : base(content)
        {
            this.modSettings = this.GetSettings<FS_Settings>();
        }

        #region Methods

        public override void WriteSettings()
        {
            if (this.modSettings != null)
            {
                this.modSettings.Write();
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            BeginArea(inRect);
            BeginVertical();
            FS_Settings.UseWrinkles = Toggle(FS_Settings.UseWrinkles, "Settings.UseWrinkles".Translate());
            EndVertical();
            BeginVertical();
            FS_Settings.UseMouth = Toggle(FS_Settings.UseMouth, "Settings.UseMouth".Translate());
            EndVertical();
            BeginVertical();
            if (Button("Settings.Apply".Translate()))
            {
                foreach (Pawn pawn in PawnsFinder.AllMapsAndWorld_Alive)
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        CompFace faceComp = pawn.TryGetComp<CompFace>();
                        if (faceComp != null)
                        {

                            faceComp.sessionOptimized = false;
                            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                            this.WriteSettings();
                        }

                    }
                }
            }
            EndVertical();
            EndArea();
        }

    }
    public class FS_Settings : ModSettings
    {
        public static bool UseMouth = false;

        public static bool UseWrinkles = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref UseWrinkles, "UseWrinkles", false, false);
            Scribe_Values.Look(ref UseMouth, "UseMouth", false, false);
        }
    }

    #endregion

}

