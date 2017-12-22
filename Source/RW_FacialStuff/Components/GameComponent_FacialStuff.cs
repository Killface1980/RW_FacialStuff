using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using System.IO;

    using FacialStuff.Components;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class GameComponent_FacialStuff : GameComponent
    {
        #region Private Fields

        private static readonly List<string> NackbladTex =
            new List<string> { "bushy", "crisis", "erik", "jr", "guard", "karl", "olof", "ruff", "trimmed" };

        private static readonly List<string> SpoonTex = new List<string> { "SPSBeard", "SPSScot", "SPSViking" };

        #endregion Private Fields

        #region Public Constructors

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
            this.WeaponComps();
            BuildWalkCycles();
        }

        public static void BuildWalkCycles()
        {
            foreach (WalkCycleDef cycle in DefDatabase<WalkCycleDef>.AllDefsListForReading)
            {
                cycle.BodyAngle = new SimpleCurve();
                cycle.BodyAngleVertical = new SimpleCurve();
                cycle.BodyOffsetVertical = new SimpleCurve();
                cycle.FootAngle = new SimpleCurve();
                cycle.FootPositionX = new SimpleCurve();
                cycle.FootPositionY = new SimpleCurve();
                cycle.HandsSwingAngle = new SimpleCurve();
                cycle.HandsSwingPosVertical = new SimpleCurve();

               // Log.Message(cycle.defName + " has " + cycle.animation.Count);
                foreach (PawnKeyframe key in cycle.animation)
                {
                    BuildAnimationKeys(key, cycle);
                }
            }
        }

        private static void BuildAnimationKeys(PawnKeyframe key, WalkCycleDef cycle)
        {
            float frameAt = (float)key.keyIndex / (cycle.animation.Count - 1);

           // Log.Message("Adding key for " + cycle.defName + " at " + frameAt);

            float? bodyAngle = key.BodyAngle;
            if (bodyAngle.HasValue)
            {
                cycle.BodyAngle.Add(frameAt, bodyAngle.Value);
            }

            float? bodyAngleVertical = key.BodyAngleVertical;
            if (bodyAngleVertical.HasValue)
            {
                cycle.BodyAngleVertical.Add(frameAt, bodyAngleVertical.Value);
            }

            float? bodyOffsetVertical = key.BodyOffsetVertical;
            if (bodyOffsetVertical.HasValue)
            {
                cycle.BodyOffsetVertical.Add(frameAt, bodyOffsetVertical.Value);
            }

            float? footAngle = key.FootAngle;
            if (footAngle.HasValue)
            {
                cycle.FootAngle.Add(frameAt, footAngle.Value);
            }

            float? footPositionX = key.FootPositionX;
            if (footPositionX.HasValue)
            {
                cycle.FootPositionX.Add(frameAt, footPositionX.Value);
            }

            float? footPositionY = key.FootPositionY;
            if (footPositionY.HasValue)
            {
                cycle.FootPositionY.Add(frameAt, footPositionY.Value);
            }

            float? handsSwingAngle = key.HandsSwingAngle;
            if (handsSwingAngle.HasValue)
            {
                cycle.HandsSwingAngle.Add(frameAt, handsSwingAngle.Value);
            }

            float? handsSwingPosVertical = key.HandsSwingPosVertical;
            if (handsSwingPosVertical.HasValue)
            {
                cycle.HandsSwingPosVertical.Add(frameAt, handsSwingPosVertical.Value);
            }
        }

        #endregion Public Constructors

        #region Public Methods

        public void WeaponComps()
        {
            for (int index = 0; index < DefDatabase<HandDef>.AllDefsListForReading.Count; index++)
            {
                HandDef handDef = DefDatabase<HandDef>.AllDefsListForReading[index];
                if (handDef.WeaponCompLoader.NullOrEmpty())
                {
                    continue;
                }

                for (int i = 0; i < handDef.WeaponCompLoader.Count; i++)
                {
                    HandDef.CompTargets wepSets = handDef.WeaponCompLoader[i];
                    if (wepSets.thingTargets.NullOrEmpty())
                    {
                        continue;
                    }
                    foreach (string t in wepSets.thingTargets)
                    {
                        ThingDef thingDef = ThingDef.Named(t);
                        if (thingDef != null)
                        {
                            CompProperties_WeaponExtensions weaponExtensions =
                                thingDef.GetCompProperties<CompProperties_WeaponExtensions>();
                            bool flag = false;

                            if (weaponExtensions == null)
                            {
                                weaponExtensions =
                                    new CompProperties_WeaponExtensions { compClass = typeof(CompWeaponExtensions) };
                                flag = true;
                            }

                            if (weaponExtensions.RightHandPosition == Vector3.zero)
                            {
                                weaponExtensions.RightHandPosition = wepSets.firstHandPosition;
                            }

                            if (weaponExtensions.LeftHandPosition == Vector3.zero)
                            {
                                weaponExtensions.LeftHandPosition = wepSets.secondHandPosition;
                            }

                            if (!weaponExtensions.AttackAngleOffset.HasValue)
                            {
                                weaponExtensions.AttackAngleOffset = wepSets.attackAngleOffset;
                            }

                            if (weaponExtensions.WeaponPositionOffset == Vector3.zero)
                            {
                                weaponExtensions.WeaponPositionOffset = wepSets.weaponPositionOffset;
                            }

                            if (flag)
                            {
                                thingDef.comps.Add(weaponExtensions);
                            }
                        }
                    }
                }
            }

            this.LaserLoad();
        }

        #endregion Public Methods

        #region Private Methods

        private static void CheckReplaceHairTexPath(HairDef hairDef)
        {
            string folder;
            List<string> collection;
            if (hairDef.defName.Contains("SPS"))
            {
                collection = SpoonTex;
                folder = "Spoon/";
            }
            else
            {
                collection = NackbladTex;
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
                    CompProperties_WeaponExtensions extensions =
                        new CompProperties_WeaponExtensions
                        {
                            compClass = typeof(CompWeaponExtensions),
                            RightHandPosition = new Vector3(-0.2f, 0.3f, -0.05f),
                            LeftHandPosition = new Vector3(0.25f, 0f, -0.05f)
                        };
                    wepzie.comps.Add(extensions);
                }
            }
        }

        #endregion Private Methods
    }
}
