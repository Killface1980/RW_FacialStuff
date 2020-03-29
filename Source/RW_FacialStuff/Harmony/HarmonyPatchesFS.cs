// ReSharper disable All

using FacialStuff.HairCut;
using FacialStuff.Tweener;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Reflection.Emit;
using FacialStuff.Defs;
using Verse.AI;

namespace FacialStuff.Harmony
{
    using AnimatorWindows;
    using FaceEditor;
    using Genetics;
    using GraphicsFS;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using Utilities;
    using Verse;
    using Verse.Sound;

    [StaticConstructorOnStartup]
    public static class HarmonyPatchesFS
    {
        public const TweenThing equipment = TweenThing.Equipment;

        #region Public Constructors

        static HarmonyPatchesFS()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("rimworld.facialstuff.mod");
            // HarmonyLib.Harmony.DEBUG = true;
            harmony.PatchAll(Assembly.GetExecutingAssembly());


               harmony.Patch(
                             AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DrawPortraitArea"),
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(AddFaceEditButton)));
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(ResolveAllGraphics_Postfix)));
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveApparelGraphics)),
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(ResolveApparelGraphics_Postfix)));
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming)),
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DrawEquipmentAiming_Prefix)),
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS),
                                               nameof(DrawEquipmentAiming_Transpiler)));
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt),
                                                new[] { typeof(Vector3), typeof(RotDrawMode), typeof(bool), typeof(bool) }),
            
                             // new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(HarmonyPatchesFS.RenderPawnAt)),
                             null,
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(RenderPawnAt_Transpiler))
                            );
            
               harmony.Patch(
                             AccessTools.Method(typeof(HediffSet), nameof(HediffSet.DirtyCache)),
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DirtyCache_Postfix)));
            
               harmony.Patch(
                             AccessTools.Method(typeof(GraphicDatabaseHeadRecords),
                                                nameof(GraphicDatabaseHeadRecords.Reset)),
                             null,
                             new HarmonyMethod(
                                               typeof(GraphicDatabaseHeadRecordsModded),
                                               nameof(GraphicDatabaseHeadRecordsModded.Reset)));
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)),
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(RandomHairDefFor_PreFix)),
                             null);
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnSkinColors), "GetSkinDataIndexOfMelanin"),
                             new HarmonyMethod(
                                               typeof(PawnSkinColors_FS),
                                               nameof(PawnSkinColors_FS.GetSkinDataIndexOfMelanin_Prefix)),
                             null);
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetSkinColor)),
                             new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.GetSkinColor_Prefix)),
                             null);
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.RandomMelanin)),
                             new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.RandomMelanin_Prefix)),
                             null);
            
               harmony.Patch(
                             AccessTools.Method(typeof(PawnSkinColors),
                                                nameof(PawnSkinColors.GetMelaninCommonalityFactor)),
                             new HarmonyMethod(
                                               typeof(PawnSkinColors_FS),
                                               nameof(PawnSkinColors_FS.GetMelaninCommonalityFactor_Prefix)),
                             null);
            
               harmony.Patch(
                             AccessTools.Method(typeof(Pawn_InteractionsTracker),
                                                nameof(Pawn_InteractionsTracker.TryInteractWith)),
                             null,
                             new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(TryInteractWith_Postfix)));
            

            Log.Message(
                        "Facial Stuff successfully completed " + harmony.GetPatchedMethods().Count()
                                                               + " patches with harmony.");

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(
                                                                                       td => td.category ==
                                                                                             ThingCategory.Pawn &&
                                                                                             td.race.Humanlike))
            {
                if (def.inspectorTabs == null || def.inspectorTabs.Count == 0)
                {
                    def.inspectorTabs = new List<Type>();
                    def.inspectorTabsResolved = new List<InspectTabBase>();
                }

                if (def.inspectorTabs.Contains(typeof(ITab_Pawn_Weapons)))
                {
                    return;
                }

                def.inspectorTabs.Add(typeof(ITab_Pawn_Weapons));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Weapons)));

                def.inspectorTabs.Add(typeof(ITab_Pawn_Face));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Face)));
            }

            List<HairDef> beardyHairList =
                DefDatabase<HairDef>.AllDefsListForReading.Where(x => x.IsBeardNotHair()).ToList();
            for (int i = 0; i < beardyHairList.Count(); i++)
            {
                HairDef beardy = beardyHairList[i];
                if (beardy.label.Contains("shaven")) continue;
                BeardDef beardDef = new BeardDef 
                {
                    defName = beardy.defName,
                    label = "_VHE_" + beardy.label,
                    hairGender = beardy.hairGender,
                    texPath = beardy.texPath.Replace("Things/Pawn/Humanlike/Beards/", ""),
                    hairTags = beardy.hairTags,
                    beardType = BeardType.FullBeard
                };
                if (beardDef.label.Contains("stubble") || beardDef.label.Contains("goatee") || beardDef.label.Contains("lincoln"))
                {
                    beardDef.drawMouth = true;
                }
                DefDatabase<BeardDef>.Add(beardDef);

            }
            Dialog_FaceStyling.FullBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType == BeardType.FullBeard)
                .ToList();
            Dialog_FaceStyling.LowerBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType != BeardType.FullBeard)
                .ToList();
            Dialog_FaceStyling.MoustacheDefs = DefDatabase<MoustacheDef>.AllDefsListForReading;

            CheckAllInjected();
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

        #region Public Fields

        public static List<Thing> PlantMoved = new List<Thing>();

        public static bool Plants;

        public static float Steps;

        #endregion Public Fields

        #region Private Fields

        private static readonly FieldInfo pawnField = AccessTools.Field(typeof(Pawn_EquipmentTracker), "pawn");

        private static Graphic_Shadow _shadowGraphic;
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
        private static bool DrawAtGiddy(Pawn __instance)
        {
            //  ExtendedDataStorage extendedDataStorage = GiddyUpCore.Base.Instance.GetExtendedDataStorage();
            //  bool                flag                = extendedDataStorage == null;
            //  if (!flag)
            //  {
            //      ExtendedPawnData extendedDataFor = extendedDataStorage.GetExtendedDataFor(__instance);
            //      if (extendedDataFor != null&&extendedDataFor.mount != null)
            //      {
            //
            //      }
            //          CompBodyAnimator animator = extendedDataFor.mount.GetCompAnim();
            //          if (animator != null)
            //          {
            //              animator.IsRider = flag2;
            //              Debug.Log("FS: Pawn "+ __instance +" is rider?" + animator.IsRider);
            //
            //      }
            //  }

            return true;
        }

        #endregion Private Fields

        #region Public Methods

        //   public static void StartPath_Postfix(Pawn_PathFollower __instance)
        //   {
        //       Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_PathFollower), "pawn").GetValue(__instance);
        //       if (pawn.GetCompAnim(out CompBodyAnimator animator))
        //       {
        //           animator.IsMoving = true;
        //       }
        //   }
        //
        //   public static void PatherArrived_Postfix(Pawn_PathFollower __instance)
        //   {
        //       Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_PathFollower), "pawn").GetValue(__instance);
        //       if (pawn.GetCompAnim(out CompBodyAnimator animator))
        //       {
        //           animator.IsMoving = false;
        //       }
        //   }

        public static void AddFaceEditButton(Page_ConfigureStartingPawns __instance, Rect rect)
        {
            FieldInfo PawnFieldInfo =
            typeof(Page_ConfigureStartingPawns).GetField("curPawn", BindingFlags.NonPublic | BindingFlags.Instance);

            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            // Shitty Transpiler, doin' it on my own
            Rect rect2 = new Rect(rect.x + 500f, rect.y, 25f, 25f);
            if (rect2.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.cyan;

                // GUI.color = new Color(0.97647f, 0.97647f, 0.97647f);
            }
            else
            {
                GUI.color = new Color(0.623529f, 0.623529f, 0.623529f);
            }

            GUI.DrawTexture(rect2, ContentFinder<Texture2D>.Get("Buttons/ButtonFace", true));
            GUI.color = Color.white;
            string tip = "FacialStuffEditor.EditFace".Translate();
            TooltipHandler.TipRegion(rect2, tip);

            // ReSharper disable once InvertIf
            if (Widgets.ButtonInvisible(rect2, false))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                OpenStylingWindow(pawn);
            }
        }

        public static void CheckAndDrawHands(Thing carriedThing, Vector3 thingVector3, bool flip, Pawn pawn, bool thingBehind)
        {
            if (pawn.RaceProps.Animal)
            {
                carriedThing.DrawAt(thingVector3, flip);
                return;
            }

            if (!pawn.GetCompAnim(out CompBodyAnimator compAnim))
            {
                carriedThing.DrawAt(thingVector3, flip);
                return;
            }

            bool showHands = compAnim.Props.bipedWithHands && Controller.settings.UseHands;
            if (!showHands)
            {
                carriedThing.DrawAt(thingVector3, flip);
                return;
            }

            // Modify the drawPos to appear behind a pawn if facing North, in case vanilla didn't
            if (!thingBehind && pawn.Rotation == Rot4.North)
            {
                thingVector3.y -= Offsets.YOffset_CarriedThing * 2;
            }

            thingVector3.y += compAnim.DrawOffsetY;
            float factor = pawn.GetCompAnim().PawnBodyGraphic.Factor;

            compAnim.DrawHands(Quaternion.identity, thingVector3, false, carriedThing, flip, factor);
        }

        public static void DirtyCache_Postfix(HediffSet __instance)
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Pawn pawn = __instance.pawn; // Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (pawn == null || !pawn.Spawned || pawn.Map == null)
            {
                return;
            }

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            if (!pawn.GetCompAnim().Deactivated && pawn.CheckForAddedOrMissingParts())
            {
                pawn.Drawer.renderer.graphics.nakedGraphic = null;
                PortraitsCache.SetDirty(pawn);
            }

            pawn.GetCompAnim()?.PawnBodyGraphic?.Initialize();
        }

        public static void DoAttackAnimationOffsetsWeapons(Pawn pawn, ref float weaponAngle,
                                                           ref Vector3 weaponPosition,
                                                           bool flipped, CompBodyAnimator animator,
                                                           out bool noTween)
        {
            CompEquippable primaryEq = pawn.equipment?.PrimaryEq;
            noTween = false;

            // DamageDef damageDef = primaryEq?.PrimaryVerb?.verbProps?.meleeDamageDef;
            if (primaryEq?.parent?.def == null)
            {
                return;
            }

            Stance_Busy busy = pawn.stances.curStance as Stance_Busy;
            if (busy == null)
            {
                return;
            }

            if (busy.verb == null || !busy.verb.IsMeleeAttack)
            {
                return;
            }

            DamageDef
            damageDef = busy.verb.GetDamageDef(); //ThingUtility.PrimaryMeleeWeaponDamageType(primaryEq.parent.def);
            if (damageDef == null)
            {
                return;
            }

            // total weapon angle change during animation sequence
            int totalSwingAngle = 0;
            Vector3 currentOffset = animator.Jitterer.CurrentOffset;

            float jitterMax = animator.JitterMax;
            float magnitude = currentOffset.magnitude;
            float animationPhasePercent = magnitude / jitterMax;

            if (damageDef == DamageDefOf.Stab)
            {
                weaponPosition += currentOffset;

                // + new Vector3(0, 0, Mathf.Pow(this.CompFace.Jitterer.CurrentOffset.magnitude, 0.25f))/2;
            }
            else if (damageDef == DamageDefOf.Blunt || damageDef == DamageDefOf.Cut)
            {
                totalSwingAngle = 120;
                weaponPosition += currentOffset + new Vector3(0, 0, Mathf.Sin(magnitude * Mathf.PI / jitterMax) / 10);
                weaponAngle += flipped
                               ? -animationPhasePercent * totalSwingAngle
                               : animationPhasePercent * totalSwingAngle;
                noTween = true;
            }
        }

        //  private static float RecoilMax = -0.15f;
        //  private static  Vector3 curOffset = new Vector3(0f, 0f, 0f);
        //  public static void AddOffset(float dist, float dir)
        //  {
        //      curOffset += Quaternion.AngleAxis(dir, Vector3.up) * Vector3.forward * dist;
        //      if (curOffset.sqrMagnitude > RecoilMax        * RecoilMax)
        //      {
        //          curOffset *= RecoilMax / curOffset.magnitude;
        //      }
        //  }
        public static void DoWeaponOffsets(Pawn pawn, Thing eq, ref Vector3 drawLoc, ref float weaponAngle,
                                           ref Mesh weaponMesh)
        {
            CompProperties_WeaponExtensions extensions = eq.def.GetCompProperties<CompProperties_WeaponExtensions>();

            bool flipped = weaponMesh == MeshPool.plane10Flip;

            if ((pawn == null) || (!pawn.GetCompAnim(out CompBodyAnimator animator)) || (extensions == null))
            {
                return;
            }

            float sizeMod = 1f;

            //  if (Controller.settings.IReallyLikeBigGuns) { sizeMod = 2.0f; }
            //     else if (Controller.settings.ILikeBigGuns)
            //  {
            //      sizeMod = 1.4f;
            //  }
            //  else
            //  {
            //      sizeMod = 1f;
            //  }

            if (Find.TickManager.TicksGame == animator.LastPosUpdate[(int)equipment] || AnimatorIsOpen() && MainTabWindow_WalkAnimator.Pawn != pawn)
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

                DoAttackAnimationOffsetsWeapons(pawn, ref weaponAngle, ref weaponPosOffset, flipped, animator,
                                                out bool noTween);

                drawLoc += weaponPosOffset * sizeMod;

                Vector3Tween eqTween = animator.Vector3Tweens[(int)equipment];

                if (pawn.pather.MovedRecently(5))
                {
                    noTween = true;
                }

                switch (eqTween.State)
                {
                    case TweenState.Running:
                        if (noTween || animator.IsMoving)
                        {
                            eqTween.Stop(StopBehavior.ForceComplete);
                        }

                        drawLoc = eqTween.CurrentValue;
                        break;

                    case TweenState.Paused:
                        break;

                    case TweenState.Stopped:
                        if (noTween || (animator.IsMoving))
                        {
                            break;
                        }

                        ScaleFunc scaleFunc = ScaleFuncs.SineEaseOut;

                        Vector3 start = animator.LastPosition[(int)equipment];
                        float distance = Vector3.Distance(start, drawLoc);
                        float duration = Mathf.Abs(distance * 50f);
                        if (start != Vector3.zero && duration > 12f)
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
                //
                // if (aiming && (isAimAngle || isAimAngleFlipped))
                // {
                //     // use the last known position to avoid 1 frame flipping when target changes
                //     drawLoc = animator.lastPosition[(int)equipment];
                //     weaponAngle = animator.lastWeaponAngle;
                // }
                // else
                {
                    animator.LastPosition[(int)equipment] = drawLoc;
                    animator.LastWeaponAngle = weaponAngle;
                    animator.MeshFlipped = flipped;
                }
            }

            // Now the remaining hands if possible
            if (animator.Props.bipedWithHands && Controller.settings.UseHands)
            {
                SetPositionsForHandsOnWeapons(
                                              drawLoc,
                                              flipped,
                                              weaponAngle,
                                              extensions, animator, sizeMod);
            }
        }



        public static void DrawEquipmentAiming_Prefix(PawnRenderer __instance, Thing eq, Vector3 drawLoc,
                                                      ref float aimAngle)
        {
            Pawn pawn = __instance.graphics.pawn;

            // Flip the angle for north

            if (pawn.Rotation == Rot4.North && aimAngle == angleStanding)
            {
                aimAngle = angleStandingFlipped;
            }

            if (!pawn.GetCompAnim(out CompBodyAnimator animator))
            {
                return;
            }

            if (Find.TickManager.TicksGame == animator.LastAngleTick)
            {
                aimAngle = animator.LastAimAngle;
                return;
            }

            animator.LastAngleTick = Find.TickManager.TicksGame;

            float angleChange;

            float startAngle = animator.LastAimAngle;
            float endAngle = aimAngle;

            FloatTween tween = animator.AimAngleTween;
            switch (tween.State)
            {
                case TweenState.Running:
                    startAngle = tween.EndValue;
                    endAngle = aimAngle;
                    aimAngle = tween.CurrentValue;
                    break;
            }

            angleChange = CalcShortestRot(startAngle, endAngle);
            if (Mathf.Abs(angleChange) > 6f)
            {
                // no tween for flipping
                bool x = Mathf.Abs(animator.LastAimAngle - angleStanding) < 3f &&
                         Mathf.Abs(aimAngle - angleStandingFlipped) < 3f;
                bool y = Mathf.Abs(animator.LastAimAngle - angleStandingFlipped) < 3f &&
                         Mathf.Abs(aimAngle - angleStanding) < 3f;
                bool z = Math.Abs(Mathf.Abs(aimAngle - animator.LastAimAngle) - 180f) < 12f;

                if (!x && !y && !z)
                {
                    //     if (Math.Abs(aimAngleTween.EndValue - weaponAngle) > 6f)

                    tween.Start(startAngle, startAngle + angleChange, Mathf.Abs(angleChange),
                                ScaleFuncs.QuinticEaseOut);
                    aimAngle = startAngle;
                }
            }

            animator.LastAimAngle = aimAngle;
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
                                               new CodeInstruction(OpCodes.Ldfld,
                                                                   AccessTools.Field(typeof(PawnRenderer),
                                                                                     "pawn")), // pawn
                                               new CodeInstruction(OpCodes.Ldarg_1),           // Thing
                                               new CodeInstruction(OpCodes.Ldarga,   2),       // drawLoc
                                               new CodeInstruction(OpCodes.Ldloca_S, 1),       // weaponAngle
                                               //   new CodeInstruction(OpCodes.Ldarg_3), // aimAngle
                                               new CodeInstruction(OpCodes.Ldloca_S,
                                                                   0), // Mesh, loaded as ref to not trigger I Love Big Guns
                                               new CodeInstruction(OpCodes.Call,
                                                                   AccessTools.Method(typeof(HarmonyPatchesFS),
                                                                                      nameof(DoWeaponOffsets))),
                                               });
            instructionList[index].labels = labels;
            return instructionList;
        }

        public static void OpenStylingWindow(Pawn pawn)
        {
            pawn.GetCompFace(out CompFace face);
            Find.WindowStack.Add(new Dialog_FaceStyling(face));
        }

        public static bool RandomHairDefFor_PreFix(Pawn pawn, FactionDef factionType, ref HairDef __result)
        {
            //  Log.Message("1 - " + pawn);
            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return true;
            }

            //   Log.Message("2 - " + pawn.def.defName);

            if (compFace.Props.useAlienRacesHairTags)
            {
                return true;
            }

            FactionDef faction = factionType;

            if (faction == null)
            {
                faction = FactionDefOf.PlayerColony;
            }

            List<string> hairTags = faction.hairTags;

            if (pawn.def == ThingDefOf.Human)
            {
                List<string> vanillatags = new List<string> { "Urban", "Rural", "Punk", "Tribal" };
                if (!hairTags.Any(x => vanillatags.Contains(x)))
                {
                    hairTags.AddRange(vanillatags);
                }
            }

            IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                          where hair.hairTags.SharesElementWith(hairTags) && !hair.IsBeardNotHair()
                                          select hair;

            __result = source.RandomElementByWeight(hair => PawnFaceMaker.HairChoiceLikelihoodFor(hair, pawn));
            return false;
        }

        public static IEnumerable<CodeInstruction> RenderPawnAt_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo drawAtMethod = AccessTools.Method(typeof(Thing), nameof(Thing.DrawAt));

            int indexDrawAt = instructionList.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand == drawAtMethod);

            instructionList.RemoveAt(indexDrawAt);
            instructionList.InsertRange(indexDrawAt, new List<CodeInstruction>
            {
                                                     // carriedThing.DrawAt(vector, flip);
                                                     // carriedThing = ldloc.1
                                                     // vector = ldloc.2
                                                     // bool flip = ldloc.s 4
                                                     new CodeInstruction(OpCodes.Ldarg_0), // this.PawnRenderer
                                                     new CodeInstruction(OpCodes.Ldfld,
                                                                         AccessTools.Field(typeof(PawnRenderer),
                                                                                           "pawn")), // pawn
                                                     new CodeInstruction(OpCodes.Ldloc_3),           // flag
                                                     new CodeInstruction(OpCodes.Call,
                                                                         AccessTools.Method(typeof(HarmonyPatchesFS),
                                                                                            nameof(CheckAndDrawHands)))
                                                     });
            return instructionList;
        }

        // [HarmonyAfter("net.pardeike.zombieland")]
        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn == null)
            {
                return;
            }

            pawn.CheckForAddedOrMissingParts();
            pawn.GetCompAnim()?.PawnBodyGraphic?.Initialize();

            // Check if race has face, else return
            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }
            
            // compFace.IsChild = pawn.ageTracker.AgeBiologicalYearsFloat < 14;

            // Return if child
            if (compFace.IsChild || pawn.GetCompAnim().Deactivated)
            {
                return;
            }

            __instance.ClearCache();
            pawn.GetComp<CompBodyAnimator>()?.ClearCache();

            GraphicDatabaseHeadRecordsModded.BuildDatabaseIfNecessary();

            // Need: get the traditional habitat of a faction => not suitable, as factions are scattered around the globe
            // if (!faceComp.IsSkinDNAoptimized)
            // {
            // faceComp.DefineSkinDNA();
            // }

            // Custom rotting color, mixed with skin tone
            Color rotColor = pawn.story.SkinColor * FaceTextures.SkinRottingMultiplyColor;
            if (!compFace.InitializeCompFace())
            {
                return;
            }

            __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, __instance.pawn.story.SkinColor);
            if (compFace.Props.needsBlankHumanHead)
            {
                if (!compFace.IsChild)
                {
                    __instance.headGraphic =
                        GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn,
                            pawn.story.SkinColor);
                    __instance.desiccatedHeadGraphic =
                        GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, rotColor);
                    __instance.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecordsModded.GetStump(rotColor);
                }
                else
                {
                    __instance.headGraphic =
                        GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath,
                            pawn.story.SkinColor);
                    __instance.desiccatedHeadGraphic =
                        GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath,
                            rotColor);
                    __instance.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecords.GetStump(rotColor);

                }
            }

            __instance.rottingGraphic =
           GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, rotColor);

            __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                                                                             pawn.story.hairDef.texPath,
                                                                             ShaderDatabase.Cutout,
                                                                             Vector2.one,
                                                                             pawn.story.hairColor);
            PortraitsCache.SetDirty(pawn);
        }

        public static void ResolveApparelGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            // Set up the hair cut graphic
            if (Controller.settings.MergeHair)
            {
                HairCutPawn hairPawn = CutHairDB.GetHairCache(pawn);

                List<Apparel> wornApparel = pawn.apparel.WornApparel
                                                .Where(x => x.def.apparel.LastLayer == ApparelLayerDefOf.Overhead).ToList();
                HeadCoverage coverage = HeadCoverage.None;
                if (!wornApparel.NullOrEmpty())
                {
                    if (wornApparel.Any(x => x.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)))
                    {
                        coverage = HeadCoverage.UpperHead;
                    }

                    if (wornApparel.Any(x => x.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)))
                    {
                        coverage = HeadCoverage.FullHead;
                    }
                }

                if (coverage != 0)
                {
                    hairPawn.HairCutGraphic = CutHairDB.Get<Graphic_Multi>(
                                                                                pawn.story.hairDef.texPath,
                                                                                ShaderDatabase.Cutout,
                                                                                Vector2.one,
                                                                                pawn.story.hairColor, coverage);
                }
            }
        }

        public static void SetPositionsForHandsOnWeapons(Vector3 weaponPosition, bool flipped, float weaponAngle,
                                                         [CanBeNull]
                                                         CompProperties_WeaponExtensions compWeaponExtensions,
                                                         CompBodyAnimator animator, float sizeMod)
        {
            // Prepare everything for DrawHands, but don't draw
            if (compWeaponExtensions == null)
            {
                return;
            }

            animator.FirstHandPosition = compWeaponExtensions.RightHandPosition;
            animator.SecondHandPosition = compWeaponExtensions.LeftHandPosition;

            // Only put the second hand on when aiming or not moving => free left hand for running
            //  bool leftOnWeapon = true;// aiming || !animator.IsMoving;

            if (animator.FirstHandPosition != Vector3.zero)
            {
                float x = animator.FirstHandPosition.x;
                float y = animator.FirstHandPosition.y;
                float z = animator.FirstHandPosition.z;
                if (flipped)
                {
                    x *= -1f;
                    y *= -1f;
                }

                //if (pawn.Rotation == Rot4.North)
                //{
                //    y *= -1f;
                //}
                x *= sizeMod;
                z *= sizeMod;
                animator.FirstHandPosition =
                weaponPosition + new Vector3(x, y, z).RotatedBy(weaponAngle);
            }

            if (animator.HasLeftHandPosition)
            {
                float x2 = animator.SecondHandPosition.x;
                float y2 = animator.SecondHandPosition.y;
                float z2 = animator.SecondHandPosition.z;
                if (flipped)
                {
                    x2 *= -1f;
                    y2 *= -1f;
                }

                x2 *= sizeMod;
                z2 *= sizeMod;

                //if (pawn.Rotation == Rot4.North)
                //{
                //    y2 *= -1f;
                //}

                animator.SecondHandPosition =
                weaponPosition + new Vector3(x2, y2, z2).RotatedBy(weaponAngle);
            }

            // Swap left and right hand position when flipped

            animator.WeaponQuat = Quaternion.AngleAxis(weaponAngle, Vector3.up);
        }

        public static void TryInteractWith_Postfix(Pawn_InteractionsTracker __instance, bool __result, Pawn recipient)
        {
            if (__instance == null)
            {
                return;
            }

            // FieldInfo PawnFieldInfo =
            // typeof(Pawn_InteractionsTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            //PawnFieldInfo?.GetValue(__instance);

            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn").GetValue(__instance);
            if (pawn == null || recipient == null)
            {
                return;
            }

            if (__result)
            {
                if (pawn.GetCompFace(out CompFace compFace))
                {
                    if (compFace.Props.canRotateHead)
                    {
                        if (compFace.HeadRotator != null && pawn.CanSee(recipient))
                        {
                            compFace.HeadRotator.LookAtPawn(recipient);
                        }
                    }
                }

                if (recipient.GetCompFace(out CompFace recipientFace))
                {
                    if (recipientFace.Props.canRotateHead && recipient.CanSee(pawn))
                    {
                        if (recipientFace.HeadRotator != null)
                        {
                            recipientFace.HeadRotator.LookAtPawn(pawn);
                        }
                    }
                }
            }
        }

        // If the return value is positive, then rotate to the left. Else,
        // rotate to the right.
        private static float CalcShortestRot(float from, float to)
        {
            // If from or to is a negative, we have to recalculate them.
            // For an example, if from = -45 then from(-45) + 360 = 315.
            if (@from < 0)
            {
                @from += 360;
            }

            if (to < 0)
            {
                to += 360;
            }

            // Do not rotate if from == to.
            if (@from == to ||
                @from == 0 && to == 360 ||
                @from == 360 && to == 0)
            {
                return 0;
            }

            // Pre-calculate left and right.
            float left = (360 - @from) + to;
            float right = @from - to;

            // If from < to, re-calculate left and right.
            if (@from < to)
            {
                if (to > 0)
                {
                    left = to - @from;
                    right = (360 - to) + @from;
                }
                else
                {
                    left = (360 - to) + @from;
                    right = to - @from;
                }
            }

            // Determine the shortest direction.
            return ((left <= right) ? left : (right * -1));
        }

        // Call CalcShortestRot and check its return value.
        // If CalcShortestRot returns a positive value, then this function
        // will return true for left. Else, false for right.
        private static bool CalcShortestRotDirection(float from, float to)
        {
            // If the value is positive, return true (left).
            if (CalcShortestRot(@from, to) >= 0)
            {
                return true;
            }

            return false; // right
        }

        private static void CalculatePositionsWeapon(Pawn pawn, ref float weaponAngle,
                                                                                                                                             CompProperties_WeaponExtensions extensions,
                                                     out Vector3 weaponPosOffset, out bool aiming,
                                                     bool flipped)
        {
            weaponPosOffset = Vector3.zero;
            if (pawn.Rotation == Rot4.West || pawn.Rotation == Rot4.North)
            {
                weaponPosOffset.y = -Offsets.YOffset_Head - Offsets.YOffset_CarriedThing;
            }

            // Use y for the horizontal position. too lazy to add even more vectors
            bool isHorizontal = pawn.Rotation.IsHorizontal;
            aiming = pawn.Aiming();
            Vector3 extOffset;
            Vector3 o = extensions.WeaponPositionOffset;
            Vector3 d = extensions.AimedWeaponPositionOffset;
            if (isHorizontal)
            {
                extOffset = new Vector3(o.y, 0, o.z);
                if (aiming)
                {
                    extOffset += new Vector3(d.y, 0, d.z);
                }
            }
            else
            {
                extOffset = new Vector3(o.x, 0, o.z);
                if (aiming)
                {
                    extOffset += new Vector3(d.x, 0, d.z);
                }
            }

            if (flipped)
            {
                if (aiming)
                {
                    weaponAngle -= extensions?.AttackAngleOffset ?? 0;
                }

                weaponPosOffset += extOffset;

                // flip x position offset
                if (pawn.Rotation != Rot4.South)
                {
                    weaponPosOffset.x *= -1;
                }
            }
            else
            {
                if (aiming)
                {
                    weaponAngle += extensions?.AttackAngleOffset ?? 0;
                }

                weaponPosOffset += extOffset;
            }
        }
        #endregion Public Methods

        #region Private Methods

        private static void ChangeAngleForNorth(Pawn pawn, ref float aimAngle)
        {
            if (pawn.ShowWeaponOpenly() && pawn.Rotation == Rot4.North)
            {
                aimAngle = 217f;
            }
        }

        private static void CheckAllInjected()
        {
            // Thanks to all modders out there for providing help and support.
            // This is just for me.
            ///    Backstory childMe = new Backstory
            ///                        {
            ///                        bodyTypeMale   = BodyTypeDefOf.Male,
            ///                        bodyTypeFemale = BodyTypeDefOf.Female,
            ///                        slot           = BackstorySlot.Childhood,
            ///                        baseDesc =
            ///                        "NAME never believed what was common sense and always doubted other people. HECAP later went on inflating toads with HIS sushi stick. It was there HE earned HIS nickname.",
            ///                        requiredWorkTags = WorkTags.Violent,
            ///                        shuffleable      = false
            ///                        };
            ///    childMe.SetTitle("Lost child");
            ///    childMe.SetTitleShort("Seeker");
            ///    childMe.skillGainsResolved.Add("Shooting", 4);
            ///    childMe.skillGainsResolved.Add("Social",   1);
            ///    childMe.skillGainsResolved.Add("Medicine", 2);
            ///    childMe.PostLoad();
            ///    childMe.ResolveReferences();
            ///
            ///    Backstory adultMale = new Backstory
            ///                          {
            ///                          bodyTypeMale   = BodyTypeDefOf.Male,
            ///                          bodyTypeFemale = BodyTypeDefOf.Female,
            ///                          slot           = BackstorySlot.Adulthood,
            ///                          baseDesc =
            ///                          "HECAP tells no one about his past. HECAP doesn't like doctors, thus HECAP prefers to tend his wounds himself.",
            ///                          shuffleable     = false,
            ///                          spawnCategories = new List<string>()
            ///                          };
            ///    adultMale.spawnCategories.AddRange(new[] {"Civil", "Raider", "Slave", "Trader", "Traveler"});
            ///    adultMale.SetTitle("Lone gunman");
            ///    adultMale.SetTitleShort("Gunman");
            ///    adultMale.skillGains.Add("Shooting", 4);
            ///    adultMale.skillGains.Add("Medicine", 3);
            ///    adultMale.skillGains.Add("Cooking",  2);
            ///    adultMale.skillGains.Add("Social",   1);
            ///    adultMale.PostLoad();
            ///    adultMale.ResolveReferences();
            ///
            ///    PawnBio me = new PawnBio
            ///                 {
            ///                 childhood = childMe,
            ///                 adulthood = adultMale,
            ///                 gender    = GenderPossibility.Male,
            ///                 name      = NameTriple.FromString("Gator 'Killface' Stinkwater")
            ///                 };
            ///    me.PostLoad();
            ///    SolidBioDatabase.allBios.Add(me);
            ///    BackstoryDatabase.AddBackstory(childMe);
            ///
            ///    BackstoryDatabase.AddBackstory(adultMale);
        }

        #endregion Private Methods
    }
}