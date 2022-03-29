using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class Controller : Mod
    {
        private const string modName = "Facial Stuff";

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once StyleCop.SA1307
        [NotNull]
        public static Settings settings;

        private static bool? skActive;



        public Controller(ModContentPack content)
            : base(content)
        {
            settings = this.GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return modName;
        }

        // ReSharper disable once MissingXmlDoc
        public override void WriteSettings()
        {
            settings?.Write();

            SetMainButtons();
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            {
            }
            List<Pawn> allPawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.ToList();
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn pawn = allPawns[i];
                if (!pawn.HasCompAnimator())
                {
                    continue;
                }

                pawn.Drawer.renderer.graphics.nakedGraphic = null;
                PortraitsCache.SetDirty(pawn);
            }

            // Bug: Not working when called or retrieved inside a mod
            // if (Find.ColonistBar != null)
            // {
            // Find.ColonistBar.MarkColonistsDirty();
            // }
        }

        public static void SetMainButtons()
        {
            MainButtonDef button = DefDatabase<MainButtonDef>.GetNamedSilentFail("WalkAnimator");
            //   MainButtonDef button2 = DefDatabase<MainButtonDef>.GetNamedSilentFail("PoseAnimator");
            button.buttonVisible = settings.Develop;
        }
    }
}