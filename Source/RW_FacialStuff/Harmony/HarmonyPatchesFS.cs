// ReSharper disable All

using System;
using System.Reflection.Emit;
using FacialStuff.HairCut;
using JetBrains.Annotations;
using TinyTween;

namespace FacialStuff.Harmony
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FaceEditor;
    using Genetics;
    using GraphicsFS;
    using Utilities;
    using global::Harmony;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    [StaticConstructorOnStartup]
    public static class HarmonyPatchesFS
    {
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

        #endregion Private Fields

        #region Public Constructors

        static HarmonyPatchesFS()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // HarmonyInstance.DEBUG = true;

            // harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), null, null, new HarmonyMethod(typeof(Alien), nameof(Alien.RenderPawnInternalTranspiler)));

            // harmony.Patch(
            // AccessTools.Method(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents)),
            // null,
            // null,
            // new HarmonyMethod(
            // typeof(Dialog_Options_DoWindowContents_Patch),
            // nameof(Dialog_Options_DoWindowContents_Patch.Transpiler)));

            harmony.Patch(
                          AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DrawPortraitArea"),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(AddFaceEditButton)));

            harmony.Patch(
                          AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(ResolveAllGraphics_Postfix)));

            harmony.Patch(
                          AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming)),
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(DrawEquipmentAiming_Prefix)),
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS),
                                            nameof(HarmonyPatchesFS.DrawEquipmentAiming_Transpiler)));

            // harmony.Patch(
            // AccessTools.Method(
            // typeof(PawnRenderer),
            // "RenderPawnInternal",
            // new[]
            // {
            // typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4),
            // typeof(RotDrawMode), typeof(bool), typeof(bool)
            // }),
            // new HarmonyMethod(
            // typeof(HarmonyPatch_PawnRenderer),
            // nameof(HarmonyPatch_PawnRenderer.RenderPawnInternal_Prefix)),
            // null);
            // harmony.Patch(
            // AccessTools.Method(
            // typeof(Pawn_HealthTracker),
            // nameof(Pawn_HealthTracker.AddHediff),
            // new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) }),
            // null,
            // new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(AddHediff_Postfix)));

            harmony.Patch(
                          AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt),
                                             new[] { typeof(Vector3), typeof(RotDrawMode), typeof(bool) }),
                          // new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(HarmonyPatchesFS.RenderPawnAt)),
                          null,
                          null,
                          new HarmonyMethod(typeof(HarmonyPatchesFS), nameof(HarmonyPatchesFS.RenderPawnAt_Transpiler))
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

            // if (!skinPatched)
            // {
            // AccessTools.Field(typeof(PawnSkinColors), "SkinColors").SetValue(
            // typeof(PawnSkinColors_FS.SkinColorData),
            // PawnSkinColors_FS.SkinColors);
            // skinPatched = true;
            // }
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

#if develop
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

                if (def.inspectorTabs.Contains(typeof(ITab_Pawn_Face)))
                {
                    return;
                }

                def.inspectorTabs.Add(typeof(ITab_Pawn_Face));
                def.inspectorTabs.Add(typeof(ITab_Pawn_Weapons));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Face)));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Weapons)));
            }

