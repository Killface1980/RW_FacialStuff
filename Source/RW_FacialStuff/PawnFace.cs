namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Genetics;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public struct PawnFace : IExposable
    {
        #region Public Fields

        public Color BeardColor;

        public BeardDef BeardDef;

        public BrowDef BrowDef;

        public CrownType CrownType;

        public bool DrawMouth;

        public bool HasSameBeardColor;

        public bool IsOptimized;

        public float Cuticula;

        public float Greyness;

        public float EuMelanin;

        public float PheoMelanin;

        public EyeDef EyeDef;

        public MoustacheDef MoustacheDef;

        public HeadType PawnHeadType;

        public WrinkleDef WrinkleDef;

        public Color HairColor;

        public Color HairColorOrg;

        // public float MelaninOrg;

        #endregion Public Fields

        #region Public Constructors

        public PawnFace(Pawn pawn, bool setColors = true)
        {
            FactionDef faction = pawn.Faction?.def;

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, faction);

            this.DrawMouth = true;

            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, faction);
            this.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn);
            PawnFaceChooser.RandomBeardDefFor(pawn, faction, out this.BeardDef, out this.MoustacheDef);
            this.HasSameBeardColor = Rand.Value > 0.3f;
            HairDNA hairDNA = HairMelanin.GenerateHairMelaninAndCuticula(pawn);
            this.EuMelanin = hairDNA.EuMelanin;
            this.PheoMelanin = hairDNA.PheoMelanin;
            this.Cuticula = hairDNA.Cuticula;
            this.Greyness = hairDNA.Greyness;
            this.HairColor = hairDNA.HairColor;
            this.HairColorOrg = hairDNA.HairColorOrg;
            this.BeardColor = hairDNA.BeardColor;

            this.CrownType = pawn.story.crownType;
            this.PawnHeadType = HeadType.Undefined;

            if (setColors)
            {
                pawn.story.hairColor = this.HairColor;
            }

            this.IsOptimized = true;

            // this.MelaninOrg = pawn.story.melanin;
        }

        #endregion Public Constructors

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");

            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Defs.Look(ref this.MoustacheDef, "MoustacheDef");

            Scribe_Values.Look(ref this.IsOptimized, "optimized");
            Scribe_Values.Look(ref this.DrawMouth, "drawMouth");

            Scribe_Values.Look(ref this.CrownType, "crownType");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.HasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.EuMelanin, "euMelanin");
            Scribe_Values.Look(ref this.PheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref this.Cuticula, "cuticula");

            Scribe_Values.Look(ref this.HairColor, "hairColor");
            Scribe_Values.Look(ref this.HairColorOrg, "hairColorOrg");

            // Scribe_Values.Look(ref this.MelaninOrg, "melaninOrg");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");
        }


    }
}