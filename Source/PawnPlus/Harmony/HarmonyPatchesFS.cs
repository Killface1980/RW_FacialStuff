namespace PawnPlus.Harmony
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using HarmonyLib;

    using PawnPlus.AnimatorWindows;
    using PawnPlus.Tweener;

    using RimWorld;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public static class HarmonyPatchesFS
    {
        public const TweenThing equipment = TweenThing.Equipment;

        #region Public Constructors

        static HarmonyPatchesFS()
        {
            Harmony harmony = new Harmony("rimworld.pawnplus.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("PP Initialized");
            Log.Message("PP patching ResolveAllGraphics");
            harmony.Patch(
                AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                null,
                new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(ResolveAllGraphics_Postfix)));

            Log.Message("PP patching ResolveApparelGraphics");
            harmony.Patch(
                AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveApparelGraphics)),
                null,
                new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(ResolveApparelGraphics_Postfix)));
            
            Log.Message("PP patching DrawEquipmentAiming");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming)),
                new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DrawEquipmentAiming_Prefix)),
                null,
                new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DrawEquipmentAiming_Transpiler)));

            Log.Message("PP patching DrawEquipmentAiming");
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair"), new HarmonyMethod(
                typeof(HarmonyPatchesFS),
                nameof(DrawHeadHair_Prefix)), null);
                

            Log.Message("PP patching DrawCarriedThing");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "DrawCarriedThing", new[]{ typeof(Vector3)}),
                null,
                null,
                new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DrawCarriedThing_Transpiler)));
          /*
            Log.Message("PP patching RenderPawnInternal");
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[]{ typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags) }),
                null,
                null,
                new HarmonyMethod(typeof(HarmonyPatch_PawnRenderer), nameof(HarmonyPatch_PawnRenderer.Transpiler)));
            */
            Log.Message("PP patching DirtyCache");
            harmony.Patch(
                AccessTools.Method(typeof(HediffSet), nameof(HediffSet.DirtyCache)),
                null,
                new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DirtyCache_Postfix)));
            
            Log.Message(
                "Pawn Plus: successfully completed " + harmony.GetPatchedMethods().Count() + " patches with harmony.");
        }

        public static bool IsBeardNotHair(this Def def)
        {
            if (def.defName.StartsWith("VHE_Beard"))
            {
                return true;
            }

            return false;
        }

        #endregion Public Constructors
        
        #region Private Fields
        
        private static float angleStanding = 143f;
        private static float angleStandingFlipped = 217f;

        public static bool AnimatorIsOpen()
        {
            return Find.WindowStack.IsOpen(typeof(MainTabWindow_WalkAnimator));// MainTabWindow_WalkAnimator.IsOpen;// || MainTabWindow_PoseAnimator.IsOpen;
        }

        public static bool IsAnimated_Prefix(Pawn pawn, ref bool __result)
        {
            if (AnimatorIsOpen() && MainTabWindow_WalkAnimator.Pawn == pawn)
            {
                __result = true;
                return false;
            }

            return true;
        }
        
        #endregion Private Fields

        #region Public Methods
        
        public static void CheckAndDrawHands(Thing carriedThing, Vector3 thingVector3, bool flip, Pawn pawn, bool thingBehind)
        {
            if(pawn.RaceProps.Animal)
            {
                carriedThing.DrawAt(thingVector3, flip);
                return;
            }

            if(!pawn.GetCompAnim(out CompBodyAnimator compAnim))
            {
                carriedThing.DrawAt(thingVector3, flip);
                return;
            }

            bool showHands = compAnim.Props.bipedWithHands && Controller.settings.UseHands;
            if(!showHands)
            {
                carriedThing.DrawAt(thingVector3, flip);
                return;
            }

            // Modify the drawPos to appear behind a pawn if facing North, in case vanilla didn't
            if(!thingBehind && pawn.Rotation == Rot4.North)
            {
                thingVector3.y -= Offsets.YOffset_CarriedThing * 2;
            }

            thingVector3.y += compAnim.DrawOffsetY;
            float factor = pawn.GetBodysizeScaling();

            compAnim.DrawHands(Quaternion.identity, thingVector3, false, carriedThing, flip, factor);
        }

        public static void DirtyCache_Postfix(HediffSet __instance)
        {
            if(Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Pawn pawn = __instance.pawn; // Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if(pawn == null || !pawn.Spawned || pawn.Map == null)
            {
                return;
            }

            if(!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            if(!pawn.GetCompAnim().Deactivated)
            {
                pawn.Drawer.renderer.graphics.nakedGraphic = null;
                PortraitsCache.SetDirty(pawn);
            }

            pawn.GetCompAnim()?.PawnBodyGraphic?.Initialize();
        }

        public static void DoAttackAnimationOffsetsWeapons(
            Pawn pawn, ref float weaponAngle,
            ref Vector3 weaponPosition,
            bool flipped, CompBodyAnimator animator,
            out bool noTween)
        {
            CompEquippable primaryEq = pawn.equipment?.PrimaryEq;
            noTween = false;

            if(primaryEq?.parent?.def == null)
            {
                return;
            }

            Stance_Busy busy = pawn.stances.curStance as Stance_Busy;
            if(busy == null)
            {
                return;
            }

            if(busy.verb == null || !busy.verb.IsMeleeAttack)
            {
                return;
            }

            DamageDef
            damageDef = busy.verb.GetDamageDef(); // ThingUtility.PrimaryMeleeWeaponDamageType(primaryEq.parent.def);
            if(damageDef == null)
            {
                return;
            }

            // total weapon angle change during animation sequence
            int totalSwingAngle = 0;
            Vector3 currentOffset = animator.Jitterer.CurrentOffset;

            float jitterMax = animator.JitterMax;
            float magnitude = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;

            if(damageDef == DamageDefOf.Stab)
            {
                weaponPosition += currentOffset;

                // + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if(damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle = 120;
                weaponPosition += currentOffset + new Vector3(0, 0, Mathf.Sin(magnitude * Mathf.PI / jitterMax) / 10);
                weaponAngle += flipped
                               ? -animationPhasePercent * totalSwingAngle
                               : animationPhasePercent * totalSwingAngle;
                noTween = true;
            }
        }

        // private static float RecoilMax = -0.15f;
        // private static  Vector3 curOffset = new Vector3(0f, 0f, 0f);
        // public static void AddOffset(float dist, float dir)
        // {
        // curOffset += Quaternion.AngleAxis(dir, Vector3.up) * Vector3.forward * dist;
        // if (curOffset.sqrMagnitude > RecoilMax        * RecoilMax)
        // {
        // curOffset *= RecoilMax / curOffset.magnitude;
        // }
        // }
        public static void DoWeaponOffsets(
            Pawn pawn, 
            Thing eq, 
            ref Vector3 drawLoc, 
            ref float weaponAngle,
            ref Mesh weaponMesh)
        {
            CompProperties_WeaponExtensions extensions = eq.def.GetCompProperties<CompProperties_WeaponExtensions>();

            bool flipped = weaponMesh == MeshPool.plane10Flip;

            if((pawn == null) || (!pawn.GetCompAnim(out CompBodyAnimator animator)) || (extensions == null))
            {
                return;
            }
            
            float sizeMod = 1f;
            
            if(Find.TickManager.TicksGame == animator.LastPosUpdate[(int)equipment] || AnimatorIsOpen() && MainTabWindow_WalkAnimator.Pawn != pawn)
            {
                drawLoc = animator.LastPosition[(int)equipment];
                weaponAngle = animator.LastWeaponAngle;
            }
            else
            {
                animator.LastPosUpdate[(int)equipment] = Find.TickManager.TicksGame;

                CalculatePositionsWeapon(pawn,
                                         ref weaponAngle,
                                         extensions,
                                         out Vector3 weaponPosOffset,
                                         out bool aiming,
                                         flipped);

                // weapon angle and position offsets based on current attack keyframes sequence
                DoAttackAnimationOffsetsWeapons(
                    pawn, 
                    ref weaponAngle, 
                    ref weaponPosOffset, 
                    flipped, 
                    animator,
                    out bool noTween);

                drawLoc += weaponPosOffset * sizeMod;
                Vector3Tween eqTween = animator.Vector3Tweens[(int)equipment];

                if(pawn.pather.MovedRecently(5))
                {
                    noTween = true;
                }

                switch(eqTween.State)
                {
                    case TweenState.Running:
                        if(noTween || animator.IsMoving)
                        {
                            eqTween.Stop(StopBehavior.ForceComplete);
                        }

                        drawLoc = eqTween.CurrentValue;
                        break;

                    case TweenState.Paused:
                        break;

                    case TweenState.Stopped:
                        if(noTween || (animator.IsMoving))
                        {
                            break;
                        }

                        ScaleFunc scaleFunc = ScaleFuncs.SineEaseOut;

                        Vector3 start = animator.LastPosition[(int)equipment];
                        float distance = Vector3.Distance(start, drawLoc);
                        float duration = Mathf.Abs(distance * 50f);
                        if(start != Vector3.zero && duration > 12f)
                        {
                            start.y = drawLoc.y;
                            eqTween.Start(start, drawLoc, duration, scaleFunc);
                            drawLoc = start;
                        }

                        break;
                }

                // // fix the reset to default pos is target is changing
                // bool isAimAngle = (Math.Abs(aimAngle - angleStanding) <= 0.1f);
                // bool isAimAngleFlipped = (Math.Abs(aimAngle - angleStandingFlipped) <= 0.1f);
                // if (aiming && (isAimAngle || isAimAngleFlipped))
                // {
                // // use the last known position to avoid 1 frame flipping when target changes
                // drawLoc = animator.lastPosition[(int)equipment];
                // weaponAngle = animator.lastWeaponAngle;
                // }
                {
                    // else
                    animator.LastPosition[(int)equipment] = drawLoc;
                    animator.LastWeaponAngle = weaponAngle;
                    animator.MeshFlipped = flipped;
                }
            }

            // Now the remaining hands if possible
            if(animator.Props.bipedWithHands && Controller.settings.UseHands)
            {
                SetPositionsForHandsOnWeapons(
                    drawLoc,
                    flipped,
                    weaponAngle,
                    extensions, animator, sizeMod);
            }
        }



        public static void DrawEquipmentAiming_Prefix(
            PawnRenderer __instance, 
            Thing eq, 
            Vector3 drawLoc,
            ref float aimAngle)
        {
            Pawn pawn = __instance.graphics.pawn;

            // Flip the angle for north
            if(pawn.Rotation == Rot4.North && aimAngle == angleStanding)
            {
                aimAngle = angleStandingFlipped;
            }

            if(!pawn.GetCompAnim(out CompBodyAnimator animator))
            {
                return;
            }

            if(Find.TickManager.TicksGame == animator.LastAngleTick)
            {
                aimAngle = animator.LastAimAngle;
                return;
            }

            animator.LastAngleTick = Find.TickManager.TicksGame;

            float angleChange;

            float startAngle = animator.LastAimAngle;
            float endAngle = aimAngle;

            FloatTween tween = animator.AimAngleTween;
            switch(tween.State)
            {
                case TweenState.Running:
                    startAngle = tween.EndValue;
                    endAngle = aimAngle;
                    aimAngle = tween.CurrentValue;
                    break;
            }

            angleChange = CalcShortestRot(startAngle, endAngle);
            if(Mathf.Abs(angleChange) > 6f)
            {
                // no tween for flipping
                bool x = Mathf.Abs(animator.LastAimAngle - angleStanding) < 3f &&
                         Mathf.Abs(aimAngle - angleStandingFlipped) < 3f;
                bool y = Mathf.Abs(animator.LastAimAngle - angleStandingFlipped) < 3f &&
                         Mathf.Abs(aimAngle - angleStanding) < 3f;
                bool z = Math.Abs(Mathf.Abs(aimAngle - animator.LastAimAngle) - 180f) < 12f;

                if(!x && !y && !z)
                {
                    tween.Start(startAngle, startAngle + angleChange, Mathf.Abs(angleChange),
                                ScaleFuncs.QuinticEaseOut);
                    aimAngle = startAngle;
                }
            }

            animator.LastAimAngle = aimAngle;
        }

        public static bool DrawHeadHair_Prefix(PawnRenderer __instance)
        {
            if (__instance.graphics.pawn.GetCompFace() != null && !__instance.graphics.pawn.IsChild())
            {
                return false;
            }

            return true;
        }
        public static IEnumerable<CodeInstruction> DrawEquipmentAiming_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGen)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            int index = instructionList.FindIndex(x => x.opcode == OpCodes.Ldloc_0);
            List<Label> labels = instructionList[index].labels;
            instructionList[index].labels = new List<Label>();
            instructionList.InsertRange(index, new List<CodeInstruction>
            {
                // DoCalculations(Pawn pawn, Thing eq, ref Vector3 drawLoc, ref float weaponAngle, float aimAngle)
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")), // pawn
                new CodeInstruction(OpCodes.Ldarg_1), // Thing
                new CodeInstruction(OpCodes.Ldarga, 2), // drawLoc
                new CodeInstruction(OpCodes.Ldloca_S, 1), // weaponAngle
                // new CodeInstruction(OpCodes.Ldarg_3), // aimAngle
                new CodeInstruction(OpCodes.Ldloca_S, 0), // Mesh, loaded as ref to not trigger I Love Big Guns
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchesFS), nameof(DoWeaponOffsets))),
            });
            instructionList[index].labels = labels;
            return instructionList;
        }

        public static bool IsChild(this Pawn pawn)
        {
            return 
                LoadedModManager.RunningModsListForReading.Any(x => x.PackageId == "dylan.csl") &&
                pawn.RaceProps.Humanlike && pawn.ageTracker.CurLifeStage.bodySizeFactor < 1f;
        }

        // From CSL mod
        public static float GetBodysizeScaling(this Pawn pawn)
        {
            float bodySizeFactor = pawn.ageTracker.CurLifeStage.bodySizeFactor;
            float num = bodySizeFactor;
            float num2 = 1f;
            try
            {
                int curLifeStageIndex = pawn.ageTracker.CurLifeStageIndex;
                int num3 = pawn.RaceProps.lifeStageAges.Count - 1;
                LifeStageAge val = pawn.RaceProps.lifeStageAges[curLifeStageIndex];
                if (num3 == curLifeStageIndex && curLifeStageIndex != 0 && bodySizeFactor != 1f)
                {
                    LifeStageAge val2 = pawn.RaceProps.lifeStageAges[curLifeStageIndex - 1];
                    num = val2.def.bodySizeFactor + (float)Math.Round((val.def.bodySizeFactor - val2.def.bodySizeFactor) / (val.minAge - val2.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val2.minAge), 2);
                }
                else if (num3 == curLifeStageIndex)
                {
                    num = bodySizeFactor;
                }
                else if (curLifeStageIndex == 0)
                {
                    LifeStageAge val3 = pawn.RaceProps.lifeStageAges[curLifeStageIndex + 1];
                    num = val.def.bodySizeFactor + (float)Math.Round((val3.def.bodySizeFactor - val.def.bodySizeFactor) / (val3.minAge - val.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val.minAge), 2);
                }
                else
                {
                    LifeStageAge val3 = pawn.RaceProps.lifeStageAges[curLifeStageIndex + 1];
                    num = val.def.bodySizeFactor + (float)Math.Round((val3.def.bodySizeFactor - val.def.bodySizeFactor) / (val3.minAge - val.minAge) * (pawn.ageTracker.AgeBiologicalYearsFloat - val.minAge), 2);
                }

                if (pawn.RaceProps.baseBodySize > 0f)
                {
                    num2 = pawn.RaceProps.baseBodySize;
                }
            }
            catch
            {
            }

            return num * num2;
        }

        public static IEnumerable<CodeInstruction> DrawCarriedThing_Transpiler(
            IEnumerable<CodeInstruction> instructions, 
            ILGenerator ilGen)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo drawAtMethod = AccessTools.Method(typeof(Thing), nameof(Thing.DrawAt));

            int indexDrawAt = instructionList.FindIndex(x => x.opcode == OpCodes.Callvirt && (MethodInfo)x.operand == drawAtMethod);

            instructionList.RemoveAt(indexDrawAt);
            instructionList.InsertRange(indexDrawAt, new List<CodeInstruction>
            {
                // carriedThing.DrawAt(vector, flip);
                // carriedThing = ldloc.1
                // vector = ldloc.2
                // bool flip = ldloc.s 4
                new CodeInstruction(OpCodes.Ldarg_0), // this.PawnRenderer
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")), // pawn
                new CodeInstruction(OpCodes.Ldloc_3), // flag
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchesFS), nameof(CheckAndDrawHands)))
            });
            return instructionList;
        }

        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            // Unity engine crashes when trying to load mesh while loading. This also includes DefsLoaded in ModBase implementation.
            // RenderParamManager.ReadFromRenderDefs() loads meshes so it should be called here instead.
            if(!RenderParamManager.Initialized)
			{
                RenderParamManager.ReadFromRenderDefs();
            }

            Pawn pawn = __instance.pawn;
            if(pawn == null)
            {
                return;
            }
            
            pawn.GetCompAnim()?.PawnBodyGraphic?.Initialize();
                        
            __instance.ClearCache();
            pawn.GetComp<CompBodyAnimator>()?.ClearCache();
            
            pawn.GetComp<CompFace>()?.OnResolveGraphics(__instance);
        }

        public static void ResolveApparelGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            pawn.GetComp<CompFace>()?.OnResolveApparelGraphics(__instance);
        }

        public static void SetPositionsForHandsOnWeapons(
            Vector3 weaponPosition, 
            bool flipped, 
            float weaponAngle,
            CompProperties_WeaponExtensions compWeaponExtensions,
            CompBodyAnimator animator, 
            float sizeMod)
        {
            // Prepare everything for DrawHands, but don't draw
            if(compWeaponExtensions == null)
            {
                return;
            }

            animator.FirstHandPosition = compWeaponExtensions.RightHandPosition;
            animator.SecondHandPosition = compWeaponExtensions.LeftHandPosition;

            // Only put the second hand on when aiming or not moving => free left hand for running
            // bool leftOnWeapon = true;// aiming || !animator.IsMoving;
            if(animator.FirstHandPosition != Vector3.zero)
            {
                float x = animator.FirstHandPosition.x;
                float y = animator.FirstHandPosition.y;
                float z = animator.FirstHandPosition.z;
                if(flipped)
                {
                    x *= -1f;
                    y *= -1f;
                }

                // if (pawn.Rotation == Rot4.North)
                // {
                // y *= -1f;
                // }
                x *= sizeMod;
                z *= sizeMod;
                animator.FirstHandPosition =
                weaponPosition + new Vector3(x, y, z).RotatedBy(weaponAngle);
            }

            if(animator.HasLeftHandPosition)
            {
                float x2 = animator.SecondHandPosition.x;
                float y2 = animator.SecondHandPosition.y;
                float z2 = animator.SecondHandPosition.z;
                if(flipped)
                {
                    x2 *= -1f;
                    y2 *= -1f;
                }

                x2 *= sizeMod;
                z2 *= sizeMod;

                // if (pawn.Rotation == Rot4.North)
                // {
                // y2 *= -1f;
                // }
                animator.SecondHandPosition =
                weaponPosition + new Vector3(x2, y2, z2).RotatedBy(weaponAngle);
            }

            // Swap left and right hand position when flipped
            animator.WeaponQuat = Quaternion.AngleAxis(weaponAngle, Vector3.up);
        }
        
        // If the return value is positive, then rotate to the left. Else,
        // rotate to the right.
        private static float CalcShortestRot(float from, float to)
        {
            // If from or to is a negative, we have to recalculate them.
            // For an example, if from = -45 then from(-45) + 360 = 315.
            if(from < 0)
            {
                from += 360;
            }

            if(to < 0)
            {
                to += 360;
            }

            // Do not rotate if from == to.
            if(from == to ||
                from == 0 && to == 360 ||
                from == 360 && to == 0)
            {
                return 0;
            }

            // Pre-calculate left and right.
            float left = (360 - from) + to;
            float right = from - to;

            // If from < to, re-calculate left and right.
            if(from < to)
            {
                if(to > 0)
                {
                    left = to - from;
                    right = (360 - to) + from;
                }
                else
                {
                    left = (360 - to) + from;
                    right = to - from;
                }
            }

            // Determine the shortest direction.
            return ((left <= right) ? left : (right * -1));
        }
        
        private static void CalculatePositionsWeapon(
            Pawn pawn, 
            ref float weaponAngle,
            CompProperties_WeaponExtensions extensions,
            out Vector3 weaponPosOffset, 
            out bool aiming,
            bool flipped)
        {
            weaponPosOffset = Vector3.zero;
            if(pawn.Rotation == Rot4.West || pawn.Rotation == Rot4.North)
            {
                weaponPosOffset.y = -Offsets.YOffset_Head - Offsets.YOffset_CarriedThing;
            }

            // Use y for the horizontal position. too lazy to add even more vectors
            bool isHorizontal = pawn.Rotation.IsHorizontal;
            aiming = pawn.Aiming();
            Vector3 extOffset;
            Vector3 o = extensions.WeaponPositionOffset;
            Vector3 d = extensions.AimedWeaponPositionOffset;
            if(isHorizontal)
            {
                extOffset = new Vector3(o.y, 0, o.z);
                if(aiming)
                {
                    extOffset += new Vector3(d.y, 0, d.z);
                }
            }
            else
            {
                extOffset = new Vector3(o.x, 0, o.z);
                if(aiming)
                {
                    extOffset += new Vector3(d.x, 0, d.z);
                }
            }

            if(flipped)
            {
                if(aiming)
                {
                    weaponAngle -= extensions?.AttackAngleOffset ?? 0;
                }

                weaponPosOffset += extOffset;

                // flip x position offset
                if(pawn.Rotation != Rot4.South)
                {
                    weaponPosOffset.x *= -1;
                }
            }
            else
            {
                if(aiming)
                {
                    weaponAngle += extensions?.AttackAngleOffset ?? 0;
                }

                weaponPosOffset += extOffset;
            }
        }

        #endregion Public Methods
    }
}