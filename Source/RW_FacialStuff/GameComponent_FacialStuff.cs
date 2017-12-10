using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using System.IO;

    using FacialStuff.Components;
    using FacialStuff.Graphics;

    using RimWorld;

    using UnityEngine;

    using Verse;

    using WHands;

    public class GameComponent_FacialStuff : GameComponent
    {
        public GameComponent_FacialStuff() { }

        public GameComponent_FacialStuff(Game game)
        {
            // Kill the damn beards - xml patching not reliable enough.
            for (int i = 0; i < DefDatabase<HairDef>.AllDefsListForReading.Count; i++)
            {
                HairDef hairDef = DefDatabase<HairDef>.AllDefsListForReading[i];
                CheckReplaceHairTexPath(hairDef);

                if (Controller.settings.UseCaching)
                {
                    string name = Path.GetFileNameWithoutExtension(hairDef.texPath);
                    CutHairDB.ExportHairCut(hairDef, name);
                }
            }
            WeaponComps();

        }

        private static List<string> spoonTex = new List<string> { "SPSBeard", "SPSScot", "SPSViking" };

        private static List<string> nackbladTex =
            new List<string> { "bushy", "crisis", "erik", "jr", "guard", "karl", "olof", "ruff", "trimmed" };

        private static void CheckReplaceHairTexPath(HairDef hairDef)
        {
            string folder;
            List<string> collection;
            if (hairDef.defName.Contains("SPS"))
            {
                collection = spoonTex;
                folder = "Spoon/";
            }
            else
            {
                collection = nackbladTex;
                folder = "Nackblad/";
            }

            for (int index = 0; index < collection.Count; index++)
            {
                string hairname = collection[index];
                if (!hairDef.defName.Equals(hairname))
                {
                    continue;
                }
                hairDef.texPath = "Hair/" + folder + hairname;
                break;
            }
        }

        private bool HandCheck()
        {
            return ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Clutter Laser Rifle");
        }

        private void LaserLoad()
        {
            if (this.HandCheck())
            {
                ThingDef wepzie = ThingDef.Named("LaserRifle");
                if (wepzie != null)
                {
                    CompProperties_WeaponExtensions Compie =
                        new CompProperties_WeaponExtensions
                            {
                                compClass = typeof(CompWeaponExtensions),
                                FirstHandPosition = new Vector3(-0.2f, 0.3f, -0.05f),
                                SecondHandPosition = new Vector3(0.25f, 0f, -0.05f)
                            };
                    wepzie.comps.Add(Compie);
                }
            }
        }

        public void WeaponComps()
        {
            ThingDef Tdef = ThingDef.Named("ClutterHandsSettings");
            ClutterHandsTDef tDef = Tdef as ClutterHandsTDef;
            if (!(tDef?.WeaponCompLoader.Count > 0))
            {
                return;
            }
            for (int i = 0; i < tDef.WeaponCompLoader.Count; i++)
            {
                ClutterHandsTDef.CompTargets wepSets = tDef.WeaponCompLoader[i];
                if (wepSets.thingTargets.Count <= 0)
                {
                    continue;
                }
                foreach (string t in wepSets.thingTargets)
                {
                    ThingDef thingDef = ThingDef.Named(t);
                    if (thingDef != null)
                    {
                        CompProperties_WeaponExtensions withHands =
                            new CompProperties_WeaponExtensions { compClass = typeof(CompWeaponExtensions) };
                        withHands.FirstHandPosition = wepSets.firstHandPosition;
                        withHands.SecondHandPosition = wepSets.secondHandPosition;

                        thingDef.comps.Add(withHands);
                        //  Log.Message("Added hands to: " + thingDef.defName);
                    }
                }
            }

            this.LaserLoad();
        }

    }
}
