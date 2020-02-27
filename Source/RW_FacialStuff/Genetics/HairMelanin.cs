using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff.Genetics
{
    public static class HairMelanin
    {
        public static readonly List<Color> ArtificialHairColors;

        public static readonly FloatRange GreyRange = new FloatRange(0f, 0.95f);

        private static readonly Gradient GradientEuMelanin;

        private static readonly Gradient GradientPheoMelanin;

        private static readonly Color GrayHair = new Color(0.9f, 0.9f, 0.9f);

        static HairMelanin()
        {
            // Build gradients
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time  = 0.0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time  = 1f;

            GradientColorKey[] phyoMelaninGradientColorKeys = new GradientColorKey[4];
            phyoMelaninGradientColorKeys[0].color = new Color32(255, 245, 226, 255);
            phyoMelaninGradientColorKeys[0].time  = 0.0f;
            phyoMelaninGradientColorKeys[1].color = new Color32(226, 188, 116, 255);
            phyoMelaninGradientColorKeys[1].time  = 0.5f;
            phyoMelaninGradientColorKeys[2].color = new Color32(210, 119, 44, 255);
            phyoMelaninGradientColorKeys[2].time  = 0.9f;
            phyoMelaninGradientColorKeys[3].color = new Color32(216, 25, 1, 255);
            phyoMelaninGradientColorKeys[3].time  = 1f;

            GradientPheoMelanin = new Gradient();
            GradientPheoMelanin.SetKeys(phyoMelaninGradientColorKeys, alphaKeys);

            GradientColorKey[] euMelaninGradientColorKeys = new GradientColorKey[4];
            euMelaninGradientColorKeys[0].color = Color.white;
            euMelaninGradientColorKeys[0].time  = 0.0f;

            // euMelaninGradientColorKeys[1].color = new Color32(139, 108, 66, 255);
            // euMelaninGradientColorKeys[1].time = 0.5f;
            euMelaninGradientColorKeys[1].color = new Color(0.5176471f, 0.3254902f, 0.184313729f);
            euMelaninGradientColorKeys[1].time  = 0.7f;
            euMelaninGradientColorKeys[2].color = new Color(0.3f, 0.2f, 0.1f);
            euMelaninGradientColorKeys[2].time  = 0.85f;
            euMelaninGradientColorKeys[3].color = new Color(0.2f, 0.2f, 0.2f);
            euMelaninGradientColorKeys[3].time  = 1f;

            // EuMelaninGradientColorKeys[4].color = new Color32(0, 0, 0, 255);
            // EuMelaninGradientColorKeys[4].time = 1f;
            GradientEuMelanin = new Gradient();
            GradientEuMelanin.SetKeys(euMelaninGradientColorKeys, alphaKeys);

            ArtificialHairColors = new List<Color>
                                   {
                                   new Color32(37,  136, 0,   255),
                                   new Color32(124, 189, 14,  255),
                                   new Color32(71,  191, 165, 255),
                                   new Color32(57,  144, 199, 255),
                                   new Color32(25,  70,  136, 255),
                                   new Color32(215, 168, 255, 255),
                                   new Color32(145, 50,  191, 255),
                                   new Color32(191, 35,  124, 255)
                                   };
        }

        public static Color ShuffledBeardColor(Color color)
        {
            Color shuffle = new Color(Rand.Range(0.9f, 1.1f), Rand.Range(0.9f, 1.1f), Rand.Range(0.9f, 1.1f));

            return color * shuffle;
        }

        public static HairDNA GenerateHairMelaninAndCuticula(
        [NotNull] Pawn pawn,
        bool           sameBeardColor,
        bool           ignoreRelative = false)
        {
            Color beardColor;
            SetInitialHairMelaninLevels(pawn, out HairColorRequest hair, ignoreRelative);

            // Log.Message(
            // pawn + " - " + melanin + " - " + face.euMelanin + " - " + face.pheoMelanin + " - " + mother?.euMelanin
            // + mother?.pheoMelanin + father?.euMelanin + father?.pheoMelanin);

            // Aging
            if (pawn.ageTracker != null)
            {
                float ageFloat            = pawn.ageTracker.AgeBiologicalYearsFloat / 100;
                float agingBeginGreyFloat = Rand.Range(0.35f, 0.5f);

                if (pawn.story != null)
                {
                    agingBeginGreyFloat +=
                        pawn.story.melanin * 0.1f + hair.EuMelanin * 0.05f + hair.PheoMelanin * 0.05f;

                    float greySpan = Rand.Range(0.07f, 0.2f);

                    greySpan += hair.EuMelanin * 0.15f;
                    greySpan += pawn.story.melanin * 0.25f;
                    if (ageFloat > agingBeginGreyFloat)
                    {
                        hair.Greyness =
                            Mathf.InverseLerp(agingBeginGreyFloat, agingBeginGreyFloat + greySpan, ageFloat);
                    }
                }
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

            // Special hair colors
            float factionColor = Rand.Value;
            float limit        = 0.98f;
            if (pawn.GetCompFace(out CompFace compFace))
            {
                Faction faction = compFace.OriginFaction ?? Faction.OfPlayer;
                if (faction?.def != null && (faction.def.techLevel > TechLevel.Industrial))
                {
                    limit *= pawn.gender == Gender.Female ? 0.7f : 0.9f;

                    float       techMod = (faction.def.techLevel - TechLevel.Industrial) / 5f;
                    SimpleCurve ageCure = new SimpleCurve {{0.1f, 1f}, {0.25f, 1f - techMod}, {0.6f, 0.9f}};
                    if (pawn.ageTracker != null) limit *= ageCure.Evaluate(pawn.ageTracker.AgeBiologicalYears / 100f);
                }
            }

            // if (pawn.story.hairDef.hairTags.Contains("Punk"))
            // {
            // limit *= 0.2f;
            // }
            if (pawn.ageTracker != null && (factionColor > limit && pawn.ageTracker.AgeBiologicalYearsFloat > 16))
            {
                Color color2 = ArtificialHairColors.RandomElement();

                hairColor = Color.Lerp(hairColor, color2, Rand.Range(0.66f, 1f));
            }

            if (sameBeardColor)
            {
                beardColor = ShuffledBeardColor(hairColor);
            }
            else
            {
                Color color2 = GradientEuMelanin.Evaluate(hair.EuMelanin + Rand.Range(-0.5f, 0.5f));

                color2 *= GradientPheoMelanin.Evaluate(hair.PheoMelanin + Rand.Range(-0.5f, 0.5f));

                beardColor = Color.Lerp(color2, GrayHair, hair.Greyness * Rand.Value);
            }

            HairDNA dna = new HairDNA {HairColorRequest = hair, HairColor = hairColor, BeardColor = beardColor};

            return dna;
        }

        public static Color GetCurrentHairColor(this PawnFace face)
        {
            HairColorRequest colorRequest = new HairColorRequest(face.PheoMelanin, face.EuMelanin, face.Greyness);

            return GetHairColor(colorRequest);
        }

        public static Color GetHairColor(HairColorRequest hairColorRequest)
        {
            Color color = GradientEuMelanin.Evaluate(hairColorRequest.EuMelanin);

            color *= GradientPheoMelanin.Evaluate(hairColorRequest.PheoMelanin);

            // var cuticula = Mathf.Lerp(cuticulaRange.min, cuticulaRange.max, hairRequest.Cuticula);
            float greyness = Mathf.Lerp(GreyRange.min, GreyRange.max, hairColorRequest.Greyness);

            // Color.RGBToHSV(color, out float h, out float s, out float v);

            // s *= cuticula;

            // color = Color.HSVToRGB(h, s, v);

            // limit the greyness to 70 %, else it's too much
            color = Color.Lerp(color, GrayHair, greyness);

            return color;
        }

        private static bool GetHairDNAByBlood([NotNull] Pawn pawn, ref HairColorRequest hairColor)
        {
            if (!pawn.relations.FamilyByBlood.Any())
            {
                return false;
            }

            Pawn relPawn =
            pawn.relations.FamilyByBlood.FirstOrDefault(
                                                        x => x.HasPawnFace());

            if (relPawn == null || !relPawn.GetPawnFace(out PawnFace pawnFace))
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            float melaninx1 = pawnFace.EuMelanin;
            float melaninx2 = pawnFace.PheoMelanin;

            // float maxbaldness = relatedPawn.PawnFace.Baldness.maxBaldness;
            hairColor.EuMelanin   = GetRandomFloatSimilarTo(melaninx1);
            hairColor.PheoMelanin = GetRandomFloatSimilarTo(melaninx2);

            // hairColor.Baldness.maxBaldness = (int)GetRandomFloatSimilarTo(maxbaldness, 0f, 10f);

            // hair.Cuticula = GetRandomFloatSimilarTo(cuticulax);
            return true;
        }

        private static float GetRandomChildFloatValue(float fatherMelanin, float motherMelanin)
        {
            float clampMin = Mathf.Min(fatherMelanin, motherMelanin);
            float clampMax = Mathf.Max(fatherMelanin, motherMelanin);
            float value    = (fatherMelanin + motherMelanin) / 2f;
            return GetRandomFloatSimilarTo(value, clampMin, clampMax);
        }

        private static float GetRandomFloatSimilarTo(float value, float clampMin = 0f, float clampMax = 1f)
        {
            return Mathf.Clamp01(Mathf.Clamp(Rand.Gaussian(value, 0.15f), clampMin, clampMax));
        }

        private static void GetRandomizedMelaninAndCuticula(ref HairColorRequest hairColor)
        {
            hairColor.PheoMelanin = Rand.Range(0f, 1f);
            hairColor.EuMelanin   = Rand.Range(0f, 1f);

            // hairColor.Baldness.maxBaldness = (int)Rand.Range(0f, 10f);
        }

        [CanBeNull]
        private static PawnFace GetFatherFace(Pawn pawn)
        {
            PawnFace face   = null;
            Pawn     father = pawn.GetFather();
            father?.GetPawnFace(out face);
            return face;
        }

        [CanBeNull]
        private static PawnFace GetMotherFace([NotNull] Pawn pawn)
        {
            PawnFace face   = null;
            Pawn     mother = pawn.GetMother();
            mother?.GetPawnFace(out face);
            return face;
        }

        private static void SetInitialHairMelaninLevels(
        Pawn                 pawn,
        out HairColorRequest hairColor,
        bool                 ignoreRelative = false)
        {
            hairColor = new HairColorRequest(0f, 0f, 0f);
            bool flag = false;
            if (!ignoreRelative)
            {
                PawnFace motherPawnFace = GetMotherFace(pawn);
                bool     hasMother      = motherPawnFace != null;
                PawnFace fatherPawnFace = GetFatherFace(pawn);
                bool     hasFather      = fatherPawnFace != null;

                if (hasMother && hasFather)
                {
                    hairColor.EuMelanin =

                    // ReSharper disable once PossibleNullReferenceException
                    GetRandomChildFloatValue(motherPawnFace.EuMelanin, fatherPawnFace.EuMelanin);
                    hairColor.PheoMelanin =
                    GetRandomChildFloatValue(motherPawnFace.PheoMelanin, fatherPawnFace.PheoMelanin);

                    // hairColor.Baldness.maxBaldness = (int)GetRandomChildFloatValue(motherPawnFace.Baldness.maxBaldness, fatherPawnFace.Baldness.maxBaldness);
                    flag = true;
                }
                else if (hasMother)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    hairColor.EuMelanin   = GetRandomFloatSimilarTo(motherPawnFace.EuMelanin);
                    hairColor.PheoMelanin = GetRandomFloatSimilarTo(motherPawnFace.PheoMelanin);

                    // hairColor.Baldness.maxBaldness = (int)GetRandomFloatSimilarTo(motherPawnFace.Baldness.maxBaldness, 0f, 10f);
                    flag = true;
                }
                else if (hasFather)
                {
                    hairColor.EuMelanin   = GetRandomFloatSimilarTo(fatherPawnFace.EuMelanin);
                    hairColor.PheoMelanin = GetRandomFloatSimilarTo(fatherPawnFace.PheoMelanin);

                    // hairColor.Baldness.maxBaldness = (int)GetRandomFloatSimilarTo(fatherPawnFace.Baldness.maxBaldness, 0f, 10f);
                    flag = true;
                }

                // Check for relatives, else randomize
                if (!flag && GetHairDNAByBlood(pawn, ref hairColor))
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                GetRandomizedMelaninAndCuticula(ref hairColor);
            }

            // hairColor.Baldness.currentBaldness = Rand.Range(0, hairColor.Baldness.maxBaldness);
        }

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
                    bool hasMother = false;
                    bool hasFather = false;

                    if (pawn.GetMother() == null)
                    {
                        hasMother = true;
                    }
                    else
                    {
                        mother = pawn.GetMother().TryGetComp<CompFace>();
                    }

                    if (pawn.GetFather() == null)
                    {
                        hasFather = true;
                    }
                    else
                    {
                        father = pawn.GetFather().TryGetComp<CompFace>();
                    }

                    bool flag = true;

                    if (!hasMother && mother.IsSkinDNAoptimized && !hasFather && father.IsSkinDNAoptimized)
                    {
                        factionMelanin = GetRandomChildFloatValue(mother.FactionMelanin, father.FactionMelanin);
                    }
                    else if (!hasMother && mother.IsSkinDNAoptimized)
                    {
                        factionMelanin = GetRandomFloatSimilarTo(mother.FactionMelanin);
                    }
                    else if (!hasFather && father.IsSkinDNAoptimized)
                    {
                        factionMelanin = GetRandomFloatSimilarTo(father.FactionMelanin);
                    }
                    else
                    {
                        // if (hasMother && hasFather)
                        if (pawn.relations.FamilyByBlood.Any())
                        {
                            Pawn relPawn =
                                pawn.relations.FamilyByBlood.FirstOrDefault(x => x.TryGetComp<CompFace>().IsSkinDNAoptimized);
                            if (relPawn != null)
                            {
                                CompFace relatedPawn = relPawn.TryGetComp<CompFace>();

                                float melaninx1 = relatedPawn.FactionMelanin;
                                factionMelanin = GetRandomFloatSimilarTo(melaninx1);
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

        // Deactivated for now, as there's no way to get the pawn's birth biome and full history, considered mostly racist
    }
}