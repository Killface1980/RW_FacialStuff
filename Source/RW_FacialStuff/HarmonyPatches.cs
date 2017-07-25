namespace FacialStuff.Detouring
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

    using static GraphicDatabaseHeadRecordsModded;

    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.facialstuff.rimworld.mod");

            harmony.Patch(
                AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics)),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ResolveAllGraphics_Postfix)));

            harmony.Patch(
                AccessTools.Method(
                    typeof(PawnRenderer),
                    "RenderPawnInternal",
                    new[]
                        {
                            typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4),
                            typeof(RotDrawMode), typeof(bool), typeof(bool)
                        }),
                new HarmonyMethod(
                    typeof(HarmonyPatch_PawnRenderer),
                    nameof(HarmonyPatch_PawnRenderer.RenderPawnInternal_Prefix)),
                null);

            #region HealthTracker

            harmony.Patch(
                AccessTools.Method(
                    typeof(Pawn_HealthTracker),
                    nameof(Pawn_HealthTracker.AddHediff),
                    new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) }),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHediff_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RestorePart)),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(RestorePart_Postfix)));

            #endregion

            #region Hair

            harmony.Patch(
                AccessTools.Method(typeof(PawnHairChooser), nameof(PawnHairChooser.RandomHairDefFor)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(RandomHairDefFor_PreFix)),
               null);

            #endregion

            #region SkinColors

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
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetSkinColor)),
                new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.GetSkinColor_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.RandomMelanin)),
                new HarmonyMethod(typeof(PawnSkinColors_FS), nameof(PawnSkinColors_FS.RandomMelanin_Prefix)),
                null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnSkinColors), nameof(PawnSkinColors.GetMelaninCommonalityFactor)),
                new HarmonyMethod(
                    typeof(PawnSkinColors_FS),
                    nameof(PawnSkinColors_FS.GetMelaninCommonalityFactor_Prefix)),
                null);

            #endregion

            // harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message(
                "Facial Stuff successfully completed " + harmony.GetPatchedMethods().Count()
                + " patches with harmony.");
        }

        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            // Check if race has face, else return
            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }

            if (pawn.Faction == null)
            {
                pawn.SetFactionDirect(Faction.OfPlayer);
            }

            BuildDatabaseIfNecessary();

            if (faceComp.FacePawn == null)
            {
                faceComp.SetFaceOwner(pawn);
            }

            // Inital definition of a pawn's appearance. Run only once - ever.
            if (!faceComp.IsOptimized)
            {
                faceComp.DefineFace();
            }

            if (!faceComp.IsSkinDNAoptimized)
            {
                faceComp.DefineSkinDNA();
            }

            if (!faceComp.IsDNAoptimized)
            {
                faceComp.DefineHairDNA();
            }

            // Added in 0.17.3 beta, needs to be separate till major update A18
            if (Controller.settings.UseHairDNA)
            {
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    pawn.story.hairDef.texPath,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    pawn.story.hairColor);
            }
   
            // Custom rotting color, mixed with skin tone
            Color rotColor = pawn.story.SkinColor * Headhelper.skinRottingMultiplyColor;

            if (faceComp.SetHeadType())
            {
                if (faceComp.InitializeGraphics())
                {
                    // Set up the hair cut graphic
                    HaircutPawn hairPawn = CutHairDb.GetHairCache(pawn);
                    hairPawn.HairCutGraphic = CutHairDb.Get<Graphic_Multi>(
                        pawn.story.hairDef.texPath,
                        ShaderDatabase.Cutout,
                        Vector2.one,
                        pawn.story.hairColor);

                    __instance.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        pawn.story.SkinColor);
                    __instance.headGraphic = GetModdedHeadNamed(pawn, pawn.story.SkinColor);
                    __instance.desiccatedHeadGraphic = GetModdedHeadNamed(pawn, rotColor);
                    __instance.desiccatedHeadStumpGraphic = GetStump(rotColor);
                    __instance.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(
                        pawn.story.bodyType,
                        ShaderDatabase.CutoutSkin,
                        rotColor);
                    PortraitsCache.Clear();
                }
            }
        }

        public static void AddHediff_Postfix(
            Pawn_HealthTracker __instance,
            Hediff hediff,
            BodyPartRecord part = null,
            DamageInfo? dinfo = null)
        {
            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.Spawned)
            {
                return;
            }
            if (part == null)
            {
                return;
            }

            AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
            if (addedPartProps != null)
            {
                if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye
                    || part.def == BodyPartDefOf.Jaw)
                {
                    CompFace face = pawn.TryGetComp<CompFace>();
                    if (face != null)
                    {
                        face.CheckForAddedOrMissingParts();
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                }
            }
        }

        public static void RestorePart_Postfix(
            Pawn_HealthTracker __instance,
            BodyPartRecord part,
            Hediff diffException = null,
            bool checkStateChange = true)
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.Spawned)
            {
                return;
            }

            if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye
                || part.def == BodyPartDefOf.Head)
            {
                CompFace face = pawn.TryGetComp<CompFace>();
                if (face != null)
                {
                    face.CheckForAddedOrMissingParts();
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
            }
        }

        public static bool RandomHairDefFor_PreFix(Pawn pawn, FactionDef factionType, ref HairDef __result)
        {
            if (pawn.TryGetComp<CompFace>() == null)
            {
                return true;
            }

            IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                          where hair.hairTags.SharesElementWith(factionType.hairTags)
                                          select hair;

            __result = source.RandomElementByWeight(hair => PawnFaceChooser.HairChoiceLikelihoodFor(hair, pawn));
            return false;
        }
    }

    #region Hair 

    // [HarmonyPatch(typeof(PawnHairColors), "RandomHairColor")]
    public static class PawnHairColors_PostFix
    {
        public static Color HairPlatinum = new Color32(255, 245, 226, 255);

        public static Color HairYellowBlonde = new Color32(255, 203, 89, 255);

        public static Color HairTerraCotta = new Color32(185, 49, 4, 255);

        public static Color HairMediumDarkBrown = new Color32(110, 70, 10, 255);

        public static Color HairDarkBrown = new Color32(64, 41, 19, 255);

        public static Color HairMidnightBlack = new Color32(30, 30, 30, 255);

        public static Color HairDarkPurple = new Color32(162, 47, 115, 255);

        public static Color HairBlueSteel = new Color32(57, 115, 199, 255);

        public static Color HairBurgundyBistro = new Color32(206, 38, 58, 255);

        public static Color HairGreenGrape = new Color32(124, 189, 14, 255);

        public static Color HairMysticTurquois = new Color32(71, 191, 165, 255);

        public static Color HairPinkPearl = new Color32(230, 74, 153, 255);

        public static Color HairPurplePassion = new Color32(145, 50, 191, 255);

        public static Color HairRosaRosa = new Color32(215, 168, 255, 255);

        public static Color HairRubyRed = new Color32(227, 35, 41, 255);

        public static Color HairUltraViolet = new Color32(191, 53, 132, 255);

        public static List<Color> HairColors =
            new List<Color>()
                {
                    HairPlatinum,
                    HairYellowBlonde,
                    HairTerraCotta,
                    HairMediumDarkBrown,
                    HairDarkBrown,
                    HairMidnightBlack,
                    HairDarkPurple,
                    HairBlueSteel,
                    HairBurgundyBistro,
                    HairGreenGrape,
                    HairMysticTurquois,
                    HairPinkPearl,
                    HairPurplePassion,
                    HairRosaRosa,
                    HairRubyRed,
                    HairUltraViolet
                };

        // [HarmonyPostfix]
        public static void RandomHairColor(ref Color __result, Color skinColor, int ageYears)
        {
            Color tempColor;

            if (Rand.Value < 0.04f)
            {
                float rand = Rand.Value;
                if (rand < 0.1f)
                {
                    __result = HairDarkPurple;
                    return;
                }

                if (rand < 0.2f)
                {
                    __result = HairBlueSteel;
                    return;
                }

                if (rand < 0.3f)
                {
                    __result = HairBurgundyBistro;
                    return;
                }

                if (rand < 0.4f)
                {
                    __result = HairGreenGrape;
                }
                else if (rand < 0.5f)
                {
                    __result = HairMysticTurquois;
                }
                else if (rand < 0.6f)
                {
                    __result = HairPinkPearl;
                }
                else if (rand < 0.7f)
                {
                    __result = HairPurplePassion;
                }
                else if (rand < 0.8f)
                {
                    __result = HairRosaRosa;
                }
                else if (rand < 0.9f)
                {
                    __result = HairRubyRed;
                }
                else
                {
                    __result = HairUltraViolet;
                }

                return;
            }

            // if (Rand.Value < 0.02f)
            if (PawnSkinColors.IsDarkSkin(skinColor))
            {
                // || Rand.Value < 0.4f)
                tempColor = Color.Lerp(HairMidnightBlack, HairDarkBrown, Rand.Range(0f, 0.35f));

                tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0f, 0.3f));
                tempColor = Color.Lerp(tempColor, HairPlatinum, Rand.Range(0f, 0.45f));
            }
            else
            {
                float value2 = Rand.Value;

                // dark hair
                if (value2 < 0.125f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0.3f, 0.7f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0.1f, 0.8f));
                }

                // brown hair
                else if (value2 < 0.25f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0f, 0.6f));
                    tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.5f));
                }

                // dirty blonde hair
                else if (value2 < 0.5f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0f, 0.35f));
                }

                // dark red/brown hair
                else if (value2 < 0.7f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairTerraCotta, Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.35f));
                }

                // pure blond / albino
                else if (value2 < 0.85f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Range(0f, 0.5f));
                }

                // red hair
                else
                {
                    tempColor = Color.Lerp(HairYellowBlonde, HairTerraCotta, Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.25f));
                }
            }

            // age to become gray as float
            float ageFloat = (float)ageYears / 100;
            float agingBeginGreyFloat = Rand.Range(0.35f, 0.7f);

            float greySpan = Rand.Range(0.7f, 0.17f);

            float greyness = 0f;

            if (ageFloat > agingBeginGreyFloat)
            {
                greyness = Mathf.InverseLerp(agingBeginGreyFloat, agingBeginGreyFloat + greySpan, ageFloat);
            }

            // Soften the greyness
            greyness *= 0.9f;

            if (PawnSkinColors.IsDarkSkin(skinColor))
            {
                greyness *= Rand.Range(0.4f, 0.8f);
            }

            __result = Color.Lerp(tempColor, new Color32(245, 245, 245, 255), greyness);
        }

        internal static Color DarkerBeardColor(Color value)
        {
            Color darken = new Color(0.9f, 0.9f, 0.9f);

            return value * darken;
        }
    }

    #endregion
}