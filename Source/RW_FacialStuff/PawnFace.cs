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
        public float Cuticula;
        public bool DrawMouth;
        public float EuMelanin;
        public EyeDef EyeDef;
        public bool HasSameBeardColor;
        public bool IsDNAoptimized;
        public bool IsOptimized;
        public MoustacheDef MoustacheDef;
        public HeadType PawnHeadType;
        public float PheoMelanin;
        public WrinkleDef WrinkleDef;

        public Pawn pawn;

        #endregion Public Fields

        #region Public Constructors

        public PawnFace(Pawn pawn)
        {
            this.pawn = pawn;
            FactionDef faction = pawn.Faction?.def;

            this.EyeDef = PawnFaceChooser.RandomEyeDefFor(pawn, faction);

            this.DrawMouth = true;

            this.BrowDef = PawnFaceChooser.RandomBrowDefFor(pawn, faction);
            this.WrinkleDef = PawnFaceChooser.AssignWrinkleDefFor(pawn);
            PawnFaceChooser.RandomBeardDefFor(pawn, faction, out this.BeardDef, out this.MoustacheDef);
            this.HasSameBeardColor = Rand.Value > 0.3f;
            HairMelanin.GenerateHairMelaninAndCuticula(
                pawn,
                out this.EuMelanin,
                out this.PheoMelanin,
                out this.Cuticula,
                out this.BeardColor);
            this.IsDNAoptimized = true;
            this.CrownType = pawn.story.crownType;
            this.PawnHeadType = HeadType.Undefined;

            this.IsOptimized = true;
        }

        #endregion Public Constructors

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.EyeDef, "EyeDef");
            Scribe_Defs.Look(ref this.BrowDef, "BrowDef");

            Scribe_Defs.Look(ref this.WrinkleDef, "WrinkleDef");
            Scribe_Defs.Look(ref this.BeardDef, "BeardDef");
            Scribe_Defs.Look(ref this.MoustacheDef, "MoustacheDef");

            Scribe_Values.Look(ref this.pawn, "pawn");
            Scribe_Values.Look(ref this.IsOptimized, "optimized");
            Scribe_Values.Look(ref this.DrawMouth, "drawMouth");

            Scribe_Values.Look(ref this.CrownType, "crownType");
            Scribe_Values.Look(ref this.BeardColor, "BeardColor");
            Scribe_Values.Look(ref this.HasSameBeardColor, "sameBeardColor");

            Scribe_Values.Look(ref this.EuMelanin, "euMelanin");
            Scribe_Values.Look(ref this.PheoMelanin, "pheoMelanin");
            Scribe_Values.Look(ref this.Cuticula, "cuticula");

            Scribe_Values.Look(ref this.IsDNAoptimized, "DNAoptimized");

            // Scribe_Values.Look(ref this.skinColorHex, "SkinColorHex");
            // Scribe_Values.Look(ref this.hairColorOrg, "HairColorOrg");
            // Scribe_Values.Look(ref this.factionMelanin, "factionMelanin");
            // Scribe_Values.Look(ref this.isSkinDNAoptimized, "IsSkinDNAoptimized");
            // Scribe_Values.Look(ref this.melaninOrg, "MelaninOrg");

        }
    }

}