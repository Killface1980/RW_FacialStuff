namespace FacialStuff.Genetics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public static class HairMelanin
    {

        #region Public Fields

        public static List<Color> ArtificialHairColors;

        public static List<Color> NaturalHairColors;

        #endregion Public Fields

        #region Private Fields

        private static readonly GradientColorKey[] EuMelaninGradientColorKeys = new GradientColorKey[4];
        private static readonly GradientColorKey[] PhyoMelaninGradientColorKeys = new GradientColorKey[5];
        private static readonly GradientAlphaKey[] AlphaKeys = new GradientAlphaKey[2];
        private static readonly Gradient GradientEuMelanin = new Gradient();
        private static readonly Gradient GradientPheoMelanin = new Gradient();

        private static readonly Color HairBlueSteel = new Color32(57, 115, 199, 255);
        private static readonly Color HairBurgundyBistro = new Color32(206, 38, 58, 255);
        private static readonly Color HairDarkBrown = new Color32(79, 47, 17, 255);
        private static readonly Color HairGreenGrape = new Color32(124, 189, 14, 255);
        private static readonly Color HairMidnightBlack = new Color32(30, 30, 30, 255);
        private static readonly Color HairMysticTurquois = new Color32(71, 191, 165, 255);
        private static readonly Color HairPlatinum = new Color32(255, 245, 226, 255);
        private static readonly Color HairPurplePassion = new Color32(145, 50, 191, 255);
        private static readonly Color HairRosaRosa = new Color32(215, 168, 255, 255);
        private static readonly Color HairTerraCotta = new Color32(185, 49, 4, 255);
        private static readonly Color HairUltraViolet = new Color32(191, 53, 132, 255);
        private static readonly Color HairYellowBlonde = new Color32(255, 203, 89, 255);

        #endregion Private Fields

        #region Public Constructors

        static HairMelanin()
        {             // Build gradients
            AlphaKeys[0].alpha = 1f;
            AlphaKeys[0].time = 0.0f;
            AlphaKeys[1].alpha = 1f;
            AlphaKeys[1].time = 1f;


            EuMelaninGradientColorKeys[0].color = HairPlatinum;
            EuMelaninGradientColorKeys[0].time = 0.0f;
            EuMelaninGradientColorKeys[1].color = new Color32(139, 108, 66, 255);
            EuMelaninGradientColorKeys[1].time = 0.5f;
            EuMelaninGradientColorKeys[2].color = new Color32(91, 60, 17, 255);
            EuMelaninGradientColorKeys[2].time = 0.75f;
            EuMelaninGradientColorKeys[3].color = new Color32(47, 30, 14, 255);
            EuMelaninGradientColorKeys[3].time = 1f;
            // EuMelaninGradientColorKeys[4].color = new Color32(0, 0, 0, 255);
            // EuMelaninGradientColorKeys[4].time = 1f;
            GradientEuMelanin.SetKeys(EuMelaninGradientColorKeys, AlphaKeys);

            PhyoMelaninGradientColorKeys[0].color = HairPlatinum;
            PhyoMelaninGradientColorKeys[0].time = 0.0f;
            PhyoMelaninGradientColorKeys[1].color = new Color32(226, 188, 116, 255);
            PhyoMelaninGradientColorKeys[1].time = 0.25f;
            PhyoMelaninGradientColorKeys[2].color = new Color32(231, 168, 84, 255);
            PhyoMelaninGradientColorKeys[2].time = 0.5f;
            PhyoMelaninGradientColorKeys[3].color = new Color32(173, 79, 9, 255);
            PhyoMelaninGradientColorKeys[3].time = 0.75f;
            PhyoMelaninGradientColorKeys[4].color = new Color32(157, 54, 0, 255);
            PhyoMelaninGradientColorKeys[4].time = 1f;

            GradientPheoMelanin.SetKeys(PhyoMelaninGradientColorKeys, AlphaKeys);

            NaturalHairColors = new List<Color>
                                    {
                                        HairPlatinum,
                                        HairYellowBlonde,
                                        HairTerraCotta,
                                        HairDarkBrown,
                                        HairMidnightBlack
                                    };

            ArtificialHairColors = new List<Color>()
                                       {
                                           HairGreenGrape,
                                           HairMysticTurquois,
                                           HairBlueSteel,
                                           HairPurplePassion,
                                           HairRosaRosa,
                                           HairUltraViolet,
                                           HairBurgundyBistro
                                       };

        }

        #endregion Public Constructors

        #region Public Methods

        public static HairDNA GenerateHairMelaninAndCuticula(Pawn pawn)
        {
            Color beardColor;

            HairColorRequest hair = new HairColorRequest(0f, 0f, 0f, 0f);

            CompFace compFace = pawn.TryGetComp<CompFace>();

            SetInitialMelaninLevels(pawn, ref hair);

            // Log.Message(
            // pawn + " - " + melanin + " - " + face.euMelanin + " - " + face.pheoMelanin + " - " + mother?.euMelanin
            // + mother?.pheoMelanin + father?.euMelanin + father?.pheoMelanin);

            // Aging
            float ageFloat = pawn.ageTracker.AgeBiologicalYearsFloat / 100;
            float agingBeginGreyFloat = Rand.Range(0.35f, 0.5f);

            agingBeginGreyFloat += pawn.story.melanin * 0.1f + hair.EuMelanin * 0.05f + hair.PheoMelanin * 0.05f;

            float greySpan = Rand.Range(0.07f, 0.2f);

            greySpan += hair.EuMelanin * 0.15f;
            greySpan += pawn.story.melanin * 0.25f;
            if (ageFloat > agingBeginGreyFloat)
            {
                hair.Greyness = Mathf.InverseLerp(agingBeginGreyFloat, agingBeginGreyFloat + greySpan, ageFloat);
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

            Color hairColor = GetHairColor(hair);
            Color hairColorOrg = hairColor;


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

                hairColor = Color.Lerp(hairColor, color2, Rand.Range(0.66f, 1f));
            }


            if (compFace.pawnFace.HasSameBeardColor)
            {
                beardColor = FacialGraphics.DarkerBeardColor(hairColor);
            }
            else
            {
                Color color2 = GradientEuMelanin.Evaluate(hair.EuMelanin + Rand.Range(-0.2f, 0.2f));

                color2 *= GradientPheoMelanin.Evaluate(hair.PheoMelanin + Rand.Range(-0.2f, 0.2f));

                beardColor = Color.Lerp(color2, new Color(0.91f, 0.91f, 0.91f), hair.Greyness * Rand.Value);
            }

            HairDNA dna = new HairDNA
            {
                EuMelanin = hair.EuMelanin,
                PheoMelanin = hair.PheoMelanin,
                Cuticula = hair.Cuticula,
                Greyness = hair.Greyness,
                HairColor = hairColor,
                HairColorOrg = hairColorOrg,
                BeardColor = beardColor
            };

            return dna;
        }

        private static void SetInitialMelaninLevels(Pawn pawn, ref HairColorRequest hair)
        {
            bool motherNull = false;
            bool fatherNull = false;

            PawnFace motherPawnFace = new PawnFace();
            PawnFace fatherPawnFace = new PawnFace();

            CheckForMother(pawn, ref motherNull, ref motherPawnFace);

            CheckForFather(pawn, ref fatherNull, ref fatherPawnFace);

            if (motherNull && fatherNull)
            {
                // Check for relatives, else randomize
                if (!MelaninSetRelationsByBlood(pawn, ref hair))
                {
                    hair.EuMelanin = Rand.Range(pawn.story.melanin * 0.75f, 1f);
                    hair.PheoMelanin = Rand.Range(pawn.story.melanin / 2, 1f);
                    hair.Cuticula = Rand.Range(0.75f, 1.25f);
                }
            }

            if (!motherNull && !fatherNull && motherPawnFace.IsOptimized && fatherPawnFace.IsOptimized)
            {
                hair.EuMelanin = GetRandomChildHairColor(motherPawnFace.EuMelanin, fatherPawnFace.EuMelanin);
                hair.PheoMelanin = GetRandomChildHairColor(motherPawnFace.PheoMelanin, fatherPawnFace.PheoMelanin);
                hair.Cuticula = GetRandomChildHairColor(motherPawnFace.Cuticula, fatherPawnFace.Cuticula);
            }
            else if (!motherNull && motherPawnFace.IsOptimized)
            {
                hair.EuMelanin = GetRandomMelaninSimilarTo(motherPawnFace.EuMelanin);
                hair.PheoMelanin = GetRandomMelaninSimilarTo(motherPawnFace.PheoMelanin);
                hair.Cuticula = GetRandomMelaninSimilarTo(motherPawnFace.Cuticula);
            }
            else if (!fatherNull && fatherPawnFace.IsOptimized)
            {
                hair.EuMelanin = GetRandomMelaninSimilarTo(fatherPawnFace.EuMelanin);
                hair.PheoMelanin = GetRandomMelaninSimilarTo(fatherPawnFace.PheoMelanin);
                hair.Cuticula = GetRandomMelaninSimilarTo(fatherPawnFace.Cuticula);
            }
        }

        private static bool MelaninSetRelationsByBlood(Pawn pawn, ref HairColorRequest hair)
        {
            if (pawn.relations.FamilyByBlood.Any())
            {
                Pawn relPawn = pawn.relations.FamilyByBlood.FirstOrDefault(x => x.TryGetComp<CompFace>().pawnFace.IsOptimized);
                if (relPawn != null)
                {
                    CompFace relatedPawn = relPawn.TryGetComp<CompFace>();

                    float melaninx1 = relatedPawn.pawnFace.EuMelanin;
                    float melaninx2 = relatedPawn.pawnFace.PheoMelanin;
                    float cuticulax = relatedPawn.pawnFace.Cuticula;
                    hair.EuMelanin = GetRandomMelaninSimilarTo(melaninx1);
                    hair.PheoMelanin = GetRandomMelaninSimilarTo(melaninx2);
                    hair.Cuticula = GetRandomMelaninSimilarTo(cuticulax);
                    return false;
                }
            }
            return true;
        }

        private static void CheckForFather(Pawn pawn, ref bool fatherNull, ref PawnFace fatherPawnFace)
        {
            if (pawn.GetFather() == null)
            {
                fatherNull = true;
            }
            else
            {
                CompFace fatherComp = pawn.GetFather().TryGetComp<CompFace>();
                fatherPawnFace = fatherComp.pawnFace;
            }
        }

        private static void CheckForMother(Pawn pawn, ref bool motherNull, ref PawnFace motherPawnFace)
        {
            if (pawn.GetMother() == null)
            {
                motherNull = true;
            }
            else
            {
                CompFace motherComp = pawn.GetMother().TryGetComp<CompFace>();
                motherPawnFace = motherComp.pawnFace;
            }
        }

        public static Color GetHairColor(HairColorRequest hairColorRequest)
        {
            Color color = GradientEuMelanin.Evaluate(hairColorRequest.EuMelanin);

            color *= GradientPheoMelanin.Evaluate(hairColorRequest.PheoMelanin);

            Color.RGBToHSV(color, out float h, out float s, out float v);
            s *= hairColorRequest.Cuticula;

            color = Color.HSVToRGB(h, s, v);

            // limit the greyness to 70 %, else it's too much
            color = Color.Lerp(color, new Color(0.86f, 0.86f, 0.86f), Mathf.Min(hairColorRequest.Greyness, 0.7f));


            return color;
        }

        #endregion Public Methods

        #region Private Methods

        private static float GetRandomChildHairColor(float fatherMelanin, float motherMelanin)
        {
            float clampMin = Mathf.Min(fatherMelanin, motherMelanin);
            float clampMax = Mathf.Max(fatherMelanin, motherMelanin);
            float value = (fatherMelanin + motherMelanin) / 2f;
            return GetRandomMelaninSimilarTo(value, clampMin, clampMax);
        }

        private static float GetRandomMelaninSimilarTo(float value, float clampMin = 0f, float clampMax = 1f)
        {
            return Mathf.Clamp01(Mathf.Clamp(Rand.Gaussian(value, 0.05f), clampMin, clampMax));
        }

        #endregion Private Methods

        // Deactivated for now, as there's no way to get the pawn's birth biome and full history, considered mostly racist
        /*
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
                factionMelanin = GetRandomMelaninSimilarTo(mother.FactionMelanin);
            }
            else if (!fatherNull && father.IsSkinDNAoptimized)
            {
                factionMelanin = GetRandomMelaninSimilarTo(father.FactionMelanin);
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

            // Log.Message(
            // pawn + " - " + melanin + " - " + face.euMelanin + " - " + face.pheoMelanin + " - " + mother?.euMelanin
            // + mother?.pheoMelanin + father?.euMelanin + father?.pheoMelanin);
        }
        */

    }
}