using RimWorld;
using RW_FacialStuff.Detouring;
using UnityEngine;
using Verse;
using static UnityEngine.GUILayout;

namespace RW_FacialStuff
{
    public class FS_Mod : Mod
    {
        #region Fields
        public override string SettingsCategory() => "Facial Stuff";

        #endregion

        public FS_Mod(ModContentPack content) : base(content)
        {
        }

        #region Methods

        public override void DoSettingsWindowContents(Rect inRect)
        {
            BeginArea(inRect);
            BeginVertical();
            FS_Settings.UseMouth = Toggle(FS_Settings.UseMouth, "Settings.UseMouth".Translate());
            EndVertical();
            BeginVertical();
            if (GUILayout.Button("Settings.Apply".Translate()))
            {
                foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
                {
                    if (pawn.RaceProps.IsFlesh && (pawn.kindDef.race.ToString().Equals("Human")))
                    {
                        CompFace faceComp = pawn.TryGetComp<CompFace>();
                        faceComp.sessionOptimized = false;
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();

                        if (pawn.Faction == Faction.OfPlayer)
                            PortraitsCache.SetDirty(pawn);

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

        public override void ExposeData()
        {
            Scribe_Values.Look(ref UseMouth, "UseMouth", false, false);
        }
    }

    #endregion

}

