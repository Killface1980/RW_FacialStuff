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

            // BuildWalkCycles();

            // foreach (BodyAnimDef def in DefDatabase<BodyAnimDef>.AllDefsListForReading)
            // {
            // if (def.walkCycles.Count == 0)
            // {
            // var length = Enum.GetNames(typeof(LocomotionUrgency)).Length;
            // for (int index = 0; index < length; index++)
            // {
            // WalkCycleDef cycleDef = new WalkCycleDef();
            // cycleDef.defName = def.defName + "_cycle";
            // def.walkCycles.Add(index, cycleDef);
            // }
            // }
            // }
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

                // cycle.BodyOffsetVerticalZ = new SimpleCurve();
                cycle.FootAngle = new SimpleCurve();
                cycle.FootPositionX = new SimpleCurve();
                cycle.FootPositionZ = new SimpleCurve();

                // cycle.FootPositionVerticalZ = new SimpleCurve();
                cycle.HandsSwingAngle = new SimpleCurve();
                cycle.HandsSwingPosVertical = new SimpleCurve();
                cycle.ShoulderOffsetHorizontalX = new SimpleCurve();
                cycle.HipOffsetHorizontalX = new SimpleCurve();

                // Quadrupeds
                cycle.FrontPawAngle = new SimpleCurve();
                cycle.FrontPawPositionX = new SimpleCurve();
                cycle.FrontPawPositionZ = new SimpleCurve();

                // cycle.FrontPawPositionVerticalZ = new SimpleCurve();
                if (cycle.keyframes.NullOrEmpty())
                {
                    cycle.keyframes = new List<PawnKeyframe>();
                    for (int i = 0; i < 9; i++)
                    {
                        cycle.keyframes.Add(new PawnKeyframe(i));
                    }
                }

                // Log.Message(cycle.defName + " has " + cycle.animation.Count);
                foreach (PawnKeyframe key in cycle.keyframes)
                {
                    BuildAnimationKeys(key, cycle);
                }
            }
        }

        private static void BuildAnimationKeys(PawnKeyframe key, WalkCycleDef cycle)
        {
            List<PawnKeyframe> keyframes = cycle.keyframes;

            List<PawnKeyframe> autoKeys = keyframes.Where(x => x.status != KeyStatus.Manual).ToList();

            List<PawnKeyframe> manualKeys = keyframes.Where(x => x.status == KeyStatus.Manual).ToList();

            float autoFrames = (float)key.keyIndex / (autoKeys.Count - 1);

            float frameAt;

            // Distribute manual keys
            if (!manualKeys.NullOrEmpty())
            {
                frameAt = (float)key.keyIndex / (autoKeys.Count - 1);
                Log.Message("frameAt " + frameAt.ToString());
                float divider = (float)1 / (autoKeys.Count - 1);
                Log.Message("divider " + divider.ToString());
                float? shift = manualKeys.Find(x => x.keyIndex == key.keyIndex)?.shift;
                if (shift.HasValue)
                {
                    Log.Message("shift " + shift.ToString());
                    frameAt += divider * shift.Value;
                    Log.Message("new frameAt " + frameAt.ToString());
                }

            }
            else
            {
                frameAt = (float)key.keyIndex / (keyframes.Count - 1);
            }

            Dictionary<SimpleCurve, float?> dict =
                new Dictionary<SimpleCurve, float?>
                    {
                        {
                            cycle.ShoulderOffsetHorizontalX,
                            key.ShoulderOffsetHorizontalX
                        },
                        { cycle.HipOffsetHorizontalX, key.HipOffsetHorizontalX },
                        { cycle.BodyAngle, key.BodyAngle },
                        { cycle.BodyAngleVertical, key.BodyAngleVertical },
                        { cycle.BodyOffsetZ, key.BodyOffsetZ },
                        { cycle.FootAngle, key.FootAngle },
                        { cycle.FootPositionX, key.FootPositionX },
                        { cycle.FootPositionZ, key.FootPositionZ },
                        { cycle.HandsSwingAngle, key.HandsSwingAngle },
                        { cycle.HandsSwingPosVertical, key.HandsSwingAngle },
                        { cycle.FrontPawAngle, key.FrontPawAngle },
                        { cycle.FrontPawPositionX, key.FrontPawPositionX },
                        { cycle.FrontPawPositionZ, key.FrontPawPositionZ },

                        // { cycle.BodyOffsetVerticalZ, key.BodyOffsetVerticalZ },

                        // { cycle.FootPositionVerticalZ, key.FootPositionVerticalZ },

                        // { cycle.HandsSwingPosVertical, key.HandsSwingPosVertical },
                        // {
                        // cycle.FrontPawPositionVerticalZ,
                        // key.FrontPawPositionVerticalZ
                        // }
                    };



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
                // No value at 0 => add points to prevent the curve from bugging out
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
