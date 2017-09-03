namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Genetics;

    using JetBrains.Annotations;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class PawnFace : IExposable
    {
        public Color BeardColor;

        [NotNull]
        public BeardDef BeardDef;

        [NotNull]
        public BrowDef BrowDef;

        public CrownType CrownType;

        // public float Cuticula;
        public bool DrawMouth = true;

        public float EuMelanin;

        [NotNull]
        public EyeDef EyeDef;

        public float Greyness;

        public Color HairColor;

        public bool HasSameBeardColor;

        [CanBeNull]
        public MoustacheDef MoustacheDef;

        public float PheoMelanin;

        [NotNull]
        public WrinkleDef WrinkleDef;

        public float wrinkles = 0f;

        public PawnFace([NotNull] Pawn pawn, bool newPawn = true)
        {
            this.DrawMouth = true;
            FactionDef faction = pawn.Faction.def;

            this.EyeDef = PawnFaceMaker.RandomEyeDefFor(pawn, faction);
            this.BrowDef = PawnFaceMaker.RandomBrowDefFor(pawn, faction);

            this.WrinkleDef = PawnFaceMaker.AssignWrinkleDefFor(pawn);
            this.HasSameBeardColor = Rand.Value > 0.3f;

            this.GenerateHairDNA(pawn);

            this.CrownType = pawn.story.crownType;
            PawnFaceMaker.RandomBeardDefFor(pawn, faction, out this.BeardDef, out this.MoustacheDef);

            this.wrinkles = Mathf.InverseLerp(45f, 80f, pawn.ageTracker.AgeBiologicalYearsFloat);

            // this.MelaninOrg = pawn.story.melanin;
        }

        public void GenerateHairDNA([NotNull] Pawn pawn, bool ignoreRelative = false)
        {
            HairDNA hairDNA = HairMelanin.GenerateHairMelaninAndCuticula(pawn, this.HasSameBeardColor, ignoreRelative);
            this.EuMelanin = hairDNA.HairColorRequest.EuMelanin;
            this.PheoMelanin = hairDNA.HairColorRequest.PheoMelanin;
            this.Greyness = hairDNA.HairColorRequest.Greyness;

            this.HairColor = hairDNA.HairColor;
            this.BeardColor = hairDNA.BeardColor;
        }

        public PawnFace()
        {
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
            Scribe_Values.Look(ref this.wrinkles, "wrinkles");

            // Scribe_Values.Look(ref this.MelaninOrg, "melaninOrg");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");
        }

        // public float MelaninOrg;
    }
}