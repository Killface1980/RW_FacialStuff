using System.Collections.Generic;

using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Detouring
{
    using System;
    using System.Reflection;

    using FacialStuff.Genetics;

    using RW_FacialStuff;

    using static GraphicDatabaseHeadRecordsModded;

    using static _PawnSkinColors;

    public static class SaveableCache
    {
        public static List<HaircutPawn> _pawnHairCache = new List<HaircutPawn>();

        public static HaircutPawn GetHairCache(Pawn pawn)
        {
            foreach (HaircutPawn c in _pawnHairCache) if (c.Pawn == pawn) return c;
            HaircutPawn n = new HaircutPawn { Pawn = pawn };
            _pawnHairCache.Add(n);
            return n;

            // if (!PawnApparelStatCaches.ContainsKey(pawn))
            // {
            // PawnApparelStatCaches.Add(pawn, new StatCache(pawn));
            // }
            // return PawnApparelStatCaches[pawn];
        }
    }

    #region Graphics

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    static class ResolveAllGraphics_Patch
    {
        [HarmonyPostfix]
        public static void ResolveAllGraphics_Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            CompFace faceComp = pawn.TryGetComp<CompFace>();

            if (faceComp == null)
            {
                return;
            }
            faceComp.pawn = pawn;

            BuildDatabaseIfNecessary();

            Color rotColor = pawn.story.SkinColor * Headhelper.skinRottingMultiplyColor;

            if (FS_Settings.UseHairDNA)
            {
                if (!faceComp.DNAoptimized)
                {
                    faceComp.DefineDNA();
                }
                __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    pawn.story.hairDef.texPath,
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    pawn.story.hairColor);
            }

            if (!faceComp.optimized)
            {
                faceComp.DefineFace();
            }

            if (faceComp.SetHeadType())
            {
                if (faceComp.InitializeGraphics())
                {


                    HaircutPawn hairPawn = SaveableCache.GetHairCache(pawn);
                    hairPawn.HairCutGraphic = CutHairDb.Get<Graphic_Multi>(
                        pawn.story.hairDef.texPath,
                        ShaderDatabase.Cutout,
                        Vector2.one,
                        pawn.story.hairColor);

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
    }

    #endregion

    #region HealthTracker

    [HarmonyPatch(
        typeof(Pawn_HealthTracker),
        "AddHediff",
        new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo) })]
    public static class AddHediff_Postfix
    {
        private static Type PawnRendererType;

        private static FieldInfo PawnFieldInfo;

        private static void GetReflections()
        {
            if (PawnRendererType == null)
            {
                PawnRendererType = typeof(Pawn_HealthTracker);
                PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        [HarmonyPostfix]
        public static void CheckBionic(
            Pawn_HealthTracker __instance,
            Hediff hediff,
            BodyPartRecord part = null,
            DamageInfo? dinfo = null)
        {
            if (!FS_Settings.ShowExtraParts)
            {
                return;

            }

            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            if (part == null)
            {
                return;
            }

            GetReflections();

            AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
            if (addedPartProps != null)
            {
                if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye
                    || part.def == BodyPartDefOf.Jaw)
                {
                    Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
                    if (pawn != null && pawn.Spawned)
                    {
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "RestorePart")]
    public static class RestorePart_Postfix
    {
        private static Type PawnRendererType;

        private static FieldInfo PawnFieldInfo;

        private static void GetReflections()
        {
            if (PawnRendererType == null)
            {
                PawnRendererType = typeof(Pawn_HealthTracker);
                PawnFieldInfo = PawnRendererType.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        [HarmonyPostfix]
        public static void CheckBionic_RestorePart(
            Pawn_HealthTracker __instance,
            BodyPartRecord part,
            Hediff diffException = null,
            bool checkStateChange = true)
        {
            if (!FS_Settings.ShowExtraParts)
            {
                return;

            }
            GetReflections();

            Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);

            if (part.def == BodyPartDefOf.LeftEye || part.def == BodyPartDefOf.RightEye
                || part.def == BodyPartDefOf.Head)
            {
                // AddedBodyPartProps addedPartProps = hediff.def.addedPartProps;
                // if (addedPartProps != null && addedPartProps.isBionic)
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }
    }

    #endregion

    #region Hair Colour

    [HarmonyPatch(typeof(PawnHairColors), "RandomHairColor")]
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

        [HarmonyPostfix]
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

                if (rand < 0.4f) __result = HairGreenGrape;
                else if (rand < 0.5f) __result = HairMysticTurquois;
                else if (rand < 0.6f) __result = HairPinkPearl;
                else if (rand < 0.7f) __result = HairPurplePassion;
                else if (rand < 0.8f) __result = HairRosaRosa;
                else if (rand < 0.9f) __result = HairRubyRed;
                else __result = HairUltraViolet;
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

        public static Color RandomBeardColor()
        {
            float value = Rand.Value;
            Color tempColor = new Color();

            // dark hair
            if (value > 0.15f)
            {
                tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0.3f, 0.7f));
                tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0.1f, 0.8f));
            }

            // brown hair
            else
            {
                tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0f, 0.6f));
                tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.5f));
            }

            return tempColor;
        }

        internal static Color DarkerBeardColor(Color value)
        {
            Color darken = new Color(0.8f, 0.8f, 0.8f);

            return value * darken;
        }
    }

    #endregion

    /*
        #region Skin Color
    
        [HarmonyPatch(typeof(PawnSkinColors), "IsDarkSkin")]
        public static class IsDarkSkin_PostFix
        {
    
            [HarmonyPostfix]
            public static void IsDarkSkin(ref bool __result, Color color)
            {
                Color skinColor = PawnSkinColors.GetSkinColor(0.5f);
                __result = color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
            }
        }
    
        [HarmonyPatch(typeof(PawnSkinColors), "GetSkinColor")]
        public static class GetSkinColor_PostFix
        {
            [HarmonyPostfix]
            public static void GetSkinColor(ref Color __result, float melanin)
            {
                int skinDataLeftIndexByMelanin = 0;
                GetSkinDataIndexOfMelanin_PostFix.GetSkinDataIndexOfMelanin(ref skinDataLeftIndexByMelanin, melanin);
                if (skinDataLeftIndexByMelanin == _SkinColors.Length - 1)
                {
                    __result = _SkinColors[skinDataLeftIndexByMelanin].color;
                    return;
                }
    
                float t = Mathf.InverseLerp(_SkinColors[skinDataLeftIndexByMelanin].melanin, _SkinColors[skinDataLeftIndexByMelanin + 1].melanin, melanin);
                __result = Color.Lerp(_SkinColors[skinDataLeftIndexByMelanin].color, _SkinColors[skinDataLeftIndexByMelanin + 1].color, t);
            }
    
        }
    
        [HarmonyPatch(typeof(PawnSkinColors), "GetSkinDataIndexOfMelanin")]
        public static class GetSkinDataIndexOfMelanin_PostFix
        {
            [HarmonyPostfix]
            public static void GetSkinDataIndexOfMelanin(ref int __result, float melanin)
            {
                int result = 0;
                for (int i = 0; i < _SkinColors.Length; i++)
                {
                    if (melanin < _SkinColors[i].melanin)
                    {
                        break;
                    }
    
                    result = i;
                }
    
                __result = result;
            }
    
        }
    
        [HarmonyPatch(typeof(PawnSkinColors), "RandomMelanin")]
        public static class RandomMelanin_PostFix
        {
            [HarmonyPostfix]
            public static void RandomMelanin(ref float __result)
            {
                float value = Rand.Value;
                int num = 0;
                for (int i = 0; i < _SkinColors.Length; i++)
                {
                    if (value < _SkinColors[i].selector)
                    {
                        break;
                    }
    
                    num = i;
                }
    
                if (num == _SkinColors.Length - 1)
                {
                    __result = _SkinColors[num].melanin;
                    return;
                }
    
                float t = Mathf.InverseLerp(_SkinColors[num].selector, _SkinColors[num + 1].selector, value);
                __result = Mathf.Lerp(_SkinColors[num].melanin, _SkinColors[num + 1].melanin, t);
            }
    
        }
    
        [HarmonyPatch(typeof(PawnSkinColors), "GetMelaninCommonalityFactor")]
        public static class GetMelaninCommonalityFactor_PostFix
        {
            [HarmonyPostfix]
            public static void GetMelaninCommonalityFactor(ref float __result, float melanin)
            {
                int skinDataLeftIndexByWhiteness = 0;
                GetSkinDataIndexOfMelanin_PostFix.GetSkinDataIndexOfMelanin(ref skinDataLeftIndexByWhiteness, melanin);
                if (skinDataLeftIndexByWhiteness == _SkinColors.Length - 1)
                {
                    __result = GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness);
                    return;
                }
    
                float t = Mathf.InverseLerp(_SkinColors[skinDataLeftIndexByWhiteness].melanin, _SkinColors[skinDataLeftIndexByWhiteness + 1].melanin, melanin);
                __result = Mathf.Lerp(GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness), GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness + 1), t);
            }
    
        }
    
        #endregion
    
        */
}