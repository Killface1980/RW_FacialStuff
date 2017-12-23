using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using System.IO;

    using FacialStuff.Components;
    using FacialStuff.Defs;
    using FacialStuff.Graphics;

    using JetBrains.Annotations;

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

        public static void BuildWalkCycles([CanBeNull] WalkCycleDef defToRebuild = null)
        {
            List<WalkCycleDef> cycles = new List<WalkCycleDef>();
            if (defToRebuild != null)
            {
                cycles.Add(defToRebuild);
            }
            else
            {
                cycles = DefDatabase<WalkCycleDef>.AllDefsListForReading;
            }

            for (int index = 0; index < cycles.Count; index++)
            {
                WalkCycleDef cycle = cycles[index];
                cycle.BodyAngle = new SimpleCurve();
                cycle.BodyAngleVertical = new SimpleCurve();
                cycle.BodyOffsetZ = new SimpleCurve();
                cycle.BodyOffsetVerticalZ = new SimpleCurve();
                cycle.FootAngle = new SimpleCurve();
                cycle.FootPositionX = new SimpleCurve();
                cycle.FootPositionZ = new SimpleCurve();
                cycle.FootPositionVerticalZ = new SimpleCurve();
                cycle.HandsSwingAngle = new SimpleCurve();
                cycle.HandsSwingPosVertical = new SimpleCurve();

                if (cycle.animation.NullOrEmpty())
                {
                    cycle.animation = new List<PawnKeyframe>();
                    for (int i = 0; i < 9; i++)
                    {
                        cycle.animation.Add(new PawnKeyframe(i));
                    }
                }
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

            Dictionary<SimpleCurve, float?> dict = new Dictionary<SimpleCurve, float?>();

            dict.Add(cycle.BodyOffsetVerticalZ, key.BodyOffsetVerticalZ);
            dict.Add(cycle.BodyAngle, key.BodyAngle);
            dict.Add(cycle.BodyAngleVertical, key.BodyAngleVertical);
            dict.Add(cycle.BodyOffsetZ, key.BodyOffsetZ);
            dict.Add(cycle.FootAngle, key.FootAngle);
            dict.Add(cycle.FootPositionX, key.FootPositionX);
            dict.Add(cycle.FootPositionZ, key.FootPositionZ);
            dict.Add(cycle.FootPositionVerticalZ, key.FootPositionVerticalZ);
            dict.Add(cycle.HandsSwingAngle, key.HandsSwingAngle);
            dict.Add(cycle.HandsSwingPosVertical, key.HandsSwingPosVertical);

            foreach (KeyValuePair<SimpleCurve, float?> pair in dict)
            {
                UpdateCurve(key, pair.Value, pair.Key, frameAt);
            }

        }

        private static void UpdateCurve(PawnKeyframe key, float? curvePoint, SimpleCurve simpleCurve, float frameAt)
        {
            if (curvePoint.HasValue)
            {
                simpleCurve.Add(frameAt, curvePoint.Value);
            }
            else
            {
                if (key.keyIndex == 0)
                {
                    simpleCurve.Add(0, 0);
                    simpleCurve.Add(1, 0);
                }
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
