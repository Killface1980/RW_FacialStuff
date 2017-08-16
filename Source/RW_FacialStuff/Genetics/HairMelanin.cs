namespace FacialStuff.Genetics
{
    using FacialStuff.Detouring;
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;

    public static class HairMelanin
    {
        public static Color HairPlatinum = new Color32(255, 245, 226, 255);

        public static Color HairYellowBlonde = new Color32(255, 203, 89, 255);

        public static Color HairTerraCotta = new Color32(185, 49, 4, 255);

        public static Color HairDarkBrown = new Color32(79, 47, 17, 255);

        public static Color HairMidnightBlack = new Color32(30, 30, 30, 255);

        private static readonly Color HairGreenGrape = new Color32(124, 189, 14, 255);

        private static readonly Color HairMysticTurquois = new Color32(71, 191, 165, 255);

        private static readonly Color HairBlueSteel = new Color32(57, 115, 199, 255);

        private static readonly Color HairPurplePassion = new Color32(145, 50, 191, 255);

        private static readonly Color HairRosaRosa = new Color32(215, 168, 255, 255);

        private static readonly Color HairUltraViolet = new Color32(191, 53, 132, 255);

        private static readonly Color HairBurgundyBistro = new Color32(206, 38, 58, 255);

        public static List<Color> NaturalHairColors =
            new List<Color> { HairPlatinum, HairYellowBlonde, HairTerraCotta, HairDarkBrown, HairMidnightBlack };

        public static List<Color> ArtificialHairColors =
            new List<Color>
                {
                    HairGreenGrape,
                    HairMysticTurquois,
                    HairBlueSteel,
                    HairPurplePassion,
                    HairRosaRosa,
                    HairUltraViolet,
                    HairBurgundyBistro
                };

        private static readonly Gradient GradientPheoMelanin = new Gradient();

        private static readonly Gradient GradientEuMelanin = new Gradient();

        public static void GenerateHairMelaninAndCuticula(
            Pawn pawn,
            CompFace face,
            out float euMelanin,
            out float pheoMelanin,
            out float cuticula)
        {
            euMelanin = pheoMelanin = cuticula = 0f;
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

            if (!motherNull && mother.IsDNAoptimized && !fatherNull && father.IsDNAoptimized)
            {
                euMelanin = GetRandomChildHairColor(mother.EuMelanin, father.EuMelanin);
                pheoMelanin = GetRandomChildHairColor(mother.PheoMelanin, father.PheoMelanin);
                cuticula = GetRandomChildHairColor(mother.Cuticula, father.Cuticula);
            }
            else if (!motherNull && mother.IsDNAoptimized)
            {
                euMelanin = GetRandomMelaninSimilarTo(mother.EuMelanin, 0f, 1f);
                pheoMelanin = GetRandomMelaninSimilarTo(mother.PheoMelanin, 0f, 1f);
                cuticula = GetRandomMelaninSimilarTo(mother.Cuticula, 0f, 1f);
            }
            else if (!fatherNull && father.IsDNAoptimized)
            {
                euMelanin = GetRandomMelaninSimilarTo(father.EuMelanin, 0f, 1f);
                pheoMelanin = GetRandomMelaninSimilarTo(father.PheoMelanin, 0f, 1f);
                cuticula = GetRandomMelaninSimilarTo(father.Cuticula, 0f, 1f);
            }
            else
            {
                // if (motherNull && fatherNull)
                bool flag = true;
                if (pawn.relations.FamilyByBlood.Any())
                {
                    Pawn relPawn =
                        pawn.relations.FamilyByBlood.FirstOrDefault(x => x.TryGetComp<CompFace>().IsDNAoptimized);
                    if (relPawn != null)
                    {
                        CompFace relatedPawn = relPawn.TryGetComp<CompFace>();

                        float melaninx1 = relatedPawn.EuMelanin;
                        float melaninx2 = relatedPawn.PheoMelanin;
                        float cuticulax = relatedPawn.Cuticula;
                        euMelanin = GetRandomMelaninSimilarTo(melaninx1);
                        pheoMelanin = GetRandomMelaninSimilarTo(melaninx2);
                        cuticula = GetRandomMelaninSimilarTo(cuticulax);
                        flag = false;
                    }
                }

                if (flag)
                {
                    euMelanin = Rand.Range(pawn.story.melanin * 0.75f, Mathf.Min(pawn.story.melanin * 1.5f + 0.1f, 1f));
                    pheoMelanin = Rand.Range(pawn.story.melanin / 2, 1f);
                    cuticula = Rand.Range(0.5f, 1.5f);
                }
            }

            // Log.Message(
            // pawn + " - " + melanin + " - " + face.euMelanin + " - " + face.pheoMelanin + " - " + mother?.euMelanin
            // + mother?.pheoMelanin + father?.euMelanin + father?.pheoMelanin);

            // Check if the player has already set a custom color, else dna!
            if (pawn.story.hairColor == face.HairColorOrg)
            {
                // Build gradients
                GradientAlphaKey[] gak = new GradientAlphaKey[2];
                gak[0].alpha = 1f;
                gak[0].time = 0.0f;
                gak[1].alpha = 1f;
                gak[1].time = 1f;

                GradientColorKey[] phyo = new GradientColorKey[5];
                GradientColorKey[] eu = new GradientColorKey[5];

                eu[0].color = HairPlatinum;
                eu[0].time = 0.0f;
                eu[1].color = new Color32(139, 108, 66, 255);
                eu[1].time = 0.6f;
                eu[2].color = new Color32(91, 60, 17, 255);
                eu[2].time = 0.75f;
                eu[3].color = new Color32(47, 30, 14, 255);
                eu[3].time = 0.9f;
                eu[4].color = new Color32(0, 0, 0, 255);
                eu[4].time = 1f;
                GradientEuMelanin.SetKeys(eu, gak);

                phyo[0].color = HairPlatinum;
                phyo[0].time = 0.0f;
                phyo[1].color = new Color32(226, 188, 116, 255);
                phyo[1].time = 0.25f;
                phyo[2].color = new Color32(231, 168, 84, 255);
                phyo[2].time = 0.5f;
                phyo[3].color = new Color32(173, 79, 9, 255);
                phyo[3].time = 0.75f;
                phyo[4].color = new Color32(157, 54, 0, 255);
                phyo[4].time = 0.95f;

                GradientPheoMelanin.SetKeys(phyo, gak);

                Color color = GradientEuMelanin.Evaluate(face.EuMelanin);

                color *= GradientPheoMelanin.Evaluate(face.PheoMelanin);
                {
                    // Aging
                    // age to become gray as float
                    float ageFloat = pawn.ageTracker.AgeBiologicalYearsFloat / 100;
                    float agingBeginGreyFloat = Rand.Range(0.35f, 0.5f);

                    agingBeginGreyFloat += pawn.story.melanin * 0.1f + euMelanin * 0.05f + pheoMelanin * 0.05f;

                    float greySpan = Rand.Range(0.07f, 0.2f);

                    greySpan += euMelanin * 0.15f;
                    greySpan += pawn.story.melanin * 0.25f;
                    float greyness = 0f;

                    if (ageFloat > agingBeginGreyFloat)
                    {
                        greyness = Mathf.InverseLerp(agingBeginGreyFloat, agingBeginGreyFloat + greySpan, ageFloat);
                    }

                    // Soften the greyness
                    // greyness *= 0.95f;

                    // Even more - melanin
                    // if (PawnSkinColors.IsDarkSkin(pawn.story.SkinColor))
                    // {
                    // greyness *= Rand.Range(0.5f, 0.9f);
                    // }
                    // Log.Message(pawn.ToString());
                    // Log.Message(ageFloat.ToString());
                    // Log.Message(agingBeginGreyFloat.ToString());
                    // Log.Message(greySpan.ToString());
                    // Log.Message(greyness.ToString());

                    // Special hair colors
                    float factionColor = Rand.Value;
                    float limit = 0.98f;
                    if (pawn.Faction.def.techLevel > TechLevel.Industrial)
                    {
                        limit *= pawn.gender == Gender.Female ? 0.7f : 0.9f;

                        float techMod = (pawn.Faction.def.techLevel - TechLevel.Industrial) / 5f;
                        SimpleCurve ageCure = new SimpleCurve { { 0.1f, 1f }, { 0.25f, 1f - techMod }, { 0.6f, 0.9f } };
                        limit *= ageCure.Evaluate(pawn.ageTracker.AgeBiologicalYears / 100f);
                    }

                    if (factionColor > limit && pawn.ageTracker.AgeBiologicalYearsFloat > 16)
                    {
                        Color color2;
                        float rand = Rand.Value;
                        if (rand < 0.15f)
                        {
                            color2 = HairBlueSteel;
                        }
                        else if (rand < 0.3f)
                        {
                            color2 = HairBurgundyBistro;
                        }
                        else if (rand < 0.45f)
                        {
                            color2 = HairGreenGrape;
                        }
                        else if (rand < 0.6f)
                        {
                            color2 = HairMysticTurquois;
                        }
                        else if (rand < 0.75f)
                        {
                            color2 = HairPurplePassion;
                        }
                        else if (rand < 0.9f)
                        {
                            color2 = HairRosaRosa;
                        }
                        else
                        {
                            color2 = HairUltraViolet;
                        }

                        Color.RGBToHSV(color2, out float h, out float s, out float v);
                        s *= cuticula;

                        color2 = Color.HSVToRGB(h, s, v);

                        pawn.story.hairColor = Color.Lerp(color, color2, Rand.Range(0.66f, 1f));
                    }
                    else
                    {
                        Color.RGBToHSV(color, out float h, out float s, out float v);
                        s *= cuticula;

                        color = Color.HSVToRGB(h, s, v);

                        pawn.story.hairColor = Color.Lerp(color, new Color(0.86f, 0.86f, 0.86f), greyness);
                    }

                    face.HasSameBeardColor = Rand.Value > 0.3f;

                    face.HairColorOrg = color;

                    if (face.HasSameBeardColor)
                    {
                        face.BeardColor = FacialGraphics.DarkerBeardColor(pawn.story.hairColor);
                    }
                    else
                    {
                        Color color2 = GradientEuMelanin.Evaluate(face.EuMelanin + Rand.Range(-0.2f, 0.2f));

                        color2 *= GradientPheoMelanin.Evaluate(face.PheoMelanin + Rand.Range(-0.2f, 0.2f));

                        face.BeardColor = Color.Lerp(color2, new Color(0.91f, 0.91f, 0.91f), greyness * Rand.Value);
                    }
                }
            }
        }

        // Deactivated for now, as there's no way to get the pawn's birth biome and full history, considered mostly racist
        public static void SkinGenetics(Pawn pawn, CompFace face, out float factionMelanin)
        {
            factionMelanin = pawn.story.melanin;
            bool isTribal = pawn.Faction?.def == FactionDefOf.Tribe || pawn.Faction?.def == FactionDefOf.PlayerTribe;
            bool isSpacer = pawn.Faction?.def == FactionDefOf.Spacer || pawn.Faction?.def == FactionDefOf.SpacerHostile;

            if (face == null)
            {
                return;
            }

            face.MelaninOrg = pawn.story.melanin;
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

            bool flag = true;

            if (!motherNull && mother.IsSkinDNAoptimized && !fatherNull && father.IsSkinDNAoptimized)
            {
                factionMelanin = GetRandomChildHairColor(mother.FactionMelanin, father.FactionMelanin);
            }
            else if (!motherNull && mother.IsSkinDNAoptimized)
            {
                factionMelanin = GetRandomMelaninSimilarTo(mother.FactionMelanin, 0f, 1f);
            }
            else if (!fatherNull && father.IsSkinDNAoptimized)
            {
                factionMelanin = GetRandomMelaninSimilarTo(father.FactionMelanin, 0f, 1f);
            }
            else
            {
                // if (motherNull && fatherNull)
                if (pawn.relations.FamilyByBlood.Any())
                {
                    Pawn relPawn =
                        pawn.relations.FamilyByBlood.FirstOrDefault(x => x.TryGetComp<CompFace>().IsSkinDNAoptimized);
                    if (relPawn != null)
                    {
                        CompFace relatedPawn = relPawn.TryGetComp<CompFace>();

                        float melaninx1 = relatedPawn.FactionMelanin;
                        factionMelanin = GetRandomMelaninSimilarTo(melaninx1);
                        flag = false;
                    }
                }

                if (flag)
                {
                    if (isTribal)
                    {
                        SimpleCurve curve =
                            new SimpleCurve
                                {
                                    new CurvePoint(0f, 0f),
                                    new CurvePoint(0.2f, 0.5f),
                                    new CurvePoint(1f, 1f)
                                };
                        factionMelanin = curve.Evaluate(pawn.story.melanin);
                    }

                    if (isSpacer)
                    {
                        SimpleCurve curve =
                            new SimpleCurve
                                {
                                    new CurvePoint(0f, 0.0f),
                                    new CurvePoint(0.5f, 0.25f),
                                    new CurvePoint(1f, 1f)
                                };
                        factionMelanin = curve.Evaluate(pawn.story.melanin);
                    }
                }
            }

            if (Controller.settings.UseDNAByFaction)
            {
                if (Math.Abs(pawn.story.melanin - factionMelanin) > 0.01f)
                {
                    pawn.story.melanin = factionMelanin;
                }
            }

            face.IsDNAoptimized = false;

            // Log.Message(
            // pawn + " - " + melanin + " - " + face.euMelanin + " - " + face.pheoMelanin + " - " + mother?.euMelanin
            // + mother?.pheoMelanin + father?.euMelanin + father?.pheoMelanin);
        }

        public static Color RandomBeardColor()
        {
            float value = Rand.Value;
            Color tempColor = new Color();

            // dark hair
            if (value > 0.15f)
            {
                tempColor = Color.Lerp(
                    PawnHairColors_PostFix.HairPlatinum,
                    PawnHairColors_PostFix.HairYellowBlonde,
                    Rand.Value);
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairTerraCotta, Rand.Range(0.3f, 1f));
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairMediumDarkBrown, Rand.Range(0.3f, 0.7f));
                tempColor = Color.Lerp(tempColor, PawnHairColors_PostFix.HairMidnightBlack, Rand.Range(0.1f, 0.8f));
            }

            // brown hair
            else
            {
                tempColor = Color.Lerp(
                    PawnHairColors_PostFix.HairPlatinum,
                    PawnHairColors_PostFix.HairYellowBlonde,
                    Rand.Value);
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