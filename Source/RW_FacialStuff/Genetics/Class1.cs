using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff.Genetics
{
    using FacialStuff.Detouring;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public static class Class1
    {

        public static Color HairPlatinum = new Color32(255, 245, 226, 255);
        public static Color HairYellowBlonde = new Color32(255, 203, 89, 255);

        public static Color HairTerraCotta = new Color32(185, 49, 4, 255);

        public static Color HairDarkBrown = new Color32(79, 47, 17, 255);

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

        public static Gradient gradient_mel2 = new Gradient();
        public static Gradient gradient_mel1 = new Gradient();

        public static void Genetics(Pawn pawn, CompFace face, out float melanin1, out float melanin2)
        {
            melanin1 = melanin2 = 0f;
            if (face == null)
            {
                return;
            }
            CompFace mother = null;
            CompFace father = null;
            bool motherNull = false;
            bool fatherNull = false;

            if (pawn.GetMother() == null)
            {
                motherNull = true;
            }
            else
            {
                mother = pawn.GetMother().TryGetComp<CompFace>();
            }
            if (pawn.GetFather() == null)
            {
                fatherNull = true;
            }
            else
            {
                father = pawn.GetFather().TryGetComp<CompFace>();
            }


            if (!motherNull && mother.DNAoptimized && !fatherNull && father.DNAoptimized)
            {
                melanin1 = GetRandomChildHairColor(mother.melanin1, father.melanin1);
                melanin2 = GetRandomChildHairColor(mother.melanin2, father.melanin2);
            }
            else if (!motherNull && mother.DNAoptimized)
            {
                melanin1 = GetRandomMelaninSimilarTo(mother.melanin1, 0f, 1f);
                melanin2 = GetRandomMelaninSimilarTo(mother.melanin2, 0f, 1f);
            }
            else if (!fatherNull && father.DNAoptimized)
            {
                melanin1 = GetRandomMelaninSimilarTo(father.melanin1, 0f, 1f);
                melanin2 = GetRandomMelaninSimilarTo(father.melanin2, 0f, 1f);
            }
            else //if (motherNull && fatherNull)
            {
                bool flag = true;
                if (pawn.relations.FamilyByBlood.Any())
                {
                    var relPawn = pawn.relations.FamilyByBlood.FirstOrDefault(x => x.TryGetComp<CompFace>().DNAoptimized);
                    if (relPawn != null)
                    {
                        CompFace relatedPawn = relPawn.TryGetComp<CompFace>();

                        float melaninx1 = relatedPawn.melanin1;
                        float melaninx2 = relatedPawn.melanin2;
                        melanin1 = GetRandomMelaninSimilarTo(melaninx1);
                        melanin2 = GetRandomMelaninSimilarTo(melaninx2);
                        flag = false;
                    }
                }
                if (flag)
                {
                    melanin1 = Rand.Range(pawn.story.melanin * 0.75f, Mathf.Min(pawn.story.melanin * 1.5f + 0.1f, 1f));
                    melanin2 = Rand.Range(pawn.story.melanin / 2, 1f);
                }
            }
            //  Log.Message(
            //      pawn + " - " + melanin + " - " + face.melanin1 + " - " + face.melanin2 + " - " + mother?.melanin1
            //      + mother?.melanin2 + father?.melanin1 + father?.melanin2);


            // Check if the player has already set a custom color, else dna!
            if (pawn.story.hairColor == face.HairColorOrg)
            {
                // Build gradients
                GradientColorKey[] gck = new GradientColorKey[4];
                gck[0].color = HairPlatinum;
                gck[0].time = 0.0f;
                gck[1].color = new Color32(255, 194, 71, 255);
                gck[1].time = 0.41f;
                gck[2].color = new Color32(255, 165, 51, 255);
                gck[2].time = 0.65f;
                gck[3].color = new Color32(255, 64, 0, 255);
                gck[3].time = 0.97f;
                GradientAlphaKey[] gak = new GradientAlphaKey[2];
                gak[0].alpha = 1f;
                gak[0].time = 0.0f;
                gak[1].alpha = 1f;
                gak[1].time = 1f;
                gradient_mel2.SetKeys(gck, gak);

                gck[0].color = HairPlatinum;
                gck[0].time = 0.0f;
                gck[1].color = new Color32(184, 139, 96, 255);
                gck[1].time = 0.33f;
                gck[2].color = new Color32(110, 59, 13, 255);
                gck[2].time = 0.66f;
                gck[3].color = new Color32(30, 14, 0, 255);
                gck[3].time = 0.92f;
                gradient_mel1.SetKeys(gck, gak);

                var color = gradient_mel1.Evaluate(face.melanin1);

                color *= gradient_mel2.Evaluate(face.melanin2);
                // Aging
                {
                    // age to become gray as float
                    float ageFloat = pawn.ageTracker.AgeBiologicalYearsFloat / 100;
                    float agingBeginGreyFloat = Rand.Range(0.4f, 0.7f);

                    float greySpan = Rand.Range(0.07f, 0.15f);

                    float greyness = 0f;

                    if (ageFloat > agingBeginGreyFloat)
                    {
                        greyness = Mathf.InverseLerp(agingBeginGreyFloat, agingBeginGreyFloat + greySpan, ageFloat);
                    }

                    // Soften the greyness
                    greyness *= 0.9f;

                    // Even more - melanin
                    if (PawnSkinColors.IsDarkSkin(pawn.story.SkinColor))
                    {
                        greyness *= Rand.Range(0.5f, 0.9f);
                    }
                     Log.Message(pawn.ToString());
                     Log.Message(ageFloat.ToString());
                     Log.Message(agingBeginGreyFloat.ToString());
                     Log.Message(greySpan.ToString());
                     Log.Message(greyness.ToString());

                    if (Rand.Value < 0.04f)
                    {
                        float rand = Rand.Value;
                        if (rand < 0.1f) pawn.story.hairColor = HairDarkPurple;
                        else if (rand < 0.2f) pawn.story.hairColor = HairBlueSteel;
                        else if (rand < 0.3f) pawn.story.hairColor = HairBurgundyBistro;
                        else if (rand < 0.4f) pawn.story.hairColor = HairGreenGrape;
                        else if (rand < 0.5f) pawn.story.hairColor = HairMysticTurquois;
                        else if (rand < 0.6f) pawn.story.hairColor = HairPinkPearl;
                        else if (rand < 0.7f) pawn.story.hairColor = HairPurplePassion;
                        else if (rand < 0.8f) pawn.story.hairColor = HairRosaRosa;
                        else if (rand < 0.9f) pawn.story.hairColor = HairRubyRed;
                        else pawn.story.hairColor = HairUltraViolet;
                    }
                    else
                    {
                        pawn.story.hairColor = Color.Lerp(color, new Color(0.91f, 0.91f, 0.91f), greyness);
                    }
                    face.sameBeardColor = Rand.Value > 0.2f;

                    face.HairColorOrg = color;

                    if (face.sameBeardColor)
                    {
                        face.BeardColor = PawnHairColors_PostFix.DarkerBeardColor(color);
                    }
                    else
                    {
                        var color2 = gradient_mel1.Evaluate(face.melanin1 + Rand.Range(-0.2f, 0.2f));

                        color2 *= gradient_mel2.Evaluate(face.melanin2 + Rand.Range(-0.2f, 0.2f));

                        face.BeardColor = color2;
                    }
                }

            }

        }

        public static Color RandomBeardColor()
        {
            float value = Rand.Value;
            Color tempColor = new Color();

            // dark hair
            if (value > 0.15f)
            {
                tempColor = Color.Lerp(PawnHairColors_PostFix.HairPlatinum, PawnHairColors_PostFix.HairYellowBlonde, Rand.Value);
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairTerraCotta, Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairMediumDarkBrown, Rand.Range(0.3f, 0.7f));
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairMidnightBlack, Rand.Range(0.1f, 0.8f));
            }

            // brown hair
            else
            {
                tempColor = Color.Lerp(PawnHairColors_PostFix.HairPlatinum, PawnHairColors_PostFix.HairYellowBlonde, Rand.Value);
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairTerraCotta, Rand.Range(0f, 0.6f));
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairMediumDarkBrown, Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairMidnightBlack, Rand.Range(0f, 0.5f));
            }

            return tempColor;
        }

        public static float GetRandomMelaninSimilarTo(float value, float clampMin = 0f, float clampMax = 1f)
        {
            return Mathf.Clamp01(Mathf.Clamp(Rand.Gaussian(value, 0.05f), clampMin, clampMax));
        }

        public static float GetRandomChildHairColor(float fatherMelanin, float motherMelanin)
        {
            float clampMin = Mathf.Min(fatherMelanin, motherMelanin);
            float clampMax = Mathf.Max(fatherMelanin, motherMelanin);
            float value = (fatherMelanin + motherMelanin) / 2f;
            return GetRandomMelaninSimilarTo(value, clampMin, clampMax);
        }
    }
}
