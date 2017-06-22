using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Detouring
{
    using System.Collections.Generic;

    public static class _PawnHairColors
    {
        // changed Midnight Black - 10/10/10 - 2016-07-28
        public static readonly Dictionary<string, Color> HairColors =
            new Dictionary<string, Color>()
                {
                    { "HairPlatinum", new Color32(255, 245, 226, 255) },
                    { "HairYellowBlonde", new Color32(255, 203, 89, 255) },
                    { "HairTerraCotta", new Color32(200, 50, 0, 255) },
                    { "HairMediumDarkBrown", new Color32(110, 70, 10, 255) },
                    { "HairDarkBrown", new Color32(64, 41, 19, 255) },
                    { "HairMidnightBlack", new Color32(30, 30, 30, 255) },
                    { "HairDarkPurple", new Color32(191, 92, 85, 255) },
                    { "HairBlueSteel", new Color32(57, 115, 199, 255) },
                    { "HairBurgundyBistro", new Color32(208, 65, 82, 255) },
                    { "HairGreenGrape", new Color32(124, 189, 14, 255) },
                    { "HairMysticTurquois", new Color32(71, 191, 165, 255) },
                    { "HairPinkPearl", new Color32(230, 74, 103, 255) },
                    { "HairPurplePassion", new Color32(145, 50, 191, 255) },
                    { "HairRosaRosa", new Color32(243, 214, 200, 255) },
                    { "HairRubyRed", new Color32(227, 35, 41, 255) },
                    { "HairUltraViolet", new Color32(191, 53, 132, 255) },
                };

        // public static readonly Color HairPlatinum = new Color32(255, 245, 226, 255);//(0,929411769f, 0,7921569f, 0,6117647f);
        // public static readonly Color HairYellowBlonde = new Color32(255, 203, 89, 255);//(0,75686276f, 0,572549045f, 0,333333343f);
        // public static readonly Color HairTerraCotta = new Color32(200, 50, 0, 255);//(0,3529412f, 0,227450982f, 0,1254902f);
        // public static readonly Color HairColors["HairMediumDarkBrown"], = new Color32(110, 70, 10, 255);//(0.25f, 0.2f, 0.15f);
        // public static readonly Color HairDarkBrown = new Color32(64, 41, 19, 255);//(0.25f, 0.2f, 0.15f);
        // public static readonly Color HairMidnightBlack = new Color32(30, 30, 30, 255);//(0.2f, 0.2f, 0.2f);
        // public static readonly Color HairDarkPurple = new Color32(191, 92, 85, 255);
        // public static readonly Color HairBlueSteel = new Color32(57, 115, 199, 255);
        // public static readonly Color HairBurgundyBistro = new Color32(208, 65, 82, 255);
        // public static readonly Color HairGreenGrape = new Color32(124, 189, 14, 255);
        // public static readonly Color HairMysticTurquois = new Color32(71, 191, 165, 255);
        // public static readonly Color HairPinkPearl = new Color32(230, 74, 103, 255);
        // public static readonly Color HairPurplePassion = new Color32(145, 50, 191, 255);
        // public static readonly Color HairRosaRosa = new Color32(243, 214, 200, 255);
        // public static readonly Color HairRubyRed = new Color32(227, 35, 41, 255);
        // public static readonly Color HairUltraViolet = new Color32(191, 53, 132, 255);

        // [HarmonyPatch(typeof(PawnHairColors), "RandomHairColor")]
        // public static class RandomHairColor_Postfix
        // {
        // }
        [Detour(typeof(PawnHairColors), bindingFlags = BindingFlags.Static | BindingFlags.Public)]
        public static Color RandomHairColor(Color skinColor, int ageYears)
        {
            Color tempColor;

            if (Rand.Value < 0.04f)
            {
                float rand = Rand.Value;
                if (rand < 0.1f)
                {
                    return HairColors["HairDarkPurple"];
                }

                if (rand < 0.2f)
                {
                    return HairColors["HairBlueSteel"];
                }

                if (rand < 0.3f)
                {
                    return HairColors["HairBurgundyBistro"];
                }

                if (rand < 0.4f) return HairColors["HairGreenGrape"];
                if (rand < 0.5f) return HairColors["HairMysticTurquois"];
                if (rand < 0.6f) return HairColors["HairPinkPearl"];
                if (rand < 0.7f) return HairColors["HairPurplePassion"];
                if (rand < 0.8f) return HairColors["HairRosaRosa"];
                if (rand < 0.9f) return HairColors["HairRubyRed"];
                return HairColors["HairUltraViolet"];
            }

            // if (Rand.Value < 0.02f)
            if (_PawnSkinColors.IsDarkSkin(skinColor))
            {
                // || Rand.Value < 0.4f)
                tempColor = Color.Lerp(
                    HairColors["HairMidnightBlack"],
                    HairColors["HairDarkBrown"],
                    Rand.Range(0f, 0.35f));

                tempColor = Color.Lerp(tempColor, HairColors["HairTerraCotta"], Rand.Range(0f, 0.3f));
                tempColor = Color.Lerp(tempColor, HairColors["HairPlatinum"], Rand.Range(0f, 0.45f));
            }
            else
            {
                float value2 = Rand.Value;

                // dark hair
                if (value2 < 0.125f)
                {
                    tempColor = Color.Lerp(HairColors["HairPlatinum"], HairColors["HairYellowBlonde"], Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairColors["HairTerraCotta"], Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, HairColors["HairMediumDarkBrown"], Rand.Range(0.3f, 0.7f));
                    tempColor = Color.Lerp(tempColor, HairColors["HairMidnightBlack"], Rand.Range(0.1f, 0.8f));
                }

                // brown hair
                else if (value2 < 0.25f)
                {
                    tempColor = Color.Lerp(HairColors["HairPlatinum"], HairColors["HairYellowBlonde"], Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairColors["HairTerraCotta"], Rand.Range(0f, 0.6f));
                    tempColor = Color.Lerp(tempColor, HairColors["HairMediumDarkBrown"], Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, HairColors["HairMidnightBlack"], Rand.Range(0f, 0.5f));
                }

                // dirty blonde hair
                else if (value2 < 0.5f)
                {
                    tempColor = Color.Lerp(HairColors["HairPlatinum"], HairColors["HairYellowBlonde"], Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairColors["HairMediumDarkBrown"], Rand.Range(0f, 0.35f));
                }

                // dark red/brown hair
                else if (value2 < 0.7f)
                {
                    tempColor = Color.Lerp(
                        HairColors["HairPlatinum"],
                        HairColors["HairTerraCotta"],
                        Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, HairColors["HairMidnightBlack"], Rand.Range(0f, 0.35f));
                }

                // pure blond / albino
                else if (value2 < 0.85f)
                {
                    tempColor = Color.Lerp(
                        HairColors["HairPlatinum"],
                        HairColors["HairYellowBlonde"],
                        Rand.Range(0f, 0.5f));
                }

                // red hair
                else
                {
                    tempColor = Color.Lerp(
                        HairColors["HairYellowBlonde"],
                        HairColors["HairTerraCotta"],
                        Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, HairColors["HairMidnightBlack"], Rand.Range(0f, 0.25f));
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

            return Color.Lerp(tempColor, new Color32(245, 245, 245, 255), greyness);
        }

        // public static Color Dark01 = new Color32(50, 50, 50, 255);//(0.2f, 0.2f, 0.2f);
        // public static Color Dark02 = new Color32(79, 71, 66, 255);//(0.31f, 0.28f, 0.26f);
        // public static Color Dark03 = new Color32(64, 50, 37, 255);//(0.25f, 0.2f, 0.15f);
        // public static Color Dark04 = new Color32(75, 50, 25, 255);//(0.3f, 0.2f, 0.1f);
        // public static Color Color01 = new Color32(90, 58, 32, 255);//(0,3529412f, 0,227450982f, 0,1254902f);
        // public static Color Color02 = new Color32(132, 83, 47, 255);//(0,5176471f, 0,3254902f, 0,184313729f);
        // public static Color Color03 = new Color32(193, 146, 85, 255);//(0,75686276f, 0,572549045f, 0,333333343f);
        // public static Color Color04 = new Color32(237, 202, 156, 255);//(0,929411769f, 0,7921569f, 0,6117647f);
        public static Color RandomBeardColor()
        {
            float value = Rand.Value;
            Color tempColor = new Color();

            // dark hair
            if (value > 0.15f)
            {
                tempColor = Color.Lerp(HairColors["HairPlatinum"], HairColors["HairYellowBlonde"], Rand.Value);
                tempColor = Color.Lerp(tempColor, HairColors["HairTerraCotta"], Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, HairColors["HairMediumDarkBrown"], Rand.Range(0.3f, 0.7f));
                tempColor = Color.Lerp(tempColor, HairColors["HairMidnightBlack"], Rand.Range(0.1f, 0.8f));
            }

            // brown hair
            else
            {
                tempColor = Color.Lerp(HairColors["HairPlatinum"], HairColors["HairYellowBlonde"], Rand.Value);
                tempColor = Color.Lerp(tempColor, HairColors["HairTerraCotta"], Rand.Range(0f, 0.6f));
                tempColor = Color.Lerp(tempColor, HairColors["HairMediumDarkBrown"], Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, HairColors["HairMidnightBlack"], Rand.Range(0f, 0.5f));
            }

            return tempColor;
        }

        internal static Color DarkerBeardColor(Color value)
        {
            Color darken = new Color(0.8f, 0.8f, 0.8f);

            return value * darken;
        }
    }
}
