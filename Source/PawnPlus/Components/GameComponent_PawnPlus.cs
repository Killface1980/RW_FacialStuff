﻿namespace PawnPlus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using JetBrains.Annotations;

    using PawnPlus.Defs;

    using RimWorld;
    using RimWorld.Planet;

    using UnityEngine;

    using Verse;

    public class GameComponent_PawnPlus : GameComponent
    {
        protected Animator animator;
        
        private float faceMeshSize;

        public bool ShouldRenderFaceDetails { get; private set; }

        #region Public Constructors
        
        public GameComponent_PawnPlus()
        {
        }

        // ReSharper disable once UnusedParameter.Local
        public GameComponent_PawnPlus(Game game)
        {
            WeaponCompsNew();
            AnimalPawnCompsBodyDefImport();
            AnimalPawnCompsImportFromAnimationTargetDefs();
            Controller.SetMainButtons();

            // Get head mesh size using reflection. Face mesh size is half of head mesh's.
            FieldInfo headAvgWidthField = typeof(MeshPool).GetField("HumanlikeHeadAverageWidth", BindingFlags.NonPublic | BindingFlags.Static);
            if(headAvgWidthField != null)
			{
                try
				{
                    faceMeshSize = (float)headAvgWidthField.GetValue(null) / 2f;
                    return;
                }
                catch(Exception e)
				{

				}
			}

            Log.Message("Pawn Plus: Couldn't retrieve the value MeshPool.HumanlikeHeadAverageWidth. Using the default value of 0.75 for face part culling.");
            faceMeshSize = 0.75f;
        }

        #endregion Public Constructors

        #region Public Methods
        
		public override void GameComponentTick()
        {
            base.GameComponentTick();

            // Perform rudimentary distance culling. Don't render if camera isn't zoomed in enough for small facial details.
            // GameComponentTick() is called before rendering the map. Therefore, this calculation can be done here.
            if (WorldRendererUtility.CurrentWorldRenderMode == WorldRenderMode.None && Find.CurrentMap != null)
            {
                float meshWidth, meshHeight;

                // Since the camera is orthorgraphic, the location of pawn relative to camera doesn't matter - All face mesh 
                // will be rendered with the same dimension on screen.
                Vector3 screenCoord = Find.Camera.WorldToScreenPoint(new Vector3(faceMeshSize, 0f, faceMeshSize));
                float minPixelDim = Mathf.Min(Mathf.Abs(screenCoord.x), Mathf.Abs(screenCoord.y));
                ShouldRenderFaceDetails = minPixelDim >= 10;
            }
            else
            {
                // Shouldn't matter because pawns aren't rendered in world view, but just in case. Also, portrait view 
                // doesn't consider LOD and renders at full detail.
                ShouldRenderFaceDetails = true;
            }
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

            if (cycles == null)
            {
                return;
            }

            for (int index = 0; index < cycles.Count; index++)
            {
                WalkCycleDef cycle = cycles[index];
                if (cycle != null)
                {
                    cycle.HeadAngleX = new SimpleCurve();
                    cycle.HeadOffsetZ = new SimpleCurve();

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
        }

        /// <summary>
        /// Pose cycles, currently disabled; needs more work
        /// </summary>
        /// <param name="defToRebuild"></param>
        public static void BuildPoseCycles([CanBeNull] PoseCycleDef defToRebuild = null)
        {
            List<PoseCycleDef> cycles = new List<PoseCycleDef>();
            if (defToRebuild != null)
            {
                cycles.Add(defToRebuild);
            }
            else
            {
                cycles = DefDatabase<PoseCycleDef>.AllDefsListForReading;
            }

            if (cycles != null)
            {
                for (int index = 0; index < cycles.Count; index++)
                {
                    PoseCycleDef cycle = cycles[index];
                    if (cycle != null)
                    {
                        cycle.BodyAngle = new SimpleCurve();
                        cycle.BodyAngleVertical = new SimpleCurve();
                        cycle.BodyOffsetZ = new SimpleCurve();
                        cycle.FootAngle = new SimpleCurve();
                        cycle.FootPositionX = new SimpleCurve();
                        cycle.FootPositionZ = new SimpleCurve();
                        cycle.HandPositionX = new SimpleCurve();
                        cycle.HandPositionZ = new SimpleCurve();
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
                        if (cycle.keyframes != null)
                        {
                            foreach (PawnKeyframe key in cycle.keyframes)
                            {
                                BuildAnimationKeys(key, cycle);
                            }
                        }
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void BuildAnimationKeys(PawnKeyframe key, WalkCycleDef cycle)
        {
            List<PawnKeyframe> keyframes = cycle.keyframes;

            List<PawnKeyframe> autoKeys = keyframes.Where(x => x.Status != KeyStatus.Manual).ToList();

            List<PawnKeyframe> manualKeys = keyframes.Where(x => x.Status == KeyStatus.Manual).ToList();

            float autoFrames = (float)key.KeyIndex / (autoKeys.Count - 1);

            float frameAt;

            // Distribute manual keys
            if (!manualKeys.NullOrEmpty())
            {
                frameAt = (float)key.KeyIndex / (autoKeys.Count - 1);
                float divider = (float)1 / (autoKeys.Count - 1);
                float? shift = manualKeys.Find(x => x.KeyIndex == key.KeyIndex)?.Shift;
                if (shift.HasValue)
                {
                    frameAt += divider * shift.Value;
                }
            }
            else
            {
                frameAt = (float)key.KeyIndex / (keyframes.Count - 1);
            }

            Dictionary<SimpleCurve, float?> dict = new Dictionary<SimpleCurve, float?>
                                                       {
                                                           {cycle.HeadAngleX, key.HeadAngleX },
                                                           {cycle.HeadOffsetZ, key.HeadOffsetZ },
                                                           {
                                                               cycle.ShoulderOffsetHorizontalX,
                                                               key.ShoulderOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.HipOffsetHorizontalX,
                                                               key.HipOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.BodyAngleVertical,
                                                               key.BodyAngleVertical
                                                           },
                                                           {
                                                               cycle.BodyOffsetZ,
                                                               key.BodyOffsetZ
                                                           },
                                                           {
                                                               cycle.FootAngle,
                                                               key.FootAngle
                                                           },
                                                           {
                                                               cycle.FootPositionX,
                                                               key.FootPositionX
                                                           },
                                                           {
                                                               cycle.FootPositionZ,
                                                               key.FootPositionZ
                                                           },
                                                           {
                                                               cycle.HandsSwingAngle,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.HandsSwingPosVertical,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.FrontPawAngle,
                                                               key.FrontPawAngle
                                                           },
                                                           {
                                                               cycle.FrontPawPositionX,
                                                               key.FrontPawPositionX
                                                           },
                                                           {
                                                               cycle.FrontPawPositionZ,
                                                               key.FrontPawPositionZ
                                                           }
                                                       };

            foreach (KeyValuePair<SimpleCurve, float?> pair in dict)
            {
                UpdateCurve(key, pair.Value, pair.Key, frameAt);
            }
        }

        private static void BuildAnimationKeys(PawnKeyframe key, PoseCycleDef cycle)
        {
            List<PawnKeyframe> keyframes = cycle.keyframes;

            List<PawnKeyframe> autoKeys = keyframes.Where(x => x.Status != KeyStatus.Manual).ToList();

            List<PawnKeyframe> manualKeys = keyframes.Where(x => x.Status == KeyStatus.Manual).ToList();

            float autoFrames = (float)key.KeyIndex / (autoKeys.Count - 1);

            float frameAt;

            // Distribute manual keys
            if (!manualKeys.NullOrEmpty())
            {
                frameAt = (float)key.KeyIndex / (autoKeys.Count - 1);
                Log.Message("frameAt " + frameAt);
                float divider = (float)1 / (autoKeys.Count - 1);
                Log.Message("divider " + divider);
                float? shift = manualKeys.Find(x => x.KeyIndex == key.KeyIndex)?.Shift;
                if (shift.HasValue)
                {
                    Log.Message("Shift " + shift);
                    frameAt += divider * shift.Value;
                    Log.Message("new frameAt " + frameAt);
                }
            }
            else
            {
                frameAt = (float)key.KeyIndex / (keyframes.Count - 1);
            }

            Dictionary<SimpleCurve, float?> dict = new Dictionary<SimpleCurve, float?>
                                                       {
                                                           {
                                                               cycle.ShoulderOffsetHorizontalX,
                                                               key.ShoulderOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.HipOffsetHorizontalX,
                                                               key.HipOffsetHorizontalX
                                                           },
                                                           {
                                                               cycle.BodyAngle,
                                                               key.BodyAngle
                                                           },
                                                           {
                                                               cycle.BodyAngleVertical,
                                                               key.BodyAngleVertical
                                                           },
                                                           {
                                                               cycle.BodyOffsetZ,
                                                               key.BodyOffsetZ
                                                           },
                                                           {
                                                               cycle.FootAngle,
                                                               key.FootAngle
                                                           },
                                                           {
                                                               cycle.FootPositionX,
                                                               key.FootPositionX
                                                           },
                                                           {
                                                               cycle.FootPositionZ,
                                                               key.FootPositionZ
                                                           },
                                                           {
                                                               cycle.HandPositionX,
                                                               key.HandPositionX
                                                           },
                                                           {
                                                               cycle.HandPositionZ,
                                                               key.HandPositionZ
                                                           },
                                                           {
                                                               cycle.HandsSwingAngle,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.HandsSwingPosVertical,
                                                               key.HandsSwingAngle
                                                           },
                                                           {
                                                               cycle.FrontPawAngle,
                                                               key.FrontPawAngle
                                                           },
                                                           {
                                                               cycle.FrontPawPositionX,
                                                               key.FrontPawPositionX
                                                           },
                                                           {
                                                               cycle.FrontPawPositionZ,
                                                               key.FrontPawPositionZ
                                                           }
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
                if (key.KeyIndex == 0)
                {
                    simpleCurve.Add(0, 0);
                    simpleCurve.Add(1, 0);
                }
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

        private void WeaponCompsNew()
        {
            // ReSharper disable once PossibleNullReferenceException
            foreach (WeaponExtensionDef weaponExtensionDef in DefDatabase<WeaponExtensionDef>.AllDefsListForReading)
            {
                ThingDef thingDef;
                try
                {
                    thingDef = ThingDef.Named(weaponExtensionDef.weapon);

                }
                catch
                {
                    continue;
                }

                if (thingDef == null)
                {
                    continue;
                }

                if (thingDef.HasComp(typeof(CompProperties_WeaponExtensions)))
                {
                    return;
                }

                CompProperties_WeaponExtensions weaponExtensions =
                new CompProperties_WeaponExtensions
                {
                    compClass = typeof(CompWeaponExtensions),
                    AttackAngleOffset = weaponExtensionDef.attackAngleOffset,
                    WeaponPositionOffset = weaponExtensionDef.weaponPositionOffset,
                    AimedWeaponPositionOffset = weaponExtensionDef.aimedWeaponPositionOffset,
                    RightHandPosition = weaponExtensionDef.firstHandPosition,
                    LeftHandPosition = weaponExtensionDef.secondHandPosition
                };

                thingDef.comps?.Add(weaponExtensions);
            }

            this.LaserLoad();
        }

        private void AnimalPawnCompsImportFromAnimationTargetDefs()
        {
            // ReSharper disable once PossibleNullReferenceException
            foreach (AnimationTargetDef def in DefDatabase<AnimationTargetDef>.AllDefsListForReading)
            {
                if (def.CompLoaderTargets.NullOrEmpty())
                {
                    continue;
                }

                foreach (CompLoaderTargets pawnSets in def.CompLoaderTargets)
                {
                    if (pawnSets == null)
                    {
                        continue;
                    }

                    if (pawnSets.thingTargets.NullOrEmpty())
                    {
                        continue;
                    }

                    foreach (string target in pawnSets.thingTargets)
                    {
                        ThingDef thingDef = ThingDef.Named(target);
                        if (thingDef == null)
                        {
                            continue;
                        }

                        // if (DefDatabase<BodyAnimDef>
                        // .AllDefsListForReading.Any(x => x.defName.Contains(thingDef.defName))) continue;
                        if (thingDef.HasComp(typeof(CompBodyAnimator)))
                        {
                            continue;
                        }

                        CompProperties_BodyAnimator bodyAnimator = new CompProperties_BodyAnimator
                                                                       {
                                                                           compClass = typeof(CompBodyAnimator),
                                                                           bodyDrawers = pawnSets.bodyDrawers,
                                                                           handType = pawnSets.handType,

                                                                           // footType = pawnSets.footType,
                                                                           // pawType = pawnSets.pawType,
                                                                           quadruped = pawnSets.quadruped,
                                                                           bipedWithHands = pawnSets.bipedWithHands
                                                                       };
                        thingDef.comps?.Add(bodyAnimator);
                    }
                }
            }

            this.LaserLoad();
        }

        private void AnimalPawnCompsBodyDefImport()
        {
            // ReSharper disable once PossibleNullReferenceException
            foreach (BodyAnimDef def in DefDatabase<BodyAnimDef>.AllDefsListForReading)
            {
                string target = def.thingTarget;
                if (target.NullOrEmpty())
                {
                    continue;
                }

                ThingDef thingDef = ThingDef.Named(target);
                if (thingDef == null)
                {
                    continue;
                }

                // if (DefDatabase<BodyAnimDef>
                // .AllDefsListForReading.Any(x => x.defName.Contains(thingDef.defName))) continue;
                if (thingDef.HasComp(typeof(CompBodyAnimator)))
                {
                    continue;
                }

                CompProperties_BodyAnimator bodyAnimator = new CompProperties_BodyAnimator
                                                               {
                                                                   compClass = typeof(CompBodyAnimator),
                                                                   bodyDrawers = def.bodyDrawers,
                                                                   handType = def.handType,

                                                                   // footType = def.footType,
                                                                   // pawType = def.pawType,
                                                                   quadruped = def.quadruped,
                                                                   bipedWithHands = def.bipedWithHands
                                                               };

                thingDef.comps?.Add(bodyAnimator);
            }
        }

        #endregion Private Methods
    }
}