#endif
            CheckAllInjected();
        }

        #endregion Public Constructors

        #region Public Methods

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
                SoundDefOf.TickLow.PlayOneShotOnCamera(null);
                OpenStylingWindow(pawn);
            }
        }

        public static bool Aiming(Pawn pawn)
        {
            return pawn.stances.curStance is Stance_Busy stanceBusy && !stanceBusy.neverAimWeapon &&
                   stanceBusy.focusTarg.IsValid;
        }


        public static bool CarryWeaponOpenly(Pawn pawn)
        {
            return pawn.carryTracker?.CarriedThing == null &&
                   (pawn.Drafted ||
                    (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) ||
                    (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
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

            pawn.CheckForAddedOrMissingParts();
            if (!compFace.Deactivated)
            {
                pawn.Drawer.renderer.graphics.nakedGraphic = null;
                PortraitsCache.SetDirty(pawn);
            }

            pawn.GetCompAnim()?.PawnBodyGraphic?.Initialize();
        }

        public static void DoAttackAnimationOffsetsWeapons(Pawn pawn, ref float weaponAngle,
                                                    ref Vector3 weaponPosition,
                                                    bool flipped, CompBodyAnimator animator, out bool noTween)
        {
            CompEquippable primaryEq = pawn.equipment?.PrimaryEq;
            noTween = false;
            // DamageDef damageDef = primaryEq?.PrimaryVerb?.verbProps?.meleeDamageDef;
            if (primaryEq?.parent?.def == null)
            {
                return;
            }

            Stance_Busy busy = pawn.stances.curStance as Stance_Busy;
            if (busy== null) { return;}

          //  if (!busy.verb.IsMeleeAttack)
          //  {
          //      return;
          //  }

            DamageDef damageDef = busy.verb.GetDamageDef();//ThingUtility.PrimaryMeleeWeaponDamageType(primaryEq.parent.def);
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
                weaponAngle += flipped ? -animationPhasePercent * totalSwingAngle : animationPhasePercent * totalSwingAngle;
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
        public static void DoCalculations(Pawn pawn, Thing eq, ref Vector3 drawLoc, ref float weaponAngle,
                                          float aimAngle)
        {
            if (pawn == null)
            {
                return;
            }
            bool aiming = Aiming(pawn);
            Vector3 weaponPosOffset = Vector3.zero;

            CompProperties_WeaponExtensions extensions = eq.def.GetCompProperties<CompProperties_WeaponExtensions>();

            // Return if nothing to do
            if (extensions == null)
            {
                return;
            }

            if (!pawn.GetCompAnim(out CompBodyAnimator animator))
            {
                return;
            }

            bool isHorizontal = pawn.Rotation.IsHorizontal;

            if (pawn.Rotation == Rot4.West || pawn.Rotation == Rot4.North)
            {
                weaponPosOffset.y = -Offsets.YOffset_Head - Offsets.YOffset_CarriedThing;
            }

            bool flipped = aimAngle > 200f && aimAngle < 340f;

            Vector3 extOffset;
            Vector3 o = extensions.WeaponPositionOffset;
            Vector3 d = extensions.AimedWeaponPositionOffset;
            // Use y for the horizontal position. too lazy to add even more vectors
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

            // weapon angle and position offsets based on current attack keyframes sequence

            DoAttackAnimationOffsetsWeapons(pawn, ref weaponAngle, ref weaponPosOffset, flipped, animator, out bool noTween);

            /*
            float distance = Vector3.Distance(animator.lastEqPos, drawLoc);
            if (distance > 1f)
            {
                Vector3Tween eqTweener = animator.eqTweener;
                switch (eqTweener.State)
                {
                    case TweenState.Stopped:
                        eqTweener.Start(animator.lastEqPos, drawLoc, distance, ScaleFuncs.CubicEaseOut);
                        break;

                    case default(TweenState):
                        if (!Find.TickManager.Paused)
                        {
                            eqTweener.Update(0.1f);
                        }

                        break;
                }

                drawLoc = eqTweener.CurrentValue;
            }

            animator.lastEqPos = drawLoc;
            */
            animator.PartTweener.PartPositions[(int)TweenThing.Equipment] = drawLoc + weaponPosOffset;

            animator.PartTweener.PreThingPosCalculation(TweenThing.Equipment);

            drawLoc = animator.PartTweener.TweenedPartsPos[(int)TweenThing.Equipment];

            float angleChange = Mathf.Abs(weaponAngle - animator.lastWeaponAngle);


            if (angleChange > 6f && !noTween)
            {
                // If pawn flips sides, no tween

                bool x = Mathf.Abs(animator.lastAimAngle - angleStanding) < 3f &&
                         Mathf.Abs(aimAngle - angleStandingFlipped) < 3f;
                bool y = Mathf.Abs(animator.lastAimAngle - angleStandingFlipped) < 3f &&
                         Mathf.Abs(aimAngle - angleStanding) < 3f;
                bool z = Math.Abs(Mathf.Abs(aimAngle - animator.lastAimAngle) - 180f) < 12f;

                if (!x && !y && !z)
                {

                    FloatTween tween = animator.tween;
                    if (!Find.TickManager.Paused)
                    {
                        if (Math.Abs(tween.EndValue - weaponAngle) > 6f)
                        {
                            tween.Start(animator.lastWeaponAngle, weaponAngle, angleChange, ScaleFuncs.QuadraticEaseInOut);
                        }
                        tween.Update(6f);
                    }
                    weaponAngle = tween.CurrentValue;
                }

            }

            animator.lastAimAngle = aimAngle;
            animator.lastWeaponAngle = weaponAngle;



            // Now the remaining hands if possible
            if (animator.Props.bipedWithHands && Controller.settings.UseHands)
            {
                CalculateHandsAiming(
                                     drawLoc,
                                     flipped,
                                     weaponAngle,
                                     extensions, animator, pawn, aiming);
            }

        }

        public static void CalculateHandsAiming(
        Vector3 weaponPosition,
        bool flipped,
        float weaponAngle,
        [CanBeNull] CompProperties_WeaponExtensions compWeaponExtensions,
        CompBodyAnimator animator, Pawn pawn, bool aiming)
        {
            // Prepare everything for DrawHands, but don't draw
            if (compWeaponExtensions == null)
            {
                return;
            }

            animator.FirstHandPosition = compWeaponExtensions.RightHandPosition;

            // Only put the second hand on when aiming or not moving => free left hand for running 
            bool leftOnWeapon = true;// aiming || !animator.IsMoving;
            animator.SecondHandPosition = leftOnWeapon ? compWeaponExtensions.LeftHandPosition : Vector3.zero;

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

                animator.FirstHandPosition =
                weaponPosition + new Vector3(x, y, z).RotatedBy(weaponAngle);
            }

            if (animator.SecondHandPosition != Vector3.zero)
            {
                float x2 = animator.SecondHandPosition.x;
                float y2 = animator.SecondHandPosition.y;
                float z2 = animator.SecondHandPosition.z;
                if (flipped)
                {
                    x2 *= -1f;
                    y2 *= -1f;
                }

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

        public static void DrawEquipmentAiming_Prefix(PawnRenderer __instance, Thing eq, Vector3 drawLoc,
                                                      ref float aimAngle)
        {
            Pawn pawn = __instance.graphics.pawn;

            // Flip the angle for north

            if (pawn.Rotation == Rot4.North && aimAngle == angleStanding)
            {
                aimAngle = angleStandingFlipped;
            }
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
                                                     new CodeInstruction(OpCodes.Ldarg_0),
                                                     new CodeInstruction(OpCodes.Ldfld,
                                                                         AccessTools.Field(typeof(PawnRenderer),
                                                                                           "pawn")),
                                                     new CodeInstruction(OpCodes.Ldloc_2),
                                                     new CodeInstruction(OpCodes.Call,
                                                                         AccessTools.Method(typeof(HarmonyPatchesFS),
                                                                                            nameof(CheckAndDrawHands)))
                                                     });
            return instructionList;
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
                                                                   AccessTools.Field(typeof(PawnRenderer), "pawn")), // pawn
                                               new CodeInstruction(OpCodes.Ldarg_1), // Thing
                                               new CodeInstruction(OpCodes.Ldarga,   2), // drawLoc
                                               new CodeInstruction(OpCodes.Ldloca_S, 1), // weaponAngle
                                               new CodeInstruction(OpCodes.Ldarg_3), // aimAngle
                                               new CodeInstruction(OpCodes.Call,
                                                                   AccessTools.Method(typeof(HarmonyPatchesFS),
                                                                                      nameof(DoCalculations))),
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
                                          where hair.hairTags.SharesElementWith(hairTags)
                                          select hair;

            __result = source.RandomElementByWeight(hair => PawnFaceMaker.HairChoiceLikelihoodFor(hair, pawn));
            return false;
        }


        public static void CheckAndDrawHands(Thing carriedThing, Vector3 vector, bool flip, Pawn pawn, Vector3 loc)
        {
            if (pawn.RaceProps.Animal)
            {
                carriedThing.DrawAt(vector, flip);
                return;
            }

            if (!pawn.GetCompAnim(out CompBodyAnimator compAnim))
            {
                carriedThing.DrawAt(vector, flip);
                return;
            }

            bool showHands = compAnim.Props.bipedWithHands && Controller.settings.UseHands;
            if (!showHands)
            {
                carriedThing.DrawAt(vector, flip);
                return;
            }

            loc.y += pawn.Rotation == Rot4.North ? -0.05f : Offsets.YOffset_Body;

            compAnim.DrawHands(Quaternion.identity, loc, false, carriedThing, flip);
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

            compFace.IsChild = pawn.ageTracker.AgeBiologicalYearsFloat < 14;

            // Return if child
            if (compFace.IsChild || compFace.Deactivated)
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

            // Set up the hair cut graphic
            if (Controller.settings.MergeHair)
            {
                HairCutPawn hairPawn = CutHairDB.GetHairCache(pawn);
                hairPawn.HairCutGraphic = CutHairDB.Get<Graphic_Multi>(
                                                                       pawn.story.hairDef.texPath,
                                                                       ShaderDatabase.Cutout,
                                                                       Vector2.one,
                                                                       pawn.story.hairColor);
            }

            __instance.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                                                                                       pawn.story.bodyType,
                                                                                       ShaderDatabase.CutoutSkin,
                                                                                       pawn.story.SkinColor);

            if (compFace.Props.needsBlankHumanHead)
            {
                __instance.headGraphic =
                GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn,
                                                                    pawn.story.SkinColor);
                __instance.desiccatedHeadGraphic =
                GraphicDatabaseHeadRecordsModded.GetModdedHeadNamed(pawn, rotColor);
                __instance.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecordsModded.GetStump(rotColor);
            }

            __instance.rottingGraphic =
            GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                                                             pawn.story.bodyType,
                                                             ShaderDatabase.CutoutSkin,
                                                             rotColor);
            __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                                                                        pawn.story.hairDef.texPath,
                                                                        ShaderDatabase.Cutout,
                                                                        Vector2.one,
                                                                        pawn.story.hairColor);
            PortraitsCache.SetDirty(pawn);
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
                        if (compFace.HeadRotator != null && !compFace.IsChild)
                        {
                            compFace.HeadRotator.LookAtPawn(recipient);
                        }
                    }
                }

                if (recipient.GetCompFace(out CompFace recipientFace))
                {
                    if (recipientFace.Props.canRotateHead)
                    {
                        if (recipientFace.HeadRotator != null && !recipientFace.IsChild)
                        {
                            recipientFace.HeadRotator.LookAtPawn(pawn);
                        }
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void ChangeAngleForNorth(Pawn pawn, ref float aimAngle)
        {
            if (CarryWeaponOpenly(pawn) && pawn.Rotation == Rot4.North)
            {
                aimAngle = 217f;
            }
        }

        private static void CheckAllInjected()
        {
            // Thanks to all modders out there for providing help and support.
            // This is just for me.
            Backstory childMe = new Backstory
            {
                bodyTypeMale = BodyType.Male,
                bodyTypeFemale = BodyType.Female,
                slot = BackstorySlot.Childhood,
                baseDesc =
                                "NAME never believed what was common sense and always doubted other people. HECAP later went on inflating toads with HIS sushi stick. It was there HE earned HIS nickname.",
                requiredWorkTags = WorkTags.Violent,
                shuffleable = false
            };
            childMe.SetTitle("Lost child");
            childMe.SetTitleShort("Seeker");
            childMe.skillGains.Add("Shooting", 4);
            childMe.skillGains.Add("Medicine", 2);
            childMe.skillGains.Add("Social", 1);
            childMe.PostLoad();
            childMe.ResolveReferences();

            Backstory adultMale = new Backstory
            {
                bodyTypeMale = BodyType.Male,
                bodyTypeFemale = BodyType.Female,
                slot = BackstorySlot.Adulthood,
                baseDesc =
                                  "HECAP tells no one about his past. HECAP doesn't like doctors, thus HECAP prefers to tend his wounds himself.",
                shuffleable = false,
                spawnCategories = new List<string>()
            };
            adultMale.spawnCategories.AddRange(new[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
            adultMale.SetTitle("Lone gunman");
            adultMale.SetTitleShort("Gunman");
            adultMale.skillGains.Add("Shooting", 4);
            adultMale.skillGains.Add("Medicine", 3);
            adultMale.skillGains.Add("Cooking", 2);
            adultMale.skillGains.Add("Social", 1);
            adultMale.PostLoad();
            adultMale.ResolveReferences();

            PawnBio me = new PawnBio
            {
                childhood = childMe,
                adulthood = adultMale,
                gender = GenderPossibility.Male,
                name = NameTriple.FromString("Gator 'Killface' Stinkwater")
            };
            me.PostLoad();
            SolidBioDatabase.allBios.Add(me);
            BackstoryDatabase.AddBackstory(childMe);

            BackstoryDatabase.AddBackstory(adultMale);
        }

        #endregion Private Methods
    }
}