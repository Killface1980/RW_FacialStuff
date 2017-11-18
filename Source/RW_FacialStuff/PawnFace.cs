namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Genetics;
    using FacialStuff.Utilities;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnFace : IExposable
    {
        public Color BeardColor;

        [CanBeNull]
        public BeardDef BeardDef;

        [CanBeNull]
        public BrowDef BrowDef;

        public CrownType CrownType;

        // public float Cuticula;
        public bool DrawMouth = true;

        public float EuMelanin;

        [CanBeNull]
        public EyeDef EyeDef;

        public float Greyness;

        public Color HairColor;

        public bool HasSameBeardColor;

        [CanBeNull]
        public MoustacheDef MoustacheDef;

        public float PheoMelanin;

        [NotNull]
        public WrinkleDef WrinkleDef;

        public float wrinkleIntensity;

        public PawnFace([NotNull] Pawn pawn, FactionDef pawnFactionDef, bool newPawn = true)
        {
            this.DrawMouth = true;

            this.EyeDef = PawnFaceMaker.RandomEyeDefFor(pawn, pawnFactionDef);
            this.BrowDef = PawnFaceMaker.RandomBrowDefFor(pawn, pawnFactionDef);

            this.WrinkleDef = PawnFaceMaker.AssignWrinkleDefFor(pawn);
            this.HasSameBeardColor = Rand.Value > 0.3f;

            this.GenerateHairDNA(pawn, false, newPawn);

            this.CrownType = pawn.story.crownType;
            PawnFaceMaker.RandomBeardDefFor(pawn, pawnFactionDef, out this.BeardDef, out this.MoustacheDef);

            this.wrinkleIntensity = Mathf.InverseLerp(45f, 80f, pawn.ageTracker.AgeBiologicalYearsFloat);
            this.wrinkleIntensity -= pawn.story.melanin / 2;

            // this.MelaninOrg = pawn.story.melanin;
        }

        // public Baldness Baldness;
        public PawnFace()
        {
            // for RW to not bug out
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");

            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Defs.Look(ref this.MoustacheDef, "MoustacheDef");

            Scribe_Values.Look(ref this.DrawMouth, "drawMouth");

            Scribe_Values.Look(ref this.CrownType, "crownType");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.HasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.EuMelanin, "euMelanin");
            Scribe_Values.Look(ref this.PheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref this.Greyness, "greyness");

            Scribe_Values.Look(ref this.HairColor, "hairColor");
            Scribe_Values.Look(ref this.wrinkleIntensity, "wrinkles");

            // Scribe_Values.Look(ref this.MelaninOrg, "melaninOrg");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");
        }

        public void GenerateHairDNA([NotNull] Pawn pawn, bool ignoreRelative = false, bool newPawn = true)
        {
            HairDNA hairDNA = HairMelanin.GenerateHairMelaninAndCuticula(pawn, this.HasSameBeardColor, ignoreRelative);
            this.EuMelanin = hairDNA.HairColorRequest.EuMelanin;
            this.PheoMelanin = hairDNA.HairColorRequest.PheoMelanin;
            this.Greyness = hairDNA.HairColorRequest.Greyness;

            // this.Baldness = hairDNA.HairColorRequest.Baldness;
            if (newPawn)
            {
                this.HairColor = hairDNA.HairColor;
                this.BeardColor = hairDNA.BeardColor;
            }
            else
            {
                this.HairColor = pawn.story.hairColor;
                this.BeardColor = HairMelanin.DarkerBeardColor(this.HairColor);
            }
        }

        // public float MelaninOrg;
    }
}