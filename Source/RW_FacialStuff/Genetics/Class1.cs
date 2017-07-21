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

        public static Color HairYellowBlonde = new Color32(255, 203, 89, 255);

        public static Color HairTerraCotta = new Color32(185, 49, 4, 255);

        public static Color HairDarkBrown = new Color32(79, 47, 17, 255);

        public static Color HairMidnightBlack = new Color32(30, 30, 30, 255);

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
                    melanin1 = Rand.Range(pawn.story.melanin / 2, 1f);
                    melanin2 = Rand.Range(pawn.story.melanin / 4, 1f);
                }
            }
            //  Log.Message(
            //      pawn + " - " + melanin + " - " + face.melanin1 + " - " + face.melanin2 + " - " + mother?.melanin1
            //      + mother?.melanin2 + father?.melanin1 + father?.melanin2);

            Color col = new Color(0.9f, 0.9f, 0.9f);

            // Build gradients
            GradientColorKey[] gck = new GradientColorKey[4];
            gck[0].color = col;
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

            gck[0].color = col;
            gck[0].time = 0.0f;
            gck[1].color = new Color32(230, 194, 161, 255);
            gck[1].time = 0.1f;
            gck[2].color = new Color32(110, 59, 13, 255);
            gck[2].time = 0.6f;
            gck[3].color = new Color32(30, 14, 0, 255);
            gck[3].time = 0.92f;
            gradient_mel1.SetKeys(gck, gak);

            var color = gradient_mel1.Evaluate(face.melanin1);

            color *= gradient_mel2.Evaluate(face.melanin2);
            // Aging
            {
                // age to become gray as float
                float ageFloat = pawn.ageTracker.AgeBiologicalYearsFloat / 100;
                float agingBeginGreyFloat = Rand.Range(0.35f, 0.7f);

                float greySpan = Rand.Range(0.07f, 0.25f);

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
                    greyness *= Rand.Range(0.4f, 0.8f);
                }
                // Log.Message(ageFloat.ToString());
                // Log.Message(agingBeginGreyFloat.ToString());
                // Log.Message(greySpan.ToString());
                // Log.Message(greyness.ToString());

                pawn.story.hairColor = Color.Lerp(color, col, greyness);
            }
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
